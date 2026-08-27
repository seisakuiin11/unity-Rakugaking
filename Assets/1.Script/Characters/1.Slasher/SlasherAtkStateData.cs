using UnityEngine;

public class SlasherAtkStateData : CharacterAttackStateList
{
    private void Awake()
    {
        left = new SlasherLeftAttackState();
        right = new SlasherRightAttackState();
        up= new SlasherUpAttackState();
        down= new SlasherDownAttackState();

    }
}
