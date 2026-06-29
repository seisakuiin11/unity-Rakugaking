
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("基本ステータス")]
    public int maxHp;           //最大hp
    public int maxShield;       //最大シールド
    public float moveSpeed;     //移動速度
    public float jumpForce;     //ジャンプ力
    public KnockBackData shieldBreakKnockBack;  //シールドブレイク時のノックバック

    public AnimatorController animation;
    
    public  AttackData leftAttackFrameData;
    public  AttackData rightAttackFrameData;
    public  AttackData upAttackFrameData;
    public  AttackData downAttackFrameData;
}



