using UnityEngine;

namespace SelectScene
{
    /// <summary>
    /// コントローラー入力情報(キャラ選択画面用)
    /// </summary>
    public struct InputData
    {
        ///<summary> 左スティックの方角コマンド </summary>
        public DIRECTIONDATA DIRECTION_DATA;

        ///<summary> 前の左スティックの方角コマンド </summary>
        public DIRECTIONDATA DIRECTION_DATA_OLD;

        /// <summary> 決定 </summary>
        public bool ACCEPT;

        /// <summary> キャンセル </summary>
        public bool CANCEL;

        /// <summary> ゲームスタート </summary>
        public bool START;

        /// <summary> 左入力 </summary>
        public bool L_MOVE;

        /// <summary> 右入力 </summary>
        public bool R_MOVE;
    }
}