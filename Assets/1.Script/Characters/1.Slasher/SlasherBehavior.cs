

public class SlasherBehavior : CharacterBehavior
{

    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);

        //各攻撃ステートの生成
        Atk_Left = new SlasherLeftAttackState();
        Atk_Right= new SlasherRightAttackState();
        Atk_Up= new SlasherUpAttackState();
        Atk_Down= new SlasherDownAttackState();
    }
}
