using UnityEngine;

/// <summary>
/// キャラクター固有の処理を行うクラス
/// </summary>
public class CharacterBehavior:MonoBehaviour
{
    public AttackState Atk_Left { get; protected set; } = null;
    public AttackState Atk_Right { get; protected set; } = null;
    public AttackState Atk_Up { get; protected set; } = null;
    public AttackState Atk_Down { get; protected set; } = null;

    protected ColliderManager colManager;



    /// <summary>
    /// 初期化
    /// </summary>
    virtual public void Init(ColliderManager _colManager)
    {
        colManager = _colManager;
        return;
    }

    /// <summary>
    /// Update処理
    /// </summary>
    virtual public void UpdateMethod()
    {
        return;
    }
}
