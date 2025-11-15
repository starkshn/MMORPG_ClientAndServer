using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // 맵 로드
        {
            Managers.Map.LoadMap(1);
        }

        Screen.SetResolution(640, 480, false);

        // 오브젝트 로드
        {
            // 나중에 패킷을 받아서 핸들러에서 생성할 것인데 우선 수동으로 만든다.
            //GameObject player = Managers.Resource.Instantiate("Creature/Player");
            //player.name = "Player";
            //Managers.Obj.Add(player);

            //for (int i = 0; i < 1; ++i)
            //{
            //    GameObject monster = Managers.Resource.Instantiate("Creature/Monster_Skeleton_Swordman");
            //    monster.name = $"Monster_{i + 1}";

            //    Vector3Int pos = new Vector3Int
            //    {
            //        x = Random.Range(-5, 5),
            //        y = Random.Range(-5, 5),
            //    };

            //    MonsterController mc = monster.GetComponent<MonsterController>();
            //    mc.CellPos = pos;
            //    Managers.Obj.Add(monster);
            //}
        }
    }

    public override void Clear()
    {
        
    }
}
