using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillAsset : ScriptableObject
{
    [Header("Animator Trigger")]
    public string _animName;

    [Header("Lock/Combo")]
    public float _comboInputWindow = 0.5f;
    public float _lockTime = 0.25f;
    public SkillAsset _nextOnCombo = null;
}

[System.Serializable]
public class DirectionalSkillSet
{
    public SkillAsset _front; // ATTACK_FRONT_01 (-> _02 -> _03 체인)
    public SkillAsset _back;  // ATTACK_BACK_01
    public SkillAsset _right; // ATTACK_RIGHT_01
    public SkillAsset _left;  // 없으면 right 재사용 + flipX

    public SkillAsset GetEntry(MoveDir dir)
    {
        switch (dir)
        {
            case MoveDir.Up: return _back ?? _front ?? _right ?? _left;
            case MoveDir.Down: return _front ?? _right ?? _left ?? _back;
            case MoveDir.Right: return _right ?? _front ?? _back ?? _left;
            case MoveDir.Left: return _left ?? _right ?? _front ?? _back;
            default: return _front ?? _right ?? _back ?? _left;
        }
    }
}