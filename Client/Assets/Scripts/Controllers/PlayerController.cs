using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : BaseController
{
    protected SkillRunner           _skillRunner;
    DirectionalSkillSet             _swordSet;
    DirectionalSkillSet             _ArrowSet;

    protected override void Init()
    {
        base.Init();

        // Skill
        {
            _skillRunner = gameObject.AddComponent<SkillRunner>();

            // Sword
            {
                var r1 = SetSkillAsset("ATTACK_RIGHT_01", 0.25f, 0.5f);
                var r2 = SetSkillAsset("ATTACK_RIGHT_02", 0.25f, 0.5f);
                var r3 = SetSkillAsset("ATTACK_RIGHT_03", 0.25f, 0.5f);
                r1._nextOnCombo = r2; r2._nextOnCombo = r3;

                var f1 = SetSkillAsset("ATTACK_FRONT_01", 0.25f, 0.5f);
                var f2 = SetSkillAsset("ATTACK_FRONT_02", 0.25f, 0.5f);
                var f3 = SetSkillAsset("ATTACK_FRONT_03", 0.25f, 0.5f);
                f1._nextOnCombo = f2; f2._nextOnCombo = f3;

                var b1 = SetSkillAsset("ATTACK_BACK_01", 0.25f, 0.5f);
                var b2 = SetSkillAsset("ATTACK_BACK_02", 0.25f, 0.5f);
                var b3 = SetSkillAsset("ATTACK_BACK_03", 0.25f, 0.5f);
                b1._nextOnCombo = b2; b2._nextOnCombo = b3;

                _swordSet = new DirectionalSkillSet { _right = r1, _front = f1, _back = b1, _left = null };

                var table = new Dictionary<SkillType, DirectionalSkillSet> { { SkillType.Sword, _swordSet } };

                _skillRunner.Configure(table, () => _lastDir);
                _skillRunner.CurrentType = SkillType.Sword;

                _skillRunner.OnSkillStarted += (asset, dir) => PlayAttackDirectional(asset);
                _skillRunner.OnSkillEnded += () =>
                {
                    if (State == CState.Skill)
                    {
                        State = CState.Idle;
                        CheckUpdatedFlag();
                    }
                };
            }

            // Arrow
            {
                var r1 = SetSkillAsset("ATTACK_ARROW_RIGHT", 0.5f, 0.5f);
                var f1 = SetSkillAsset("ATTACK_ARROW_FRONT", 0.5f, 0.5f);
                var b1 = SetSkillAsset("ATTACK_ARROW_BACK", 0.5f, 0.5f);
                _ArrowSet = new DirectionalSkillSet { _right = r1, _front = f1, _back = b1, _left = null };
                var table = new Dictionary<SkillType, DirectionalSkillSet> { { SkillType.Arrow, _ArrowSet } };
                _skillRunner.Configure(table, () => _lastDir);

                _skillRunner.OnSkillStarted += (asset, dir) => PlayAttackDirectional(asset);
                _skillRunner.OnSkillEnded += () =>
                {
                    if (State == CState.Skill)
                    {
                        State = CState.Idle;

                    }
                };
            }
        }
    }

    protected override void UpdateAnimation()
    {
        if (State == CState.Idle)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.None:
                    break;
            }
        }
        else if (State == CState.Skill)
        {

        }
        else if (State == CState.Dead)
        {
            _animator.Play("DEAD");
        }
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    void PlayAttackDirectional(SkillAsset asset)
    {
        _sprite.flipX = (_lastDir == MoveDir.Left);
        if (!string.IsNullOrEmpty(asset._animName))
            _animator.Play(asset._animName);
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (Dir != MoveDir.None)
        {
            State = CState.Moving;
            return;
        }
    }

    public void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            // Skill 사용
            // 현재 타입으로 자동 선택(방향/콤보 포함)
            if (_skillRunner.TryPlay(SkillType.Sword))
            {
                _updated = true;
                State = CState.Skill;
            }
        }
    }

    protected virtual void CheckUpdatedFlag()
    {

    }
}
