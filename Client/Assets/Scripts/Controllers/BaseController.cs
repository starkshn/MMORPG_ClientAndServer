using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.Hp = value.Hp;
            _stat.MaxHp = value.MaxHp;
            _stat.Speed = value.Speed;
        }
    }

    public float Speed
    {
        get { return Stat.Speed; }
        set { Stat.Speed = value; }
    }

    public virtual int Hp
    {
        get { return Stat.Hp; }
        set
        {
            Stat.Hp = value;
        }
    }

    protected bool              _updated = false;
    protected Animator          _animator;
    protected SpriteRenderer    _sprite = null;
    PositionInfo                _positionInfo = new PositionInfo();

    [SerializeField]
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            CellPos = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
        }
    }

    public void SyncPos()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = destPos;
    }

    public Vector3Int CellPos
    { 
        get
        {
            return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0);
        }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }

    [SerializeField]
    public virtual CState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;

            UpdateAnimation();

            _updated = true;
        }
    }

    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public Vector3Int GetFrontCellPos()
    { 
        Vector3Int cellPos = CellPos; 
        
        switch (Dir)
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
        else
            return MoveDir.Down;
    }

    protected virtual void UpdateAnimation()
    {
        if (_animator == null || _sprite == null)
            return;

        if (State == CState.Idle)
        {
            switch (Dir)
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
            }
        }
        else if (State == CState.Skill)
        {
            
        }
        else if (State == CState.Hit)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("HIT_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("HIT_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("HIT_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("HIT_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CState.Dead)
        {
            _animator.Play("DEAD");
            _sprite.flipX = false;
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

        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CState.Idle:
                UpdateIdle();
                UpdateEmote();
                break;
            case CState.Moving:
                UpdateMoving();
                UpdateEmote();
                break;
            case CState.Skill:
                UpdateSkill();
                UpdateEmote();
                break;
            case CState.Hit:
                UpdateHit();
                UpdateEmote();
                break;
            case CState.Dead:
                UpdateDead();
                UpdateEmote();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
        
    }

    protected virtual void UpdateMoving()
    {
        Vector3 worldDestPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = worldDestPos - transform.position;

        // 도착 여부를 확인한다.
        float dist = moveDir.magnitude;

        // 거의다 도착한 경우
        if (dist < Speed * Time.deltaTime)
        {
            transform.position = worldDestPos;
            MoveToNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
            State = CState.Moving;
        }
    }

    protected virtual void UpdateHit()
    {
        
    }


    protected virtual void MoveToNextPos()
    {
        
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

    protected virtual void UpdateEmote()
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
