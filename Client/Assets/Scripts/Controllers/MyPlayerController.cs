using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    bool            _moveKeyPressed = false;
    int             _chatPressedCount = 0;
    bool            _chatFlag = false;
    
    protected override void Init()
    {
        base.Init();

        // 채팅 전송 콜백 등록
        if (_chattingController != null)
        {
            _chattingController.OnSubmitChat = HandleChatSubmit;
        }
    }

    void HandleChatSubmit(string msg)
    {
        // 이 함수는 ChattingController가 Enter 눌렀을 때 불러줌
        if (string.IsNullOrEmpty(msg))
            return;

        // 1) 서버로 패킷 보내기
        // C_Chat chatPacket = new C_Chat();
        // chatPacket.Message = msg;
        // Managers.Network.Send(chatPacket);

        // 2) 내 머리 위 말풍선 띄우고 싶다면
        // _emoteController.ShowChat(msg);
    }

    protected override void UpdateController()
    {
        if (_chatFlag)
        {
            GetChatInput();
        }
        else
        {
            switch (State)
            {
                case CState.Idle:
                    GetDirInput();
                    GetChatInput();
                    break;
                case CState.Moving:
                    GetDirInput();
                    break;
            }

            base.UpdateController();
        }
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed == true)
        {
            State = CState.Moving;
            return;
        }

        // MyCode
        if (_coSkillCooltime == null && Input.GetKeyDown(KeyCode.Space))
        {
            // 마지막 콤보인경우
            if (_skillRunner._pressedCount == 3)
            {
                CoInputCooltime(_skillRunner._comboUntil + 0.1f);
                return;
            }

            Debug.Log("Skill Sword!");

            GetSkillInput();

            // 스킬 쿨 타임, 키 무한 입력 방지
            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.1f);
        }
        else if (_coSkillCooltime == null && Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Skill Arrow!");

            GetSkillInput();

            // 스킬 쿨 타임, 키 무한 입력 방지
            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.5f);
        }
    }

    Coroutine _coSkillCooltime;
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCooltime = null;
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetDirInput()
    {
        _moveKeyPressed = true;

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
            _moveKeyPressed = false;
        }
    }

    void GetSkillInput()
    {
        // 칼을 휘두르는 스킬을 사용하는 경우
        // Idle일 때만, 스킬이 사용가능해서 사용하는 경우
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //if (_skillRunner.TryPlay(SkillType.Sword)) // 현재 타입으로 자동 선택(방향/콤보 포함)
            //    State = CState.Skill;

            C_Skill skill = new C_Skill()
            {
                Info = new SkillInfo()
            };
            skill.Info.SkillId = 1;
            Managers.Network.Send(skill);

        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            {
                // if (_skillRunner.TryPlay(SkillType.Arrow))
                // {
                //     State = CState.Skill;
                // 
                //     // TODO
                //     {
                //         GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
                //         ArrowController ac = go.GetComponent<ArrowController>();
                //         ac.Dir = _lastDir;
                //         ac.CellPos = CellPos;
                //     }
                // }
            }
            // 화살 스킬
            C_Skill skill = new C_Skill()
            {
                Info = new SkillInfo()
            };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill);
        }
    }

    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            State = CState.Idle;
            CheckUpdatedFlag();
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
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
            if (Managers.Obj.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated == true)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    protected override void UpdateEmote()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // todo packet 보내기
            _emoteController.PlayEmote(1, 1.0f);
            
            C_Emote emotePacket = new C_Emote();
            emotePacket.ObjectId = Id;
            emotePacket.EmoteId = 1;
            Managers.Network.Send(emotePacket);
        }
    }

    private void GetChatInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _chatPressedCount = (_chatPressedCount + 1) % 2;
            if (_chatPressedCount == 1)         // 채팅 하려고하는 경우
            {
                // 뭔가 여러 설정을 해주고 키 입력을 받을 수 있게 해주어야한다.


                Debug.Log("Enter ChatMode");
                _chatFlag = true;
                _chattingController.EnterChatMode();
            }
            else if (_chatPressedCount == 0)     // 채팅 보내는 경우
            {
                Debug.Log("Exit ChatMode");

                // 보낸다.
                _chatFlag = false;
                _chattingController.ExitChatMode();

            }
            else
            {

            }
        }
        else
        {
            // 여기서 문자열 입력을 받는다.
            
        }
    }
}
