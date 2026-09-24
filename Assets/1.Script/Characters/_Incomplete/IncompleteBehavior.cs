

public class IncompleteBehavior : CharacterBehavior
{

    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);

        //各攻撃ステートの生成
        Atk_Left = new IncompleteLeftAttackState();
        Atk_Right= new IncompleteRightAttackState();
        Atk_Up= new IncompleteUpAttackState();
        Atk_Down= new IncompleteDownAttackState();
    }
}
