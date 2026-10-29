using UnityEngine;


[CreateAssetMenu(fileName = "CharacterProfile", menuName = "ScriptableObjects/CreateCharacterProfile")]
public class CharacterProfile : ScriptableObject
{
    public const byte ATTACK_UP = 0, ATTACK_LEFT = 1, ATTACK_RIGHT = 2, ATTACK_DOWN = 3;
    /// <summary>
    /// キャラクターの名前
    /// </summary>
    [Header("キャラ名")]
    public string CharaName;
    /// <summary>
    /// 各種スキルの名前 
    /// </summary>
    public string[] SkillNames;
    /// <summary>
    /// 各種スキルの説明
    /// </summary>
    [TextArea(3,10)]
    public string[] SkillExplanation;
}
