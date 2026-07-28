using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "ScriptableObjects/CreateCharacterProfile")]
public class CharacterProfile : ScriptableObject
{
    public string CharaName;
    public string[] SkillNames;
    public CharaVisualData CharaVisual;
}
