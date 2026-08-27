using UnityEngine;

public class CharacterAttackStateList : MonoBehaviour
{
    protected AttackState left = null;
    protected AttackState right = null;
    protected AttackState up = null;
    protected AttackState down = null;


    public AttackState GetLeftAtkState => left;
    public AttackState GetRightAtkState => right;
    public AttackState GetUpAtkState => up;
    public AttackState GetDownAtkState => down;
}
