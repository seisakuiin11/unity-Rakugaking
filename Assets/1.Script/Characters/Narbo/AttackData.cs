using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject
{



    public int AtkDamage;

    public int AtkHitStanFrame;

    [Tooltip("ノックバック情報")]
    public KnockBackData KnockBackData;

    [Tooltip("攻撃の全体フレーム")]
    public int AllFrame;
    public List<FrameData> data;

}

[Serializable]
public struct FrameData
{
    public int TargetFrame;
    public List<CircleColData> colliders;
}

[Serializable]
public struct KnockBackData
{

    [Tooltip("True:KnockBackVecに飛ぶ False:当たり判定の中心点からのベクトルで飛ぶ")]
    public bool SpecifyDirectionFlag;

    [Tooltip("※SpecifyDirectionFlagがTrueの時のみ有効  吹っ飛ぶベクトル")]
    public Vector2 KnockBackVec;

    [Tooltip("ノックバックする力")]
    public float KnockBackPower;

}

