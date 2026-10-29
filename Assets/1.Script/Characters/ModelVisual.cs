using UnityEngine;

public class ModelVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer head;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer ArmL1;
    [SerializeField] private SpriteRenderer ArmL2;
    [SerializeField] private SpriteRenderer ArmR1;
    [SerializeField] private SpriteRenderer ArmR2;
    [SerializeField] private SpriteRenderer LegL1;
    [SerializeField] private SpriteRenderer LegL2;
    [SerializeField] private SpriteRenderer LegR1;
    [SerializeField] private SpriteRenderer LegR2;


    /// <summary>
    /// ビジュアルデータから画像をアタッチする
    /// </summary>
    /// <param name="visualData"></param>
    public void AttachVisual(ModelVisualData visualData)
    {
        head.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.Head];
        body.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.Body];

        ArmL1.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.ArmL1];
        ArmL2.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.ArmL2];
        ArmR1.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.ArmR1];
        ArmR2.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.ArmR2];

        LegL1.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.LegL1];
        LegL2.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.LegL2];
        LegR1.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.LegR1];
        LegR2.sprite = visualData.partSprite[(int)ModelVisualData.LayerType.LegR2];
    }
}
