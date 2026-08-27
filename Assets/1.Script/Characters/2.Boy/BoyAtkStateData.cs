using UnityEngine;

public class BoyAtkStateData : CharacterAttackStateList
{
    private void Awake()
    {
        left = new BoyLeftAttackState();
        right = new BoyRightAttackState();
        up = new BoyUpAttackState();
        down = new BoyDownAttackState();

    }
}