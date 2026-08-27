using UnityEngine;

public class NarboAtkStateData : CharacterAttackStateList
{
    private void Awake()
    {
        left = new NarboLeftAttackState();
        right = new NarboRightAttackState();
        up = new NarboUpAttackState();
        down = new NarboDownAttackState();

    }
}
