using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterAI : BaseAI
{
    private readonly SkillType _defaultSkill; // 이 몬스터가 사용할 스킬(예: Sword)

    public MonsterAI(IPathfinder pf, IWorldQuery world, float leash, float skillRange, SkillType defaultSkill)
        : base(pf, world, leash, skillRange)
    {
        _defaultSkill = defaultSkill;
    }

    public override AIResult Decide(in AIContext ctx)
    {
        // 1) 목적지 선정
        Vector3Int dest = ctx.HasTarget ? ctx.TargetCell.Value : ctx.PatrolDest;

        // 2) 공격 기회: 타겟이 있고, 직선 정렬 + 사거리 이내면 공격
        if (ctx.HasTarget)
        {
            Vector3Int d = dest - ctx.SelfCell;
            if (IsAligned(d) && d.magnitude <= _skillRange)
            {
                return new AIResult
                {
                    type = AIActionType.Attack,
                    faceDir = ToDir(d),
                    skillType = _defaultSkill
                };
            }
        }

        // 3) 이동 시도: 추적 또는 패트롤
        if (TryStep(ctx.SelfCell, dest, ctx.HasTarget, out var next, out var face))
        {
            // 다음 칸이 실제로 이동 가능한지(월드 충돌) 최종 확인
            if (_world.CanGo(next) && _world.IsOccupied(next) == false)
            {
                return new AIResult
                {
                    type = AIActionType.Move,
                    nextCell = next,
                    faceDir = face
                };
            }
        }

        // 4) 실패(경로 없음/리쉬 초과/충돌 등) → Idle (컨트롤러에서 Dir=None으로 리셋)
        return new AIResult { type = AIActionType.Idle };
    }
}
