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

public enum ATTACK_STATE
{
    none=0,
    left,
    right,
    up, 
    down,
}



/// <summary>
/// キャラクター自身
/// </summary>
public class CharacterController : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator anim;
    [SerializeField] public CharacterData charaData;
    [SerializeField] protected CapsuleCollider2D capsuleCol;
    [SerializeField] CharaTransition transitionAnim;
    [SerializeField] GameObject shieldObject;

    public CharacterStateBase nowState;

    Dictionary<CHARA_STATE, CharacterStateBase> stateDictionary;
    private (Func<bool> condition, Action action)[] _attackTable;

    private int playerNum;                       //プレイヤーの番号

    [SerializeField] private int hp;              //キャラクターの体力
    private int maxHp;                            //キャラクターの最大体力

    private float speed;                         //速度
    public float gravity;


    private float jumpForce;                     //ジャンプの高さ
    private float jumpStickThreshold = 0.45f;    //ジャンプと判定するスティックの傾き具合
    private float sideMoveDeadZone = 0.25f;      //横移動のデッドゾーン


    private bool isDead;                         //死んでいるかどうか
    public int stanFrame;                       //スタンする時間


    private int maxShield;                              //シールドの最大値
    public int shieldValue;                            //シールドの耐久値
    public int shieldBreakStanFlame = 300;               //シールドブレイクによるスタンフレーム数
    public float shieldDamageMultiply = 0.1f;             //hpダメージ → シールドダメージへの変換倍率
    public Vector2 shieldBreakKnockBack = new(0, 30);    //シールドブレイクによるノックバックベクトル



    InputCommandData inputData;

    private Action damageAction = null;



    [SerializeField] private CHARA_STATE state;



    //当たり判定関係
    private HitBoxColliderList hitboxColList;     //現在このプレイヤーについている当たり判定
    private PushBox_Circle pushBox;        //現在このプレイヤーについている押し出しコライダー

    public event Action<int,int> OnDamage;

    public event Action<int, CharacterController> OnDead;


    [NonSerialized]public ColliderManager colManager;

    

    private Dictionary<CHARA_STATE, Action> animDictionary;


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(int _playerNum,ColliderManager _colManager)
    {
        //プレイヤー番号を挿入
        playerNum = _playerNum;

        //コライダーマネージャーを取得
        colManager= _colManager;

        //キャラクターの情報を挿入
        speed = charaData.moveSpeed;
        maxHp = charaData.maxHp;
        hp = maxHp;
        jumpForce = charaData.jumpForce;
        maxShield = charaData.maxShield;
        shieldValue = maxShield;

        //当たり判定をデフォルトに設定
        HitBoxDefaultSet();

        //押し出しコライダーの初期設定
        CircleColData _pushBox = new();
        _pushBox.localPos = Vector3.zero;
        _pushBox.radius = 0.5f;
        _pushBox.trans = gameObject.transform;

        //押し出しコライダーを生成
        pushBox = colManager.UpdatePushBoxColliderData(_pushBox, this);



        List<CircleColData> hitboxList = new();

        //攻撃ステート情報スクリプトを取得
        CharacterAttackStateList atkState=GetComponent<CharacterAttackStateList>();

        //各ステートの情報を挿入
        stateDictionary = new Dictionary<CHARA_STATE, CharacterStateBase>()
        {
            {CHARA_STATE.idle,     new IdleState()    },
            {CHARA_STATE.move,     new MoveState()    },
            {CHARA_STATE.jump,     new JumpState()    },
            {CHARA_STATE.air,      new AirState()     },
            {CHARA_STATE.stan,     new StanState()    },
            {CHARA_STATE.shield,   new ShieldState()  },
            {CHARA_STATE.atkLeft,  atkState.GetLeftAtkState },
            {CHARA_STATE.atkRight, atkState.GetRightAtkState},
            {CHARA_STATE.atkUp,    atkState.GetUpAtkState   },
            {CHARA_STATE.atkDown,  atkState.GetDownAtkState },
            
        };


        //各ステートに初回情報を挿入
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

        nowState.StateUpdateMethod(inputData);



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
    /// キャラクターのState変更(攻撃State)
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(ATTACK_STATE _state)
    {
        nowState.StateEnd();
        nowState = stateDictionary[CHARA_STATE.attack];
        state = CHARA_STATE.attack;
        nowState.StateStart();
    }


    /// <summary>
    /// 移動速度が固定化された移動処理（Animation関係なし、動くだけ）
    /// </summary>
    /// <param name="value"></param>
    public void FixedMoveAction(Vector2 value)
    {
        
        var v = rb.linearVelocity;

        //入力がデッドゾーン以下なら移動しない
        if (Mathf.Abs(value.x) <= sideMoveDeadZone) return;


        // 入力から方向を調べ、進行方向にする
        var direction = value.x >= 0 ? 1 : -1;

        v.x = direction *speed * Time.deltaTime;

        rb.linearVelocity = v;


        if (value.x != 0) DirectionChange(value.x);
    }

    /// <summary>
    /// 移動速度が固定化されていない移動処理（Animation関係なし、動くだけ）
    /// </summary>
    /// <param name="value"></param>
    public void MoveAction(Vector2 value)
    {

        var v = rb.linearVelocity;


        //入力がデッドゾーン以下なら移動しない
        if (Mathf.Abs(value.x) <= sideMoveDeadZone) return;

        v.x = value.x * speed * Time.deltaTime;

        rb.linearVelocity = v;

        if (value.x != 0) DirectionChange(value.x);

    }

    /// <summary>
    /// プレイヤーの向きを変更する
    /// </summary>
    /// <param name="xValue">x方向の値</param>
    private void DirectionChange(float xValue)
    {


        // 入力から方向を調べ、進行方向にする
        var direction = xValue >= 0 ? 1 : -1;

        //進行方向からプレイヤーの向きを変更
        transform.localScale = new Vector3(direction, 1, 1);
    }

    public void Move(Vector2 value)
    {
        transform.position = new Vector3(transform.position.x+value.x, transform.position.y+value.y);
    }


    /// <summary>
    /// 足場に足がついているか確認する
    /// </summary>
    /// <returns></returns>
    public bool FootingGroundCheck(out Collider2D col)
    {

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();


        var pColHeightHalf = pCol.size.y / 2;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

        col = null;

        //rayを飛ばし何もなかったら抜ける
        if (hit.collider == null) return false ;

        if (hit.collider.tag != "Footing") return false;


        col = hit.collider;

        return true;
        



    }

    /// <summary>
    /// 足場を降りる
    /// </summary>
    /// <param name="footingCol"></param>
    public void FootingGroundOff(Collider2D footingCol)
    {

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();

        StartCoroutine(FootingGroundOFF(pCol,footingCol));
    }

    private IEnumerator FootingGroundOFF(Collider2D iCol,Collider2D footingCol)
    {
        Physics2D.IgnoreCollision(iCol, footingCol, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(iCol, footingCol, false);
    }


    /// <summary>
    /// プレイヤーのサイズ変更
    /// </summary>
    /// <param name="scale"></param>
    public void ScaleChange(Vector3 scale)
    {
        //現在の向いている方向にx軸を合わせる
        if (transform.localScale.x < 0) scale.x *= -1f;
        transform.localScale=scale;
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
    public void Damage(int _damage, int _stanFrame, KnockBackData _knockBackData,Collider _col)
    {

       
        //食らうActionを格納
        damageAction += () =>
        {
            nowState.Damage(_damage, _stanFrame, _knockBackData, _col);
        };

        

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
    public void KnockBack(KnockBackData _data,Collider _col)
    { 

 

        //---------吹っ飛び方が指定ベクトルの場合

        if (_data.Vector == VectorType.Const)
        {
            ConstKnockBack(_data.KnockBackFlip, _data.KnockBackVec, _data.KnockBackPower, _col);
            return;
        }



        //----------------------放射状に飛ぶ場合
        if (_data.Vector == VectorType.Radial)
        {
            RadialKnockBack(_data.KnockBackFlip, _data.KnockBackPower, _col);
            return;
        }

 
    }


    /// <summary>
    /// 指定した方向に吹っ飛ぶ
    /// </summary>
    private void ConstKnockBack(KnockBackFlipMode _mode,Vector2 _knockBackVec ,float _knockBackPower, Collider _col)
    {
        //吹っ飛ぶ方向を決める（右側か左側か）

        float flipVec = 0;

        //基準がない場合はベクトルの正規方向に飛ぶ
        if (_mode == KnockBackFlipMode.None) flipVec = 1;
        //基準がコライダーの場合
        else if (_mode == KnockBackFlipMode.Collider) { flipVec = transform.position.x - _col.WorldPos.x >= 0 ? 1 : -1; }
        //基準が持ち主の場合
        else if (_mode == KnockBackFlipMode.Character)
        {
            var x = _col.WorldPos.x - _col.LocalPos.x;
            flipVec = transform.position.x - x >= 0 ? 1 : -1;
        }



        //反転を加味して吹っ飛ばす
        var vector = _knockBackVec;
        vector.x *= flipVec;

        

        rb.linearVelocity = vector * _knockBackPower;



    }

    /// <summary>
    /// 放射状に飛ぶ
    /// </summary>
    private void RadialKnockBack(KnockBackFlipMode _mode,float _knockBackPower,Collider _col)
    {

        if (_mode== KnockBackFlipMode.Collider)
        {
            var v = (transform.position - _col.WorldPos).normalized;
            rb.linearVelocity = v * _knockBackPower;
            return;
        }
        else if (_mode == KnockBackFlipMode.Character)
        {
            //Characterならそのコライダーの持ち主の中心点を基準に左右の判定をする
            var x = _col.WorldPos - _col.LocalPos;
            var v = (transform.position - x).normalized;
            rb.linearVelocity = v * _knockBackPower;
            return;
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
    /// 復活する
    /// </summary>
    public void Revive()
    {
        isDead = false;
        gameObject.SetActive(true);

        hp = maxHp;
        HitBoxDefaultSet();

        CircleColData _pushBox = new();
        _pushBox.localPos = Vector3.zero;
        _pushBox.radius = 0.5f;
        _pushBox.trans = gameObject.transform;

        List<CircleColData> hitboxList = new();
        pushBox = colManager.UpdatePushBoxColliderData(_pushBox, this);
    }



    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        DeadAnim();

        HitBoxReset();
        PushBoxReset();

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

    /// <summary>
    /// 自身の生成しているHitBoxをすべて削除する
    /// </summary>
    public void HitBoxReset()
    {
        colManager.DestroyCircleCol(hitboxColList);
        hitboxColList = null;
    }

    /// <summary>
    /// 自身の生成しているPushBoxを削除する
    /// </summary>
    public void PushBoxReset()
    {
        colManager.DestroyCircleCol(pushBox);
        pushBox = null;
    }

    /// <summary>
    /// 当たり判定を通常時の場所に生成する
    /// </summary>
    public void HitBoxDefaultSet()
    {
        CircleColData hitbox = new();
        hitbox.localPos = Vector3.zero;
        hitbox.radius = 1f;
        hitbox.trans = gameObject.transform;

        List<CircleColData> hitboxList = new();
        hitboxList.Add(hitbox);
        hitboxColList = colManager.UpdateHitBoxColliderData(hitboxList, this);



    }


    /// <summary>
    /// HitBoxを生成する
    /// </summary>
    /// <param name="colDatas"></param>
    public void UpdateHitBoxColliderData(List<CircleColData>colDatas)
    {
       hitboxColList = colManager.UpdateHitBoxColliderData(colDatas, this);
    }



    public int GetHP() => hp;
    public bool GetIsDead() => isDead;

    public CHARA_STATE State() => state;
    public Rigidbody2D GetRigidBody()=> rb;

    public GameObject GetShieldObject() => shieldObject;

    public int GetPNum() => playerNum;

    /// <summary>
    /// 現在のシールド割合を取得する
    /// </summary>
    /// <returns></returns>
    public float GetShieldRatio() =>(float)shieldValue / (float)maxShield;

    /// <summary>
    /// キャラクターのトランジションアニメーション　登場,退出
    /// </summary>
    /// <param name="triggerName"></param>
    public void TransitionAnim(string triggerName)
    {
        transitionAnim.TransitionAnim(triggerName);
    }


    //-----------------------------------------------------------------------------------------------状態取得





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
    /// 足場に足がついているか確認する
    /// </summary>
    /// <returns></returns>
    public bool GroundCheck()
    {
        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();
        //var pColHeightHalf = transform.localScale.y/2;

        //RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

        var pColHeightHalf = pCol.size.y/ 2;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

    

        //rayを飛ばし何もなかったら抜ける
        if (hit.collider == null) return false;

        if (hit.collider.tag == "Player") return false;
        //Debug.Log(hit.collider, gameObject);
        
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();
        var pColHeightHalf = pCol.size.y/ 2;
        Ray hit = new Ray(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down);
   
        Gizmos.DrawRay(hit);


    }

    
}


