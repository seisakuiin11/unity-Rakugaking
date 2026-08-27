using UnityEngine;

public class CharaTransition : MonoBehaviour
{
    [SerializeField, Header("登場退出のトランジションアニメーション")]
    Animator transitionAnim;
    [SerializeField] SpriteMask transitionMaskSprite;
    [SerializeField] SpriteRenderer[] targetSprites;


    public void Init(int playerIndex)
    {
        foreach (var sprite in targetSprites)
            sprite.sortingOrder = playerIndex;
        transitionMaskSprite.frontSortingOrder = playerIndex;
        transitionMaskSprite.backSortingOrder = playerIndex-1;
    }

    /// <summary>
    /// キャラクターのトランジションアニメーション　登場,退出
    /// </summary>
    /// <param name="triggerName"></param>
    public void TransitionAnim(string triggerName)
    {
        transitionAnim.SetTrigger(triggerName);
    }
}
