using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class MonsterController : BaseController
{
    SkillRunner                 _skillRunner;
    DirectionalSkillSet         _swordSet;


    Coroutine _coPatrol;
    Coroutine                   _coSearch;
    Vector3Int                  _destCellPos;

    [SerializeField]
    GameObject                  _target;

    [SerializeField]
    float                       _searchRange = 5.0f;

    [SerializeField]
    float                       _skillRange = 1.0f;

    public override CState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            base.State = value;

            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        State = CState.Idle;
        Dir = MoveDir.Down;

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
                if (State == CState.Skill) State = CState.Idle;
            };
        }
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

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }

        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    protected override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        if (_target != null)
        {
            destPos = _target.GetComponent<BaseController>().CellPos;

            Vector3Int dir = destPos - CellPos;
            if (dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                Dir = GetDirFromVec(dir);
                if (_skillRunner.TryPlay(SkillType.SkillSword))
                    State = CState.Skill;
                return;
            }
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target != null && path.Count > 10))
        {
            _target = null;
            State = CState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];
        Vector3Int moveCellDir = nextPos - CellPos;

        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;

        if (Managers.Map.CanGo(nextPos) && Managers.Obj.Find(nextPos) == null)
        {
            CellPos = nextPos;
        }
        else
        {
            State = CState.Idle;
        }
    }

    public override void OnDamaged()
    {
        // 화살인 경우 예외적으로 effect 처리를 해주어야한다. -> todo
        {
            GameObject effect = Managers.Resource.Instantiate("Effect/Arrow_Effect");
            effect.transform.position = transform.position;
            effect.GetComponent<Animator>().Play("Anim");
            GameObject.Destroy(effect, 0.35f);
        }

        // 충돌 범위에서 제외
        Managers.Obj.Remove(Id);

        State = CState.Dead;

        Animator animator = GetComponent<Animator>();
        StartCoroutine(DisableAfter(animator, "DEAD"));
    }

    IEnumerator DisableAfter(Animator anim, string name)
    {
        float len = anim.runtimeAnimatorController.animationClips
            .First(c => c.name == name).length;
        yield return new WaitForSeconds(len + 0.1f);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; i++)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanGo(randPos) && Managers.Obj.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CState.Moving;
                yield break;
            }
        }

        State = CState.Idle;
    }

    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (_target != null)
                continue;

            _target = Managers.Obj.Find((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null)
                    return false;

                Vector3Int dir = (pc.CellPos - CellPos);
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }
}
