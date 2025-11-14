using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{

    public enum SkillType { Sword, Arrow, /*, Water, Chop, Mine, Arrow ...*/ }

    // CreatureState
    public enum CState
    {
        Idle,
        Moving, 
        Skill,



        Dead,
    }

    public enum MoveDir
    {
        None,
        Up, 
        Down,
        Left,
        Right,
    }


    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
}
