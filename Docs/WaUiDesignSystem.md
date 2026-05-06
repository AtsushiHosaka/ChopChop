# Wa UI Design System

`ChopChop` のゲーム中 UI 向け和風デザインメモ。実装は Unity / C# で、ステージ内の固定文言や演出ラベルは `Assets/Resources/UI/Wa/Labels` 配下の画像スプライトを使う前提にする。

## Scope

- 対象: ゲーム中 HUD、工程キュー、成功/失敗演出、結果演出、ステージ内パネル
- 対象外: Start、Level Select、Settings などのメニュー画面
- メニュー画面は画面ごとのデザインを優先し、このルールを強制しない

## Visual Direction

- 方向性: 雅で上品な和風
- 主色: 朱色、金、生成り、焦げ茶
- 素材感: 和紙、木札、組紐、水引
- 装飾ルール: 外周フレームは朱 + 金の二重線、見出しは札 + 水引、ボタンは和紙札の立体感を優先

## Asset Structure

- 背景: `Assets/Resources/UI/Wa/background_*.png`
- フレーム: `Assets/Resources/UI/Wa/frame_main.png`
- パネル: `Assets/Resources/UI/Wa/panel_result.png`
- 見出し: `Assets/Resources/UI/Wa/header_plaque.png`
- ボタン: `button_primary*`, `button_secondary*`, `button_small*`
- カード: `level_card.png`
- 文言画像:
  - 日本語: `Assets/Resources/UI/Wa/Labels/ja/*.png`
  - 英語: `Assets/Resources/UI/Wa/Labels/en/*.png`

## Label Naming

- 共通画面文言: `app_title`, `start`, `language`, `quit`, `back`, `level_select`, `result`, `retry`, `locked`
- レベル名: `tutorial_name`, `level_1_name` ... `level_5_name`
- レベル説明: `tutorial_desc`, `level_1_desc` ... `level_5_desc`
- HUD / 結果ラベル: `hud_time`, `hud_speed`, `hud_care`, `hud_failures`

## Implementation Rules

- ゲーム中の固定文言は `Text` 直描画ではなく、画像スプライトで表示する
- 数値やタイムなどの可変値だけを `Text` で扱う
- ゲーム中 UI 側は `WaUiCatalog.LoadLocalizedKey(...)` を通してラベルを読む
- 新しい固定文言を追加する場合は、`ja` と `en` の両方を同時に用意する
- 状態差分は色変化ではなく、原則として別スプライトで表現する
- 動的テキスト用フォントは `Assets/Resources/Fonts/YujiSyuku-Regular.ttf` を優先して読み込む

## Art Notes

- 背景は和紙ではなく、ふすま面と木枠が見える構成を基準にする
- 外周フレームは朱 + 金、内側は木枠と引き手で和室らしさを出す
- ボタンは漆塗りよりも、料亭の札や欄間に合う淡い生成りと金縁を優先する
- 見出しプレートは水引を残しつつ、背景のふすまと喧嘩しないように余白を広めに取る
