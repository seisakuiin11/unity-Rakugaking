using UnityEngine;

public class IncompleteAtkStateData : CharacterAttackStateList
{
    private void Awake()
    {
        left = new IncompleteLeftAttackState();
        right = new IncompleteRightAttackState();
        up= new IncompleteUpAttackState();
        down= new IncompleteDownAttackState();

    }
}
