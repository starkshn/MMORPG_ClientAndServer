using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class MonsterController : CController
{
    SkillRunner                 _skillRunner;
    DirectionalSkillSet         _swordSet;

    protected override void Init()
    {
        base.Init();

        _skillRunner = gameObject.AddComponent<SkillRunner>();
        {
            var r1 = SetSkillAsset("ATTACK_RIGHT", 0.25f, 0.5f);
            var f1 = SetSkillAsset("ATTACK_FRONT", 0.25f, 0.5f);
            var b1 = SetSkillAsset("ATTACK_BACK", 0.25f, 0.5f);
            _swordSet = new DirectionalSkillSet { _right = r1, _front = f1, _back = b1, _left = null };

            var table = new Dictionary<SkillType, DirectionalSkillSet> { { SkillType.SkillSword, _swordSet } };

            _skillRunner.Configure(table, () => Dir);
            _skillRunner.CurrentType = SkillType.SkillSword;

            _skillRunner.OnSkillStarted += (asset, dir) => PlayAttackDirectional(asset);
            _skillRunner.OnSkillEnded += () =>
            {
                if (State == CState.Skill) 
                    State = CState.Idle;
            };
        }

        // _skillRunner._lockUntil = 1.5f;
    }

    void PlayAttackDirectional(SkillAsset asset)
    {
        _sprite.flipX = (Dir == MoveDir.Left);
        if (!string.IsNullOrEmpty(asset._animName))
            _animator.Play(asset._animName);
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }

    public override void OnDamaged()
    {

    }

    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            // Skill 사용
            // 현재 타입으로 자동 선택(방향/콤보 포함)
            if (_skillRunner.TryPlay(SkillType.SkillSword))
            {
                _updated = true;
                State = CState.Skill;
            }
        }
    }
}
