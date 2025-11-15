using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillRunner : MonoBehaviour
{
    public Dictionary<SkillType, List<DirectionalSkillSet>>   _skillTables;
    Func<MoveDir>                           _getDir;                        // Player쪽에서 _lastDir을 넘겨줄 콜백


    SkillAsset                              _currentSkill = null;           // 현재 스킬
    SkillAsset                              _expectedSkill = null;          // 콤보 기대 스킬
    float                                   _lockUntil = 0.0f;
    public float                            _comboUntil = 0.0f;
    MoveDir                                 _currentSkillDir = MoveDir.Down; // 마지막 스킬 방향
    MoveDir                                 _lastSkillDir = MoveDir.Down; // 마지막 스킬 방향

    public SkillType                        CurrentType { get; set; } = SkillType.Sword;

    public event Action                     OnSkillEnded;  // ← 종료 알림
    int                                     _castSerial = 0;
    Coroutine                               _coSkillEndToPlayer;
    Coroutine                               _coSkillEnd;
    public int                              _pressedCount = 0;


    // Runner가 어떤 스킬을 골랐는지 알려주기 → Player가 애니 재생
    public event Action<SkillAsset, MoveDir> OnSkillStarted;

    // Player에서 테이블/방향 콜백 주입
    public void Configure(Dictionary<SkillType, DirectionalSkillSet> table, Func<MoveDir> getDir)
    {
        if (_skillTables == null)
            _skillTables = new Dictionary<SkillType, List<DirectionalSkillSet>>();

        foreach (var kv in table)
        {
            List<DirectionalSkillSet> list;
            if (!_skillTables.TryGetValue(kv.Key, out list) || list == null)
            {
                list = new List<DirectionalSkillSet>();
                _skillTables[kv.Key] = list;
            }
            list.Add(kv.Value);
        }

        _getDir = getDir;
    }

    // 타입만 넘겨서 시도
    public bool TryPlay(SkillType type)
    {
        switch (type)
        {
            case SkillType.Sword:
                {
                    bool ret = PlaySwordSkill();
                    if (ret == true)
                    {
                        ++_pressedCount;
                        Debug.Log($"프레스 카운트 : {_pressedCount}");
                    }

                    return ret;
                }
                break;
            case SkillType.Arrow:
                {
                    return PlayArrowSkill();
                }
                break;
        }

        return false;
    }

    bool Cast(SkillAsset asset, MoveDir dir, bool isComboSkill = false)
    {
        // 락/콤보 만료 시각 세팅
        _lockUntil = Time.time + Mathf.Max(0f, asset._lockTime);
        _comboUntil = Time.time + Mathf.Max(0f, asset._comboInputWindow);

        // 다음 콤보 기대 스킬(없으면 null) 설정
        _currentSkill = asset;
        _expectedSkill = asset._nextOnCombo;

        _lastSkillDir = _currentSkillDir;
        _currentSkillDir = dir;

        OnSkillStarted?.Invoke(asset, dir);

        _castSerial++;
        if (_coSkillEndToPlayer != null)
            StopCoroutine(_coSkillEndToPlayer);

        if (isComboSkill == true)
        {
            if (_coSkillEnd != null)
                StopCoroutine(_coSkillEnd);
            _coSkillEnd = StartCoroutine(CoEndAfter(0.5f));
        }

        _coSkillEndToPlayer = StartCoroutine(CoEndAfterToPlayer(_castSerial, asset._lockTime));

        return true;
    }

    IEnumerator CoEndAfterToPlayer(int serial, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 캐스트가 중간에 다른 스킬로 바뀌었으면 무시
        if (serial != _castSerial)
            yield break;

        OnSkillEnded?.Invoke();
    }

    IEnumerator CoEndAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 락만 해제 (콤보 입력창은 자연 소멸되도록 유지)
        _lockUntil = 0.0f;
        _comboUntil = 0.0f;
        _pressedCount = 0;
    }

    // ---- 핵심: 현재 타입에서 DirectionalSkillSet 하나를 "찾아" 선택하는 헬퍼 ----
    // index 파라미터로 어떤 세트를 쓸지 결정 (기본 0: 첫 번째 세트)
    bool TryGetSet(SkillType type, out DirectionalSkillSet set, int index = 0)
    {
        set = null;
        if (_skillTables == null) return false;
        if (!_skillTables.TryGetValue(type, out var list) || list == null || list.Count == 0)
            return false;

        // 인덱스 보호: 범위를 벗어나면 0으로 보정 (원하면 다른 정책 적용 가능)
        if (index < 0 || index >= list.Count)
            index = 0;

        set = list[index];
        return set != null;
    }

    private bool PlaySwordSkill()
    {
        // Sword용 세트 하나 선택 (필요하면 인덱스를 상태/장비에 따라 바꾸세요)
        if (!TryGetSet(SkillType.Sword, out var set))
            return false;

        // 현재 입력 순간의 방향 스냅샷
        MoveDir nowDir = SafeDir();
        SkillAsset entrySkill = set.GetEntry(nowDir);

        // 방향이 같은 경우
        if (nowDir == _currentSkillDir)
        {
            if (_pressedCount == 0)
            {
                // 첫타
                Debug.Log("첫타");
                return Cast(entrySkill, nowDir, true);
            }
            else
            {
                // 락 걸려있는 시간보다는 크거나 같고 && 콤보 가능한 시간보다는 작거나 같은 경우 => 콤보가 가능하다.
                if (Time.time <= _comboUntil && _currentSkill._nextOnCombo != null)
                {
                    Debug.Log($"콤보 가능 {_pressedCount}");
                    return Cast(_currentSkill._nextOnCombo, nowDir, true);
                }
                else
                {
                    // 콤보가 불가능
                    Debug.Log("콤보 불가능");
                }
            }
        }
        else
        {
            // 이전에 스킬을 사용한 방향과 현재 방향이 다른경우 무조건 첫타 부터 실행한다.
            _pressedCount = 0;
            Debug.Log("방향을 바꾸고 첫타");
            return Cast(entrySkill, nowDir, true);
        }
        return false;
    }

    private bool PlayArrowSkill()
    {
        if (Time.time <= _lockUntil)
            return false;

        if (!TryGetSet(SkillType.Arrow, out var set))
            return false;

        // 현재 입력 순간의 방향 스냅샷
        MoveDir nowDir = SafeDir();
        SkillAsset entrySkill = set.GetEntry(nowDir);

        return Cast(entrySkill, nowDir);
    }

    MoveDir SafeDir() => _getDir != null ? _getDir() : MoveDir.Down;

    public void Cancel()
    {
        _expectedSkill = null;
        _lockUntil = _comboUntil = 0f;
    }

}
