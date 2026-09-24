using UnityEngine;


/// <summary>
/// CPUのベースクラス 各キャラに派生する
/// </summary>
public class ZeraCpu : CpuBase
{
    const float TikutikuAreaLength = 14f;
    const int TikutikuMaxFrame = 10;
    const float TogetogeAreaLength = 18f;
    const float TogetogeShotAreaLength = 6f;
    const int TogetogeMaxFrame = 60;
    const int TogetogeMaxCoolTime = 5;
    const float IgaigaAreaLength = 4f;
    const float KatikatiAreaLength = 4f;
    const float StopMoveAreaLength = 12f;
    const float LongLengeAreaLength = 6f;

    bool togetogeFlag;
    int togetogeCount;
    int togetogeCoolTime;
    bool togetogeShotFlag;
    bool tikutikuFlag;
    int tikutikuCount;


    /// <summary>
    /// アクションを考える 思考
    /// </summary>
    /// <param name="_characters"></param>
    public override InputCommandData Think(CharacterController[] _characters)
    {
        // ターゲットを決める
        SetTarget(_characters, out Vector3 myPos, out Vector3 targetPos);

        var data = new InputCommandData();
        float dis = Vector3.Distance(myPos, targetPos);
        float disX = Mathf.Abs(targetPos.x - myPos.x);
        float disY = targetPos.y - myPos.y;


        // 足場を降りる
        if (Descend(ref data)) return data;

        if(disX < StopMoveAreaLength)
        {
            // 敵が前方にいる確認
            var foward = myCharacter.transform.lossyScale.x;
            var dif = (targetPos.x - myPos.x) / disX;
            if (foward != dif) LookTarget(ref data, myPos, targetPos); ;
        }
        else {
            // ターゲットを向く
            LookTarget(ref data, myPos, targetPos);
        }

        // とげとげの長押し
        if(togetogeFlag && disX < TogetogeShotAreaLength)
        {
            togetogeFlag = false;
            togetogeCount = 0;
            togetogeShotFlag = true;
            togetogeCoolTime = 0;
            return data;
        }
        else if (togetogeFlag && togetogeCount < TogetogeMaxFrame)
        {
            togetogeCount++;
            data.ATTACK_RIGHT = true;
            data.ATTACK_RIGHT_OLD = true;
            return data;
        }
        else if(togetogeFlag)
        {
            togetogeFlag = false;
            togetogeCount = 0;
            togetogeShotFlag = true;
            togetogeCoolTime = 0;
            return data;
        }

        // とげとげのクールタイム
        if(togetogeShotFlag && togetogeCoolTime < TogetogeMaxCoolTime)
        {
            togetogeCoolTime++;
        }
        else if(togetogeShotFlag) togetogeShotFlag = false;


            // 判定をするかしないかのフラグ
            clock = !clock;
        // 攻撃判定をしない時
        if (clock) return data;

        if (tikutikuFlag && tikutikuCount < TikutikuMaxFrame)
        {
            tikutikuCount++;
            data.ATTACK_UP = true;
            return data;
        }
        else if (tikutikuFlag)
        {
            tikutikuFlag = false;
            tikutikuCount = 0;
            return data;
        }

        if (Mathf.Abs(disY) <= VerticalDiff)
        {
            if (CpuMoveRoll(0.5f) && disX < TikutikuAreaLength && disX > LongLengeAreaLength)
            {
                tikutikuFlag = true;
                data.ATTACK_UP = true;
                return data;
            }
            else if (!togetogeShotFlag && disX < TogetogeAreaLength)
            {
                togetogeFlag = true;
                data.ATTACK_RIGHT = true;
                return data;
            }
        }

        // 相手が攻撃していたら
        if (targetPlayer.State() == CHARA_STATE.atkUp || targetPlayer.State() == CHARA_STATE.atkLeft || targetPlayer.State() == CHARA_STATE.atkRight || targetPlayer.State() == CHARA_STATE.atkDown)
        {
            if(CpuMoveRoll(0.7f) && dis < KatikatiAreaLength)
            {
                data.ATTACK_LEFT = true;
                return data;
            }
        }

        // 攻撃範囲なら
        if (dis < IgaigaAreaLength)
        {
            data.ATTACK_DOWN = true;
            return data;
        }

        // 敵が上にいたら、ジャンプ
        JumpThink(ref data, dis, disY);

        // 敵が真下にいたら、足場を降りる
        DescendThink(ref data, disY);

        return data;
    }
}
