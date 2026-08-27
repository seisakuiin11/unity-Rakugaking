using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/AttackData")]
public class AttackData : ScriptableObject
{



    public int AtkDamage;

    public int AtkHitStanFrame;

    [Tooltip("ノックバック情報")]
    public List<KnockBackData> KnockBackData;

    [Tooltip("攻撃の全体フレーム")]
    public int AllFrame;
    public List<FrameData> data;

    [Tooltip("個別の変数")]
    public　List<OriginalParameter> palameters;

    [Tooltip("飛び道具などの召喚系プレハブ")]
    public List<GameObject> prefabs;

}

[Serializable]
public struct OriginalParameter
{
    [Tooltip("変数名")]
    public string name;
    [Tooltip("値")]
    public float value;
}


[Serializable]
public struct FrameData
{
    public int TargetFrame;
    public int KnockBackNum;
    public List<CircleColData> colliders;
}

[Serializable]
public struct KnockBackData
{

    [Tooltip("ベクトルの基準")]
    public KnockBackFlipMode KnockBackFlip;

    [Tooltip("ノックバックのベクトル")]
    public VectorType Vector;

    [Tooltip("吹っ飛ぶベクトル(ノックバックのベクトルが「放射状に飛ぶ」の場合有効)")]
    public Vector2 KnockBackVec;



    [Tooltip("ノックバックする力")]
    public float KnockBackPower;

}

[Serializable]
public enum KnockBackFlipMode
{
    [InspectorName("基準なし")]
    None,
    [InspectorName("衝突したコライダーの位置")]
    Collider,
    [InspectorName("衝突したコライダーの持ち主の位置")]
    Character,

}

[Serializable]
public enum VectorType
{
    [InspectorName("指定ベクトルに飛ぶ")]
    Const,
    [InspectorName("放射状に飛ぶ")]
    Radial,
}