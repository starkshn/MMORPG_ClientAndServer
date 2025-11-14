using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BaseController : MonoBehaviour
{
    public float                _speed = 5.0f;
    public Vector3Int           CellPos { get; set; }  = Vector3Int.zero;

    protected Animator          _animator;
    protected CState            _state = CState.Idle;
    protected SpriteRenderer    _sprite = null;
    
    public virtual CState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;

            UpdateAnimation();
        }
    }

    protected MoveDir   _lastDir = MoveDir.Down;
    protected MoveDir   _dir = MoveDir.Down;

    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;

            _dir = value;
            if (value != MoveDir.None)
                _lastDir = value;

            UpdateAnimation();
        }
    }

    public Vector3Int GetFrontCellPos()
    { 
        Vector3Int cellPos = CellPos; 
        
        switch(_lastDir)
        {
            case MoveDir.Up:
                cellPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPos += Vector3Int.right;
                break;
        }

        return cellPos;
    }

    public MoveDir GetDirFromVec(Vector3Int dir)
    {
        if (dir.x > 0)
            return MoveDir.Right;
        else if (dir.x < 0)
            return MoveDir.Left;
        else if (dir.y > 0)
            return MoveDir.Up;
        else if (dir.y < 0)
            return MoveDir.Down;
        else
            return MoveDir.None;
    }

    protected virtual void UpdateAnimation()
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
            switch (_dir)
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

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 worldPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = worldPos;
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CState.Idle:
                UpdateIdle();
                break;
            case CState.Moving:
                UpdateMoving();
                break;
            case CState.Skill:
                UpdateSkill();
                break;
            case CState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
        if (State == CState.Idle && _dir != MoveDir.None)
        {
            Vector3Int destPos = CellPos;

            switch (_dir)
            {
                case MoveDir.Up:
                    destPos += Vector3Int.up;
                    break;
                case MoveDir.Down:
                    destPos += Vector3Int.down;
                    break;
                case MoveDir.Left:
                    destPos += Vector3Int.left;
                    break;
                case MoveDir.Right:
                    destPos += Vector3Int.right;
                    break;
            }

            bool canAdvance = Managers.Map.CanGo(destPos) && Managers.Obj.Find(destPos) == null;

            if (canAdvance)
                State = CState.Moving;
            else
            {
                State = CState.Idle;
                Dir = MoveDir.None;
            }
        }
    }

    protected virtual void UpdateMoving()
    {
        Vector3 worldDestPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = worldDestPos - transform.position;

        // 도착 여부를 확인한다.
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = worldDestPos;
            MoveToNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CState.Moving;
        }
    }

    protected virtual void MoveToNextPos()
    {
        if (_dir == MoveDir.None)
        {
            State = CState.Idle;
            return;
        }

        Vector3Int destPos = CellPos;

        switch (_dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Obj.Find(destPos) == null)
            {
                CellPos = destPos;
            }
        }
    }

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {
        
    }

    public virtual void OnDamaged()
    {
        
    }

    #region "스킬 설정"

    protected SkillAsset SetSkillAsset(string animName, float lockTime, float comboInputWindow)
    {
        var r1 = ScriptableObject.CreateInstance<SkillAsset>(); 
        r1._animName = animName; 
        r1._lockTime = lockTime; 
        r1._comboInputWindow = 0.5f;
        return r1;
    }
    
    #endregion
}
