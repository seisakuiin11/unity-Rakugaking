using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

public enum ATTACK_STATE
{
    none=0,
    left,
    right,
    up, 
    down,
}



/// <summary>
/// キャラクター自信
/// </summary>
public class CharacterController : MonoBehaviour
{
    [SerializeField]protected Rigidbody2D rb;
    [SerializeField]protected Animator anim;
    [SerializeField]public CharacterData charaData;

    [SerializeField]public CharacterStateBase nowState;

    Dictionary<CHARA_STATE, CharacterStateBase> stateDictionary;
    private (Func<bool> condition, Action action)[] _attackTable;

    private int playerNum;                       //プレイヤーの番号
  

    private float speed;                         //速度
    private float jumpForce;                     //ジャンプの高さ
    private float jumpStickThreshold=0.5f;       //ジャンプと判定するスティックの傾き具合
    private int hp;                              //キャラクターの体力
    private int maxHp;                           //キャラクターの最大体力
    public  int stanFrame;                       //スタンする時間
    private int maxShield;
    public  int shieldValue;                     //シールドの耐久値
    public  int shieldBreakStanFlame=300;
    public float shieldDamageMultiply=0.1f; 
    public Vector2 shieldBreakKnockBack = new(0,30);
    private bool isDead;


    InputCommandData inputData;


    //攻撃関連
    private int atkAllFrame;
    private int targetFrameIndex;
    private List<FrameData> atkFrameData = new();
    private List<Attack_Circle> atkColList = new();

    public event Action<int,int> OnDamage;//OnHPChangeとかに改名予定

    public ColliderManager colManager;

    

    private Dictionary<CHARA_STATE, Action> animDictionary;


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(int _playerNum,ColliderManager _colManager)
    {
        playerNum = _playerNum;

        colManager= _colManager;

        speed = charaData.moveSpeed;
        maxHp = charaData.maxHp;
        hp = maxHp;
        jumpForce = charaData.jumpForce;

        maxShield = charaData.maxShield;
        shieldValue = maxShield;


        CircleColData hitbox = new();
        hitbox.localPos = Vector3.zero;
        hitbox.radius = 0.5f;
        hitbox.trans = gameObject.transform;

        colManager.AddNewHitBoxCircleCol(hitbox, this);

        

        stateDictionary = new Dictionary<CHARA_STATE, CharacterStateBase>()
        {
            {CHARA_STATE.idle,     new IdleState()    },
            {CHARA_STATE.move,     new MoveState()    },
            {CHARA_STATE.jump,     new JumpState()    },
            {CHARA_STATE.air,      new AirState()     },
            {CHARA_STATE.stan,     new StanState()    },
            {CHARA_STATE.shield,   new ShieldState()  },
            {CHARA_STATE.atkLeft,  new NarboLeftAttackState()  },
            {CHARA_STATE.atkRight, new NarboRightAttackState()  },
            {CHARA_STATE.atkUp,    new AttackState()  },
            {CHARA_STATE.atkDown,  new NarboDownAttackState()  },
            
        };


        foreach (var state in stateDictionary)
        {
            state.Value.StateInit(this, anim);
        }



        _attackTable = new (Func<bool> condition, Action action)[]
        {
            (()=>inputData.ATTACK_LEFT,()=>ChangeState(CHARA_STATE.atkLeft)),
            (()=>inputData.ATTACK_RIGHT,()=>ChangeState(CHARA_STATE.atkRight)),
            (()=>inputData.ATTACK_UP,()=>ChangeState(CHARA_STATE.atkUp)),
            (()=>inputData.ATTACK_DOWN,()=>ChangeState(CHARA_STATE.atkDown)),

        };

        nowState = stateDictionary[CHARA_STATE.idle];
        nowState.StateStart();

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

        nowState.StateUpdateMethod(inputData);

    }




    /// <summary>
    /// キャラクターのState変更
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(CHARA_STATE state)
    {
        nowState.StateEnd();
        nowState=stateDictionary[state];
        nowState.StateStart();
    }
    public void ChangeState(ATTACK_STATE state)
    {
        nowState.StateEnd();
        nowState = stateDictionary[CHARA_STATE.attack];
        nowState.StateStart();
    }


    /// <summary>
    /// 移動処理（Animation関係なし、動くだけ）
    /// </summary>
    /// <param name="value"></param>
    public void MoveAction(Vector2 value)
    {
        
        var v = rb.linearVelocity;

        // 入力から方向を調べ、進行方向にする
        var direction = value.x >= 0 ? 1 : -1;

        v.x = direction *speed * Time.deltaTime;

        rb.linearVelocity = v;


        //進行方向からプレイヤーの向きを変更
        transform.localScale = new Vector3(direction, 1, 1);
    }

    /// <summary>
    /// その場で止まる
    /// </summary>
    public void StopXMove()
    {
      var v = rb.linearVelocity;
        v.x = 0;
        rb.linearVelocity = v;
    }


    /// <summary>
    /// ダメージ関数
    /// </summary>
    /// <param name="_damage">ダメージ</param>
    /// <param name="_stanFrame">操作不可能時間</param>
    /// <param name="_knockBackData">ノックバック情報</param>
    /// <param name="_colPos">衝突したコライダーのWorldPos</param>
    public void Damage(int _damage, int _stanFrame, KnockBackData _knockBackData,Vector3 _colPos)
    {

        nowState.Damage(_damage,_stanFrame,_knockBackData,_colPos);


        

    }


    /// <summary>
    /// HPの値をvalue分増やし反映させる
    /// </summary>
    /// <param name="value"></param>
    public void HPChange(int value)
    {
        hp += value;
        
        //最大以上にはならないように
        if(hp>=maxHp)hp=maxHp;

        //UIを反映
        OnDamage?.Invoke(playerNum, hp);

        //０以下なら死亡
        if (hp <= 0) Dead();
    }

   /// <summary>
   /// 吹っ飛ぶ
   /// </summary>
   /// <param name="_data">ノックバックの情報</param>
   /// <param name="pos"></param>
    public void KnockBack(KnockBackData _data,Vector3 pos)
    {
        //flagがtrueなら固定のベクトルに吹っ飛ぶ
        if(_data.SpecifyDirectionFlag)rb.linearVelocity += _data.KnockBackVec* _data.KnockBackPower;
        else
        {
            //falseならそのコライダーの中心点からプレイヤーの中心点へのベクトルへ吹っ飛ぶ
            var v = (transform.position - pos).normalized;
            rb.linearVelocity = v*_data.KnockBackPower;

        }
    }

    /// <summary>
    /// Vector2の方向に吹っ飛ぶ
    /// </summary>
    /// <param name="vec"></param>
    public void KnockBack(Vector2 vec)
    {
        rb.linearVelocity = vec; 
    }

    /// <summary>
    /// スタンするフレーム数をセットする
    /// </summary>
    /// <param name="value"></param>
    public void SetStanFlame(int value)
    {
        stanFrame=value;
    }






    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        isDead = true;
        nowState.StateEnd();
        gameObject.SetActive(false);
    }


    /// <summary>
    /// ジャンプ処理（Animation関係なし、動くだけ）
    /// </summary>
    public void JumpAction()
    {
        var v = rb.linearVelocity;
        v.y = jumpForce;
        rb.linearVelocity = v;

    }


    /// <summary>
    /// シールド回復処理
    /// </summary>
    public void ShieldHeal()
    {
        if (shieldValue >= maxShield) return;
        shieldValue++;
        
    }


    public int GetHP() => hp;
    public bool GetIsDead() => isDead;

    public Rigidbody2D GetRigidBody()=> rb;




    //-----------------------------------------------------------------------------------------------各処理の入力調査





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
    /// 足場に足がついているか確認する
    /// </summary>
    /// <returns></returns>
    public bool GroundCheck()
    {
        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();
        var pColHeightHalf = pCol.size.y / 2;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

        //rayを飛ばし何もなかったら抜ける
        if (hit.collider == null) return false;
        return true;



    }





    /// <summary>
    /// 攻撃入力をしているか確認し、していたらStateをそれぞれのAttackにする
    /// </summary>
    /// <returns></returns>
    public bool AttackCheck()
    {
        foreach (var value in _attackTable)
        {
            if (!value.condition()) continue;

            value.action();
            return true;
        }
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


}


