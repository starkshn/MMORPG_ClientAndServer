using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {

        switch(State)
        {
            case Define.CState.Idle:
                GetDirInput();
                break;
            case Define.CState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (Dir != MoveDir.None)
        {
            State = CState.Moving;
            return;
        }

        GetSkillInput();
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    void GetSkillInput()
    {
        // 칼을 휘두르는 스킬을 사용하는 경우
        // Idle일 때만, 스킬이 사용가능해서 사용하는 경우
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_skillRunner.TryPlay(SkillType.Sword)) // 현재 타입으로 자동 선택(방향/콤보 포함)
                State = CState.Skill;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (_skillRunner.TryPlay(SkillType.Arrow))
            {
                State = CState.Skill;

                GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
                ArrowController ac = go.GetComponent<ArrowController>();
                ac.Dir = _lastDir;
                ac.CellPos = CellPos;
            }
        }
    }
}
