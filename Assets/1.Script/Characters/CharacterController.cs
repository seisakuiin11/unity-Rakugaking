using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public enum CHARA_STATE
{
    idle = 0,
    move,
    jump,
    air,
    stan,
    shield,
    atkLeft,
    atkRight,
    atkUp,
    atkDown,
    attack

}





/// <summary>
/// キャラクター全体の制御
/// </summary>
public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator anim;             //アニメーション
    [SerializeField] public CharacterData charaData;    //キャラクターのステータス

    [SerializeField] CharaTransition transitionAnim;    //トランジションアニメーション
    [SerializeField] GameObject shieldObject;           //キャラクターのシールドオブジェクト
　  private CharacterCommon charaCommon;                //キャラクターの共通処理スクリプト
    private CharacterBehavior charaBehavior;            //キャラクターの固有処理スクリプト

    private Rigidbody2D rb;                             //RigidBody
    
    Dictionary<CHARA_STATE, CharacterStateBase> stateDictionary;
    private (Func<bool> condition, Action action)[] _attackTable;

    public CharacterStateBase nowState;                 //現在のステート
    [SerializeField] private CHARA_STATE state;         //現在のステート（デバッグの確認用）
    private CHARA_STATE saveAtkState;

    private int playerNum;                       //プレイヤーの番号


    private float jumpStickThreshold = 0.45f;    //ジャンプと判定するスティックの傾き具合


    private bool isDead;                         //死んでいるかどうか
    public int stanFrame;                       //スタンする時間



 

    private InputCommandData inputData;             //  プレイヤーの入力情報

    private Action damageAction = null;






    public event Action<int,int> OnDamage;                   //ダメージ時のイベント

    public event Action<int, CharacterController> OnDead;    //死亡時のイベント


    [NonSerialized]public ColliderManager colManager;

    

  


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(int _playerNum,ColliderManager _colManager)
    {
        //プレイヤー番号を挿入
        playerNum = _playerNum;

        //コライダーマネージャーを取得
        colManager= _colManager;





        List<CircleColData> hitboxList = new();

        rb=GetComponent<Rigidbody2D>();

        //CharacterCommonの初期化
        charaCommon = GetComponent<CharacterCommon>();
        charaCommon.Init(this, rb, charaData, _colManager);

        //キャラ固有スクリプトを取得
        charaBehavior =GetComponent<CharacterBehavior>();
        charaBehavior.Init(colManager);

        //各ステートの情報を挿入
        stateDictionary = new Dictionary<CHARA_STATE, CharacterStateBase>()
        {
            {CHARA_STATE.idle,     new IdleState()    },
            {CHARA_STATE.move,     new MoveState()    },
            {CHARA_STATE.jump,     new JumpState()    },
            {CHARA_STATE.air,      new AirState()     },
            {CHARA_STATE.stan,     new StanState()    },
            {CHARA_STATE.shield,   new ShieldState()  },
            {CHARA_STATE.atkLeft,  charaBehavior.Atk_Left },
            {CHARA_STATE.atkRight, charaBehavior.Atk_Right},
            {CHARA_STATE.atkUp,    charaBehavior.Atk_Up   },
            {CHARA_STATE.atkDown,  charaBehavior.Atk_Down },
            
        };

        //各ステートに初回情報を挿入
        foreach (var state in stateDictionary)
        {
            state.Value.StateInit(this,charaCommon ,anim);
        }



        _attackTable = new (Func<bool> condition, Action action)[]
        {
            (()=>inputData.ATTACK_LEFT,()=>ChangeAtkState(CHARA_STATE.atkLeft)),
            (()=>inputData.ATTACK_RIGHT,()=>ChangeAtkState(CHARA_STATE.atkRight)),
            (()=>inputData.ATTACK_UP,()=>ChangeAtkState(CHARA_STATE.atkUp)),
            (()=>inputData.ATTACK_DOWN,()=>ChangeAtkState(CHARA_STATE.atkDown)),

        };



        nowState = stateDictionary[CHARA_STATE.idle];
        nowState.StateStart();

        transitionAnim.Init(playerNum);
    }


    /// <summary>
    /// コマンドデータをセットする
    /// </summary>
    public void SetCommandData(InputCommandData _data)
    {
        inputData = _data;
    }



    /// <summary>
    /// アップデート関数
    /// </summary>
    public void UpdateMethod(float _deltaTime)
    {
        if (isDead) return;

        //現在のStateのupdate処理を行う
        nowState.StateUpdateMethod(inputData);
        
        //キャラ固有のupdate処理を行う
        charaBehavior.UpdateMethod();


        //ダメージアクションが格納されていた場合、Updateの最後に処理(相打ち処理のため)
        if (damageAction != null)
        {
            damageAction.Invoke();
            damageAction= null; 
        }
    }




    //-----------------------------------------------------------------------値変更関係


    /// <summary>
    /// キャラクターのState変更
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(CHARA_STATE _state)
    {
        nowState.StateEnd();
        nowState=stateDictionary[_state];
        state = _state;
        nowState.StateStart();
    }


    /// <summary>
    /// キャラクターのState変更(攻撃時)
    /// </summary>
    /// <param name="state"></param>
    public void ChangeAtkState(CHARA_STATE _state)
    {
        //長押しによる連射防止
        if (saveAtkState == _state) return;
        nowState.StateEnd();
        nowState = stateDictionary[_state];
        state = _state;
        saveAtkState = state;
        nowState.StateStart();
    }


    /// <summary>
    /// ダメージ関数
    /// </summary>
    /// <param name="_damage">ダメージ</param>
    /// <param name="_stanFrame">操作不可能時間</param>
    /// <param name="_knockBackData">ノックバック情報</param>
    /// <param name="_col">衝突したコライダー</param>
    public void Damage(int _damage, int _stanFrame, KnockBackData _knockBackData,Collider _col)
    {

       
        //食らうActionを格納
        damageAction += () =>
        {
            nowState.Damage(_damage, _stanFrame, _knockBackData, _col);
        };

        

    }


    


    /// <summary>
    /// 復活する
    /// </summary>
    public void Revive()
    {
        isDead = false;
        gameObject.SetActive(true);
        charaCommon.Revive();
    }

    public void HPUIChange(int _hp)
    {
        //UIを反映
        OnDamage?.Invoke(playerNum, _hp);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Dead()
    {
        DeadAnim();



        isDead = true;
        nowState.StateEnd();

        OnDead?.Invoke(playerNum, this);
    }
    async void DeadAnim()
    {
        TransitionAnim("Hide");
        await Task.Delay(1000);
        gameObject.SetActive(false);
    }


    
    public void GameEnd()
    {
        charaCommon.StopXMove();
    }
    



    /// <summary>
    /// キャラクターのトランジションアニメーション　登場,退出
    /// </summary>
    /// <param name="triggerName"></param>
    public void TransitionAnim(string triggerName)
    {
        transitionAnim.TransitionAnim(triggerName);
    }


    //-----------------------------------------------------------------------------------------------状態取得
    public bool GetIsDead() => isDead;

    public CHARA_STATE State() => state;
    public Rigidbody2D GetRigidBody() => rb;

    public GameObject GetShieldObject() => shieldObject;

    public int GetPNum() => playerNum;

    public int GetHP() =>charaCommon.GetHP();

    public CharacterCommon GetCharacterCommon => charaCommon;

    /// <summary>
    /// 横入力しているか確認する
    /// </summary>
    /// <returns></returns>
    public bool MoveCheck(){   return inputData.DIRECTION_VEC2.x != 0; }


    /// <summary>
    /// ジャンプ入力しているか確認する
    /// </summary>
    /// <returns></returns>
    public bool JumpCheck(){   return inputData.DIRECTION_VEC2.y >= jumpStickThreshold; }

    /// <summary>
    /// シールド入力されているかを確認する
    /// </summary>
    /// <returns></returns>
    public bool ShieldCheck(){ return inputData.SHIELD; }



    /// <summary>
    /// 右方向を向いているか確認する
    /// </summary>
    /// <returns></returns>
    public bool DirectionRightCheck()=> transform.localScale.x > 0; 










    /// <summary>
    /// 攻撃入力をしているか確認し、していたらStateをそれぞれのAttackにする
    /// </summary>
    /// <returns>攻撃を入力しているか</returns>
    public bool AttackCheck()
    {
        foreach (var value in _attackTable)
        {
            if (!value.condition()) continue;

            value.action();
            return true;
        }

        //入力していない場合saveAtkを初期化（長押し連打防止）
        saveAtkState = CHARA_STATE.idle;
        return false;
    }



    /// <summary>
    /// スタン時間が終了したか確認
    /// </summary>
    /// <returns></returns>
    public bool StanEndCheck()
    {
        return stanFrame <= 0;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();
        var pColHeightHalf = pCol.size.y/ 2;
        Ray hit = new Ray(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down);
   
        Gizmos.DrawRay(hit);


    }

    
}


