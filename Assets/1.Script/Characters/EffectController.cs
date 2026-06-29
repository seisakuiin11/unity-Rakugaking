using UnityEngine;

public class EffectController : MonoBehaviour
{
    public enum EffectType
    {
        Idle,
        Move,
        Jump,
        Air,
        Stun
    }

    [SerializeField] Animator effect;
    [SerializeField] Transform effectText;


    private void Update()
    {
        var vec = effectText.localScale;
        vec.x *= effectText.lossyScale.x;
        effectText.localScale = vec;
    }

    public void PlayEffect(EffectType type)
    {
        effect.SetTrigger(type.ToString());
    }

    public void PlayUniqueEffect(string type)
    {
        effect.SetTrigger(type);
    }
}
