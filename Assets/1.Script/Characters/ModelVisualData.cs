using UnityEngine;


[CreateAssetMenu(fileName = "ModelVisual", menuName = "ScriptableObjects/CreateModelVisualData")]
public class ModelVisualData : ScriptableObject
{
    /// <summary>
    /// Še•”ˆÊ‚Ì•\¦—Dæ‡ˆÊ
    /// </summary>
    public enum LayerType : byte
    {
        ArmL2,
        ArmL1,
        Head,
        Body,
        LegL2,
        LegL1,
        LegR1,
        LegR2,
        ArmR1,
        ArmR2,
        MAX
    }

    public Sprite[] partSprite;
    public Sprite KeyVisual;
    public Sprite BustUp;

    public ModelVisualData() {  partSprite = new Sprite[(int)LayerType.MAX]; }
}
