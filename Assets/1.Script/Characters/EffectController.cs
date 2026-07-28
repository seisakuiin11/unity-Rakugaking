using UnityEngine;

public class EffectController : MonoBehaviour
{
    public enum EffectType
    {
        Idle,
        Move,
        Jump,
        Air,
        Stun,
        Shield
    }

    [SerializeField] Animator effect;
    [SerializeField] Transform effectText;

    string effectType;


    private void Update()
    {
        var vec = effectText.localScale;
        vec.x *= System.Math.Sign(effectText.lossyScale.normalized.x);
        effectText.localScale = vec;
    }

    public void PlayEffect(EffectType type)
    {
        if (effectType == type.ToString()) return;

        effectType = type.ToString();
        effect.SetTrigger(effectType);
    }

    public void PlayUniqueEffect(string type)
    {
        if (effectType == type) return;

        effectType = type;
        effect.SetTrigger(type);
    }
}
