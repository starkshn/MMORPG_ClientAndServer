using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CController : BaseController
{
    HpBar           _hpBar;
    Coroutine       _coHit;
    Coroutine       _coDead;

    public override StatInfo Stat
    {
        get { return base.Stat; }
        set { base.Stat = value; UpdateHpBar(); }
    }

    public override int Hp
    {
        get { return Stat.Hp; }
        set { base.Hp = value; UpdateHpBar(); }
    }

    protected void AddHpBar()
    {
        GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
        go.transform.localPosition = new Vector3(0, 1.0f, 0f);
        go.name = "HpBar";
        _hpBar = go.GetComponent<HpBar>();
        UpdateHpBar();
    }

    void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = 0.0f;
        if (Stat.MaxHp > 0)
        {
            // float 가 우선순위가 높다
            ratio = ((float)Hp / Stat.MaxHp);
        }

        _hpBar.SetHpBar(ratio);
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    protected override void Init()
    {
        base.Init();
        AddHpBar();
    }

    public override void OnDamaged()
    {
        Debug.Log("Damaged!");
    }

    public virtual void OnDead()
    {
        State = CState.Dead;
        Debug.Log("Dead!");
    }

    protected override void UpdateHit()
    {
        if (_coHit != null)
            return;

        _coHit = StartCoroutine(CoHitRoutine());
    }
    
    protected override void UpdateDead()
    {
        if (_coDead != null)
            return;

        _coDead = StartCoroutine(CoDeadRoutine());
    }

    protected override void UpdateEmote()
    {
        
    }

    IEnumerator CoHitRoutine()
    {

        // 0.2초 동안 Hit 애니 유지
        yield return new WaitForSeconds(0.2f);

        State = CState.Idle;
        _coHit = null;
    }


    IEnumerator CoDeadRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // State = CState.Idle;
        _coDead = null;
        
        // TODO
    }

    public virtual void UseSkill(int skillId)
    {

    }
}
