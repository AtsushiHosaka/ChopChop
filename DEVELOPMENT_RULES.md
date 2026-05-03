# ChopChop Development Rules

このファイルは、ChopChopで機能を追加するときの開発規則です。特にデモ実装と本番実装を混ぜないためのルールを定義します。

## Demo And Production Separation

### 1. Demoは本番の代替ではない

- Demo modeは「動作確認」「展示前の最低限確認」「BLE実機や本番assetが未接続の状態での開発補助」のためだけに使う。
- Demo modeを実装しても、対応するProduction pathのTODOを完了扱いにしてはいけない。
- Demoで動いたことを理由に、BLE実機、Obi実物理、本番asset、本番Scene flowの検証を省略してはいけない。

### 2. DemoとProductionは入口を分ける

- プレイヤー向けUIにMock/BLE/Demo切替ボタンを出さない。
- Demo modeはInspector、Editor-only tool、または明示的な開発用設定からのみ有効化する。
- Production flowは、BLE未接続やasset欠落時に自動でDemoへfallbackしてはいけない。必要な本番要件が欠けている場合は、接続待ち、Setup Missing、または明示的なエラー状態にする。
- Release buildではDemo modeが有効なScene設定を禁止する。最終的にはValidatorで検出できるようにする。

### 3. コード上で混ざらない命名にする

- Demo専用クラス、Prefab、GameObject、設定項目には必ず `Demo` を含める。
- Production実装の名前にDemo用途を隠さない。
- Demo専用ロジックをBLE受信、食材物理、スコア計算などの本番コアロジックに直接埋め込まない。
- 共有してよいのは、入力frame、工程controller、presenterなどの本番でも使う抽象化だけにする。

### 4. TODOはDemoとProductionを別々に管理する

- Demoの完了条件とProductionの完了条件は同じチェックボックスにしない。
- Demoで確認済みの項目はDemo sectionだけを `[x]` にする。
- 本番BLE、本番食材、本番asset、本番Scene flowが未検証なら、Production sectionは未完了のまま残す。
- Demoを追加した機能には、同じTODO更新でProduction側の未完了条件も必ず追記する。

### 5. 実装時の完了条件

デモ機能を追加したPRまたはcommitは、最低限次を満たす。

- Demo modeをOFFにするとProduction input/pathへ戻る。
- Demo modeをONにしてもプレイヤー向け画面に開発用UIが出ない。
- Production pathの不足要件がTODOに残っている。
- Release前にDemoが有効なままになっていないことを確認するTODOまたはValidatorがある。

本番機能を追加したPRまたはcommitは、最低限次を満たす。

- 実機BLE、実食材物理、本番asset、または本番Scene flowのどれを満たしたかがTODOで追跡されている。
- Demo専用の自動進行や固定値に依存せずに進行する。
- Demo modeを削除しても、本番機能として成立する。

## Current Demo Policy

- `Sukiyaki Demo Driver` はLv.5すき焼きの開発確認用であり、本番入力ではない。
- `HashiInputProvider` のProduction modeはBLE入力である。
- Lv.5 Production MVPの完了判断は、Demo完走ではなく、BLE実機、肉/卵/鍋/口Target、Obi Cloth/Fluid、本番Scene flow、Result Sceneが接続されていることを基準にする。
