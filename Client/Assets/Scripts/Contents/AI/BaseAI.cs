using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


/// AI가 컨트롤러에게 돌려주는 액션 종류
public enum AIActionType { None, Idle, Move, Attack }

/// AI가 결정한 결과(Controller는 이것만 반영)
public struct AIResult
{
    public AIActionType type;   // 무엇을 할지
    public Vector3Int nextCell;   // Move일 때 목적 셀(= path[1])
    public MoveDir faceDir;    // 바라볼 방향(이동/공격)
    public SkillType skillType;  // Attack 시 어떤 스킬로
}

/// AI 입력 컨텍스트(외부에서 주입)
public struct AIContext
{
    public Vector3Int SelfCell;              // 내 현재 셀
    public Vector3Int? TargetCell;            // 타겟(플레이어 등) 셀 (없으면 null)
    public Vector3Int PatrolDest;            // 패트롤 목적지
    public bool HasTarget => TargetCell.HasValue;
}

/// 경로 탐색 포트(서버/클라 각각 구현)
public interface IPathfinder
{
    List<Vector3Int> FindPath(Vector3Int start, Vector3Int dest, bool ignoreDestCollision);
}

/// 월드 질의 포트(충돌/점유/타겟 탐지 등은 필요 시 확장)
public interface IWorldQuery
{
    bool CanGo(Vector3Int cell);
    bool IsOccupied(Vector3Int cell);
}

public abstract class BaseAI
{
    protected readonly IPathfinder _pf;
    protected readonly IWorldQuery _world;

    protected readonly float _leash;      // 리쉬(경로 길이) 제한
    protected readonly float _skillRange; // 근접 스킬 사거리(격자 기준)

    // 경로 캐시(디버깅/최적화에 사용 가능)
    protected List<Vector3Int> _path;

    protected BaseAI(IPathfinder pf, IWorldQuery world, float leash, float skillRange)
    {
        _pf = pf;
        _world = world;
        _leash = leash;
        _skillRange = skillRange;
    }

    /// 외부에서 매 틱 호출: 입력 컨텍스트 → 결과
    public abstract AIResult Decide(in AIContext ctx);

    /// 목적지로 A* 경로를 구하고 다음 한 칸(step)을 반환
    /// - 경로 없음/도착 또는 리쉬 초과면 false
    protected bool TryStep(Vector3Int self, Vector3Int dest, bool hasTarget, out Vector3Int next, out MoveDir dir)
    {
        next = self;
        dir = MoveDir.None;

        _path = _pf.FindPath(self, dest, ignoreDestCollision: true);
        if (_path == null || _path.Count < 2) 
            return false;               // 경로 없음 or 이미 도착
        if (hasTarget && _path.Count > _leash) 
            return false;               // 추적 리쉬 초과

        next = _path[1];
        dir = ToDir(next - self);
        return true;
    }

    /// 직선 정렬(상하좌우) 여부
    protected static bool IsAligned(Vector3Int d) => (d.x == 0 || d.y == 0);

    /// 벡터 → 방향
    protected static MoveDir ToDir(Vector3Int d)
    {
        if (d.x > 0) return MoveDir.Right;
        if (d.x < 0) return MoveDir.Left;
        if (d.y > 0) return MoveDir.Up;
        if (d.y < 0) return MoveDir.Down;
        return MoveDir.None;
    }
}
