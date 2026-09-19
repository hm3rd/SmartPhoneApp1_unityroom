# PlayerATK2 - 攻撃設定

攻撃方法はすべて `AttackData` アセットで作成します。

## 新しい攻撃の作成

1. `Assets/Scripts/PlayerATK2/AttackData` を開く
2. Projectウィンドウを右クリック
3. `Create > PlayerATK2 > AttackData`
4. `Attack Type` を選ぶ
   - `Normal`: 1回生成する通常攻撃
   - `Multi Hit`: 一定間隔で複数回生成する攻撃
   - `Charge`: ボタンを押している時間で威力と大きさが変わる攻撃
5. `Hit Box Prefab` に、`HitBox` とTriggerのCollider2Dを付けたPrefabを設定

選択したAttack Typeに必要な項目だけがInspectorへ表示されます。

## キャラクターへの登録

各 `CharacterData` の `Available Attacks` にAttackDataを登録します。
リストの0番、1番、2番がそれぞれ攻撃ボタンへ割り当てられます。

## 主な設定

- Damage: 基本ダメージ
- Cooldown Time: 再使用までの秒数
- Duration: 攻撃判定の寿命
- Spawn Distance: 前方への距離
- Spawn Offset: 前後・上下の微調整
- Scale / Scale Axes: 全体およびXY別の大きさ
- Rotation Z: 攻撃判定の角度
- Flip On Direction: 左向き時の反転

多段攻撃では回数、間隔、ヒットごとの向き更新を設定できます。
チャージ攻撃では最大時間、最小・最大ダメージ、最小・最大サイズを設定できます。

## 実行側

- `AttackManager`: 生成とクールタイムを一元管理
- `AttackButton`: 通常タップ・チャージ入力をAttackManagerへ渡す
- `AttackCooldownUI`: クールタイム表示
- `HitBox`: 敵へダメージを与える攻撃判定
