# らくがきんぐ
最大4人で遊べる対戦ゲーム。<br>
落書きをモチーフに、個性あるキャラクターたちが繰り広げるハチャメチャバトル。<br>
&nbsp;
!["タイトル"](./RepositoryImages/TitleImage.png)

## 開発ポイント
ProjectManagerがゲーム全体の管理を担当。<br>
Inspecter上で設定できるようにし、デバッグのしやすい環境を目指しました。
!["スライド1"](./RepositoryImages/ProjectManager_編集済.png)
開発段階ですべての音素材がそろっているわけではないため、<br>
ロードする音素材を選択できるようにしました。<br>
音素材のロードにはAddressablesを使用し、ロード処理を並列で行っています。
!["スライド2"](./RepositoryImages/SoundManager_編集済.png)
キャラクター関係はCharaDataManagerで管理しています。
!["スライド3"](./RepositoryImages/CharaDataManager_編集済.png)
!["スライド4"](./RepositoryImages/CharaData.png)

## 使用技術
- Unity 6000.0.64f1
- C#
- DOTween / Addressables

## 制作期間
1ヶ月

## 制作体制
チーム制作（プログラマー兼デザイナー担当）<br>
・プランナー　　1人<br>
・プログラマー　2人