# PlayerATK2 - 統一攻撃システム

## 概要
シンプルで一貫性のある攻撃システム。全ての攻撃をデータドリブンで管理し、Inspector上で簡単に設定できます。

## 主要コンポーネント

### 1. AttackData (ScriptableObject)
攻撃の基本データを定義します。

**設定項目:**
- `attackName`: 攻撃の名前
- `damage`: 与えるダメージ量
- `cooldownTime`: クールタイム（秒）
- `hitBoxPrefab`: 攻撃判定のPrefab
- `duration`: 攻撃判定の持続時間
- `spawnDistance`: 生成位置のオフセット
- `followPlayerDirection`: プレイヤーの向きに従うか
- `scale`: スケール倍率

**作成方法:**
1. Projectウィンドウで右クリック
2. `Create > PlayerATK2 > AttackData`
3. 設定を入力

### 2. AttackManager
全ての攻撃を統一的に管理するマネージャー。

**使い方:**
1. プレイヤーオブジェクトにアタッチ
2. `availableAttacks`リストに使用する攻撃データを登録
3. 自動でクールタイムを管理

**主要メソッド:**
- `ExecuteAttack(int attackIndex)`: 攻撃実行
- `ExecuteAttack(AttackData attackData)`: 攻撃実行
- `IsOnCooldown(AttackData)`: クールタイム中か確認
- `GetCooldownProgress(AttackData)`: クールタイム進行率（0～1）

### 3. HitBox
攻撃のヒットボックス。敵との衝突を検知してダメージを与えます。

**使い方:**
攻撃判定Prefabにアタッチし、Collider2Dと組み合わせる

### 4. AttackButton
UIボタンから攻撃を発動するコンポーネント。

**使い方:**
1. UIボタンにアタッチ
2. `attackData`に実行する攻撃を設定
3. ボタンのOnClickイベントで`OnAttackButtonPressed()`を呼び出す

### 5. AttackCooldownUI
クールタイムをUI上に表示。

**使い方:**
1. UI要素にアタッチ
2. `cooldownImage`にImage（fillAmount用）を設定
3. `attackData`に対応する攻撃を設定

## 特殊な攻撃タイプ

### MultiHitAttackData
連続攻撃用のデータ。

**追加設定:**
- `hitCount`: 攻撃回数
- `hitInterval`: 各攻撃の間隔

**使い方:**
1. `Create > PlayerATK2 > MultiHitAttackData`で作成
2. `MultiHitAttackExecutor`コンポーネントを使用

### ChargeAttackData
チャージ攻撃用のデータ。

**追加設定:**
- `maxChargeTime`: 最大チャージ時間
- `minDamage` / `maxDamage`: ダメージ範囲
- `minScale` / `maxScale`: スケール範囲

**使い方:**
1. `Create > PlayerATK2 > ChargeAttackData`で作成
2. `ChargeAttackExecutor`コンポーネントを使用

## セットアップ手順

### 基本的な攻撃の設定

1. **AttackDataの作成**
   - Project > 右クリック > `Create > PlayerATK2 > AttackData`
   - 名前を設定（例: `NormalAttack`）
   - Inspector で設定:
     - damage: 10
     - cooldownTime: 1.0
     - hitBoxPrefab: ヒットボックスPrefab
     - duration: 0.2
     - spawnDistance: 1.0

2. **HitBoxPrefabの作成**
   - 空のGameObjectを作成
   - `HitBox`コンポーネントをアタッチ
   - `CircleCollider2D`または`BoxCollider2D`をアタッチ
   - IsTriggerをONに
   - Prefab化

3. **AttackManagerの設定**
   - プレイヤーに`AttackManager`をアタッチ
   - `availableAttacks`に作成した攻撃データを追加

4. **攻撃ボタンの設定**
   - UIボタンに`AttackButton`をアタッチ
   - `attackData`に攻撃データを設定
   - Button > OnClickで`AttackButton.OnAttackButtonPressed()`を設定

### チャージ攻撃の設定

1. **ChargeAttackDataの作成**
   - `Create > PlayerATK2 > ChargeAttackData`
   - チャージ設定を入力

2. **UIボタンにChargeAttackExecutorをアタッチ**
   - `chargeData`を設定
   - EventTriggerは不要（自動で動作）

### 多段攻撃の設定

1. **MultiHitAttackDataの作成**
   - `Create > PlayerATK2 > MultiHitAttackData`
   - hitCountとhitIntervalを設定

2. **MultiHitAttackExecutorを使用**
   - 任意のオブジェクトにアタッチ
   - UIボタンから`ExecuteMultiHit()`を呼び出す

## プレイヤーの向き対応

システムは`IPlayerDirection`または`IPlayerAttack`インターフェースから自動的にプレイヤーの向きを取得します。

**対応方法:**
- 既存の`TouchMove2`などが`IPlayerAttack.isRight`を持っていれば自動で連携

## 利点

✅ **データドリブン**: ScriptableObjectで攻撃を定義
✅ **一貫性**: 全ての攻撃が同じ方法で動作
✅ **簡単設定**: Inspector上で全て設定可能
✅ **拡張性**: 新しい攻撃タイプを簡単に追加
✅ **再利用性**: 攻撃データを複数のボタンで共有可能
✅ **クールタイム自動管理**: AttackManagerが全て管理

## 例: 3種類の攻撃ボタン

```
プレイヤー
 ├─ AttackManager (availableAttacks: Attack1, Attack2, Attack3)
 └─ TouchMove2 (IPlayerAttack)

Canvas
 ├─ Button1
 │   └─ AttackButton (attackData: Attack1)
 ├─ Button2
 │   └─ AttackButton (attackData: Attack2)
 └─ Button3
     └─ ChargeAttackExecutor (chargeData: ChargeAttack1)
```

これで簡潔で管理しやすい攻撃システムが完成します！
