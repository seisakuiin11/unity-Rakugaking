


using UnityEngine;

public class NarboBehavior : CharacterBehavior
{
    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);

        //各攻撃ステートの生成
        Atk_Left= new NarboLeftAttackState();
        Atk_Right= new NarboRightAttackState();
        Atk_Up = new NarboUpAttackState();
        Atk_Down = new NarboDownAttackState();
    }
}
