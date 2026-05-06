# ChopChop MVP Completion Checklist

このTODOは、ChopChopを「製品として遊べる」と言える状態まで進めるための完了条件です。最初の完成対象はLv.5すき焼きだが、レベル追加の導線とScene分割は最初から全レベル分を持つ。

開発規則は `DEVELOPMENT_RULES.md` に置く。特にDemo modeとProduction pathは別物として扱い、Demo完了をProduction完了に読み替えない。

## 0. MVP Definition

- [x] Demo modeとProduction pathを混ぜない規則が `DEVELOPMENT_RULES.md` に明文化されている。
- [ ] Start画面からResult画面まで、Unity EditorのPlay Modeで一度も手動Scene切替せず完走できる。
- [x] 開発用Mock入力、Mock切替UI、デバッグ用ボタンがプレイヤー向け画面に表示されない。
- [x] Tutorial/Lv.1-Lv.5がLevel Selectに表示され、対応Sceneへ遷移できるCatalog構成になっている。
- [ ] Lv.5は本番用BLE箸、食材、工程UI、スコア、Result Sceneを使って進行する。
- [ ] Tutorial/Lv.1-Lv.4も本番入力とデモ検証を分離したまま、各レベル固有の完了条件まで遊べる。
- [x] `ChopChop > Validate MVP Setup` がError/Warningなしで通る。

## 0.5 Production Asset Policy

- [ ] プレイヤーに見える器/皿/鍋/食材/箸/背景は、仮Primitiveではなく本番用assetまたは本番品質の自作meshになっている。
- [ ] Unityで生成した仮Primitiveは、MVPでは「見えない物理Collider」「検証用placeholder」に限定されている。
- [x] 見た目用assetと物理用Colliderが別GameObjectとして分離されている。
- [x] 見た目用assetのRendererは有効、物理用ColliderのRendererは存在しないか無効になっている。
- [x] Obi Fluid/Obi Cloth用のCollider壁がGame Viewで見えない。
- [x] 本番asset差し替え後も `SukiyakiStageLayout` から座標を一括調整できる。
- [x] 本番asset差し替え後もTarget ZoneとWaypoint Railを再利用できる。
- [x] asset導入時は `Assets/Art/Food/Sukiyaki`, `Assets/Art/Props`, `Assets/Art/Environment` など用途別に配置されている。
- [ ] 外部assetのライセンスがMVP展示/配布用途で問題ないことを確認済み。
- [x] 外部assetを改変した場合、元asset名/入手元/改変内容を `Assets/Docs/AssetCredits.md` に記録している。

## 1. Scene Flow

- [x] `StartScene` を起動するとタイトル、Start、Settings、Quitが表示される。
- [x] `StartScene` に生成済み背景画像が表示され、UIテキストとボタンがその上に重なる。
- [x] `StartScene` に和風の生成背景画像と漆/金基調のメニューUIが表示される。
- [x] `Start` を押すと `LevelSelectScene` に遷移する。
- [x] `Settings` を押すと `SettingsScene` に遷移し、Backで遷移元Sceneへ戻れる。
- [x] `LevelSelectScene` でTutorial/Lv.1-Lv.5が現在の学習順で表示される。
- [x] `LevelSelectScene` に和紙/料理の生成背景画像と縦型レベルカードUIが表示される。
- [x] `LevelSelectScene` のレベルカードは固定幅ではなく、Viewport/ScrollRect/柔軟Layoutで画面外にはみ出さない。
- [x] `LevelSelectScene` から `SettingsScene` へ遷移できる。
- [x] 各レベルカードを押すと、対応する分割Sceneへ遷移する。
- [x] Lv.5カードを押すと `Level05_SukiyakiScene` に遷移する。
- [x] Lv.5カードに生成済みすき焼きサムネイル画像が表示される。
- [x] Tutorial/Lv.1-Lv.4カードに生成済みレベルサムネイル画像が表示される。
- [x] `Level05_SukiyakiScene` 完了後、ゲーム内Overlayではなく `ResultScene` に遷移する。
- [x] `ResultScene` にSpeed、Care、失敗回数、完了時間が表示される。
- [x] `ResultScene` のRetryでLv.5を再開始できる。
- [x] `ResultScene` のLevel Selectで `LevelSelectScene` に戻れる。
- [x] `ResultScene` にBackボタンがあり、押すと `LevelSelectScene` に戻れる。
- [x] Start/ResultのボタンがLayoutGroupで潰れないよう、UIサイズがLayoutElementへ反映される。
- [x] 共通ボタンは単色の四角ではなく、丸み、金アクセント、highlight/shadowを持つ共通surfaceになっている。

## 1.5 Level Roadmap And Scene Split

- [x] 学習順を `Tutorial: Tempura -> Lv.1 Sashimi -> Lv.2 Tamago Kake Gohan -> Lv.3 Tofu -> Lv.4 Grilled Fish -> Lv.5 Sukiyaki` に更新する。
- [x] プレイヤー向けのレベル番号は `Tutorial, Lv.1 Sashimi, Lv.2 Tamago Kake Gohan, Lv.3 Tofu, Lv.4 Grilled Fish, Lv.5 Sukiyaki` とする。
- [ ] 開発着手順は `Lv.5 Sukiyaki -> Lv.1 Sashimi -> Lv.3 Tofu -> Tutorial Tempura -> Lv.2 Tamago Kake Gohan -> Lv.4 Grilled Fish` を基本にする。
- [x] `LevelCatalog.asset` が上記の順序で `LevelDefinition` を参照している。
- [x] `Tutorial_Tempura.asset` が存在し、`Tutorial_TempuraScene` を指している。
- [x] `Level01_Sashimi.asset` が存在し、`Level01_SashimiScene` を指している。
- [x] `Level02_TamagoKakeGohan.asset` が存在し、`Level02_TamagoKakeGohanScene` を指している。
- [x] `Level03_Tofu.asset` が存在し、`Level03_TofuScene` を指している。
- [x] `Level04_GrilledFish.asset` が存在し、`Level04_GrilledFishScene` を指している。
- [x] `Level05_Sukiyaki.asset` が存在し、`Level05_SukiyakiScene` を指している。
- [x] `Assets/Scenes/Levels/Tutorial_TempuraScene.unity` が存在する。
- [x] `Assets/Scenes/Levels/Level01_SashimiScene.unity` が存在する。
- [x] `Assets/Scenes/Levels/Level02_TamagoKakeGohanScene.unity` が存在する。
- [x] `Assets/Scenes/Levels/Level03_TofuScene.unity` が存在する。
- [x] `Assets/Scenes/Levels/Level04_GrilledFishScene.unity` が存在する。
- [x] `Assets/Scenes/Levels/Level05_SukiyakiScene.unity` が存在する。
- [x] `Assets/Scenes/SettingsScene.unity` が存在する。
- [x] Build Settingsが `Start -> LevelSelect -> Settings -> Tutorial -> Lv.1 -> Lv.2 -> Lv.3 -> Lv.4 -> Lv.5 -> Result` の順になっている。
- [x] 非すき焼きSceneでも `AppRuntime` がPrototype Level画面を生成し、戻る導線を持つ。
- [ ] 各レベルSceneに本番用 `ILevelController` 実装を接続する。
- [ ] 各レベルSceneにデモ専用Driverを別GameObjectとして接続する。
- [ ] 各レベルSceneのデモDriverは初期状態OFFで、プレイヤー向けUIから切り替えられない。
- [x] `ChopChop > Validate MVP Setup` が全LevelDefinition/Scene/thumbnailの欠損を検出する。

## 2. Localization

- [x] Unity Localization packageがPackage Managerで解決済み。
- [x] `Assets/Localization/Locales/Japanese.asset` が存在する。
- [x] `Assets/Localization/Locales/English.asset` が存在する。
- [x] `Assets/Localization/StringTables/ChopChopText_ja.asset` が存在する。
- [x] `Assets/Localization/StringTables/ChopChopText_en.asset` が存在する。
- [x] すべてのUIテキストが `ChopChopText` String Table経由で表示される。
- [x] Settings画面でLanguageを押すと日本語/英語が切り替わる。
- [x] Settings画面でBGM ON/OFFを切り替えられる。
- [x] Sukiyaki画面でLanguageを押すとHUDと工程表示が切り替わる。
- [x] Result画面でLanguageを押すと結果ラベルとボタンが切り替わる。
- [x] 日本語/英語のString Tableで欠けているkeyがない。
- [ ] 画面内テキストがボタンやHUDからはみ出さない。

## 3. BLE Chopsticks Input

- [x] `HashiInputProvider` がLv.5 Sceneの `Hashi` オブジェクトにアタッチされている。
- [x] `HashiInputProvider` が内部入力源をBLE/Demoで差し替えられる。
- [x] Demo modeを有効にしてもプレイヤー向け画面にMock/BLE切替ボタンが表示されない。
- [x] `HashiInputProvider` に本番device name filterを設定できる。
- [x] `HashiInputProvider` に本番service UUIDを設定できる。
- [x] `HashiInputProvider` に本番characteristic UUIDを設定できる。
- [x] device/service/characteristicが空欄の場合、検出したBLE候補がUnity Consoleに出る。
- [ ] 実機箸をONにした状態でmacOS Editorから接続できる。
- [ ] BLE characteristic notificationを購読できる。
- [ ] スクリーンショット形式のJSON payloadを受信できる。
- [x] `theta`, `pinch`, `vel`, `acc`, `ax`, `ay`, `az`, `gx`, `gy`, `gz`, `ts` をparseできる。
- [x] 欠けているfieldがあってもクラッシュしない。
- [x] 不正JSONが来てもクラッシュせず、throttled warningだけが出る。
- [x] 2秒以上受信が途切れた場合、最後の姿勢を保持する。
- [x] 受信途絶時にConsoleへ状態ログが出る。
- [ ] BLE再接続時に入力が復帰する。
- [x] `BleHashiInputSource` がrecoverableなBLE停止/failed状態で自動reconnectを試す。
- [x] native plugin missing / unsupported / unauthorized では自動reconnectを連打しない。
- [x] Native plugin missing時に本番画面がクラッシュしない。

## 4. Hashi Pose And Model

- [x] `Hashi` 親オブジェクトがLv.5 Sceneに配置されている。
- [x] `Lower Chopstick` と `Upper Chopstick` が `Hashi` 配下にある。
- [x] `HashiPosePresenter` が `HashiInputProvider` を参照している。
- [x] `HashiPosePresenter` が上下の箸Transformを参照している。
- [x] BLEの `theta` が上側の箸の開閉角として見える。
- [x] 下側の箸は基準箸として安定して見える。
- [x] `gx/gy/gz` によって `Hashi` 全体の姿勢が回転する。
- [x] 加速度 `ax/ay/az` によってgyro driftが弱く補正される。
- [x] 初回有効なBLE sampleでneutral poseをキャプチャできる。
- [ ] neutral pose後、自然な持ち方が画面上の自然な箸姿勢になる。
- [ ] thetaの正負/開閉方向が実機の動きと一致する。
- [ ] gyro軸の向きが実機の回転方向と一致する。
- [ ] 30秒操作しても姿勢が許容できないほど drift しない。
- [x] 箸モデルはCube仮モデルから本番モデルへ差し替え可能な構造になっている。

## 5. Chopstick Tips And Contact

- [x] `Lower Tip` が下側箸の先端に配置されている。
- [x] `Upper Tip` が上側箸の先端に配置されている。
- [x] 各Tipに `ChopstickTip` が付いている。
- [x] 各TipにColliderが付いている。
- [x] 各Tipにkinematic Rigidbodyが付いている。
- [x] 各Tipに `ObiCollider` が付いている。
- [x] `HashiTipInputBinder` がBLEのpinch/thetaをTipに渡している。
- [x] Tipのworld velocityが箸の動きに応じて更新される。
- [x] Tipが肉の `MeatContactZone` に入ると接触として検出される。
- [x] Tipが肉から離れると接触が解除される。

## 6. Waypoint Rail

- [x] `WaypointRailMover` が `Hashi` にアタッチされている。
- [x] Rail targetが `Hashi` rootになっている。
- [x] Calibration waypointがある。
- [x] Egg waypointがある。
- [x] Meat waypointがある。
- [x] Broth waypointがある。
- [x] Flip工程は独立した皿ではなく、Broth waypointを再利用して割下内で行う。
- [x] Mouth waypointがある。
- [x] 工程が変わるたびにHashiが対応waypointへ移動する。
- [ ] waypoint移動中にHashiのBLE姿勢操作が破綻しない。
- [ ] waypointの位置が卵/肉/鍋/口モデルと視覚的に一致する。
- [x] 将来Lv.1-Lv.4でもRailを再利用できる。

## 7. Sukiyaki Stage Logic

- [x] Lv.5開始時にCalibration工程から始まる。
- [x] Calibration中は「箸を自然に持ってください」が表示される。
- [x] Calibration完了後、Mix Eggへ進む。
- [x] Mix Eggは適切な回転速度で卵を混ぜると進行する。
- [x] Mix Eggで荒すぎる操作をするとCareが下がる。
- [x] Pick Meatは箸を閉じて肉を掴むと進行する。
- [ ] Put In Brothは肉を掴んだまま鍋/割下へ移動すると進行する。
- [x] Flip Meatは割下内の肉を返す動きで進行する。
- [x] Flip Meatで乱暴に動かすと `MeatTearController` 経由で肉が破れ、失敗回数が増える。
- [ ] Dip Eggは肉を掴んだまま卵へ入れると進行する。
- [ ] Deliver To Mouthは肉を口Targetへ運ぶと完了する。
- [ ] 各工程の進行条件がBLE値だけでなく、対応Target Zoneとの位置関係も見る。
- [x] `SukiyakiStepTargetZone` が箸Tipの内外判定を持ち、工程ごとのTarget Zoneとして使える。
- [x] `SukiyakiStageController` はTarget Zoneが設定された工程で、Zone未到達時にprogressを進めない。
- [ ] 工程が成功した時だけ次工程へ進む。
- [x] 工程失敗時はプレイヤーに分かるフィードバックが出る。
- [x] 完了時に `LevelRunResult` が作成される。
- [x] 完了時に `ResultScene` へ遷移する。

## 8. Food And Target Zones

- [x] 本番用の卵ボウルassetがSceneに配置されている。
- [x] 卵液面または卵メッシュがSceneに配置されている。
- [x] 本番用のすき焼き鍋assetがSceneに配置されている。
- [x] 割下/ broth 表現がSceneに配置されている。
- [ ] 本番用またはMVP品質の口/顔Target assetがSceneに配置されている。
- [x] Raw meat spawn位置がSceneに定義されている。
- [ ] Egg target zoneが配置されている。
- [ ] Meat pick target zoneが配置されている。
- [ ] Broth target zoneが配置されている。
- [ ] Flip判定はBroth内のTarget Zoneまたは鍋内専用Flip Zoneで行われる。
- [x] Mouth target zoneが配置されている。
- [ ] それぞれのTarget Zoneが工程ロジックに接続されている。
- [ ] Zoneに入っていないのに工程が進まない。
- [ ] Zoneに入っていても箸操作が不適切なら工程が進まない。

## 8.5 Stage Props And Environment

- [x] `SukiyakiStageLayout` から卵/肉/鍋/口の座標を一括編集できる。
- [x] `SukiyakiStageLayout` のEgg座標を変えると卵ボウル、卵液面、Egg waypointが一緒に動く。
- [x] `SukiyakiStageLayout` のRaw Meat座標を変えると生肉皿、肉初期位置、Meat waypointが一緒に動く。
- [x] `SukiyakiStageLayout` のBroth座標を変えると鍋、割下、Broth waypointが一緒に動く。
- [x] Flip工程は `SukiyakiStageController` と `SukiyakiCameraDirector` でBroth座標を参照する。
- [ ] 旧Flip Station/Flip皿が本番フローに不要ならSceneから撤去する。
- [x] `SukiyakiStageLayout` のMouth座標を変えると口Target、Mouth waypointが一緒に動く。
- [x] Scene上で直接動かした座標を `Capture Positions From Scene` でInspector値へ取り込める。
- [x] 各Station配下が `Visuals`, `Physics`, `Targets`, `Food`, `Legacy` に整理されている。
- [x] 各Stationの `Visuals` 配下にはプレイヤーに見えるassetだけが入っている。
- [x] 各Stationの `Physics` 配下にはCollider/ObiColliderだけが入っていてRendererが表示されない。
- [x] 各Stationの `Targets` 配下には工程判定用Triggerだけが入っている。
- [x] `Waypoints` 配下には箸Rail用のTransformだけが入っている。
- [x] 肉が落下しないように、肉の下に皿/トレイ/鍋などの支持Colliderが配置されている。
- [x] 肉を支えるColliderに `ObiCollider` が付いている。
- [x] 肉を支えるColliderの高さがObi Clothの初期位置と合っている。
- [x] Play Mode開始直後、肉が皿や鍋をすり抜けて落下しない。
- [ ] Play Mode開始から10秒間、肉が安定してステージ上に残る。
- [ ] 箸Tipが皿/鍋Colliderに過剰に引っかからない。
- [x] 本番用のテーブルまたは作業面assetが配置されている。
- [x] テーブルまたは作業面にColliderが付いている。
- [x] テーブルまたは作業面に `ObiCollider` が付いている。
- [x] 本番用の生肉皿/トレイassetが配置されている。
- [x] 本番用の割下鍋assetが配置されている。
- [x] 本番用の卵ボウルassetが配置されている。
- [ ] 本番用またはMVP品質の口/顔Target assetが配置されている。
- [x] 生肉皿/トレイは薄切り肉が皿からはみ出さないサイズになっている。
- [x] 卵ボウルはFluidが入る内側のくぼみが視覚的に分かる形状になっている。
- [x] 卵ボウルの内側に黒い穴、放射状のZ-fighting、見えるCollider壁がない。
- [x] 卵ボウルの見た目用assetとObi Fluid Colliderが別GameObjectになっている。
- [x] 卵ボウルのFluid Colliderは見た目の縁より少し内側/高めに配置され、液体が自然に収まる。
- [x] 割下鍋は白い皿ではなく、すき焼き鍋として認識できる色/深さ/縁になっている。
- [x] 割下鍋の内側に黒い穴、放射状のZ-fighting、見えるCollider壁がない。
- [x] 割下鍋の見た目用assetとObi Fluid Colliderが別GameObjectになっている。
- [x] 割下鍋のFluid Colliderは鍋の縁より少し内側/高めに配置され、割下がこぼれない。
- [x] Flip用の支持皿/小皿assetが配置されている。
- [x] Flip用の支持皿は肉を返す動作を邪魔しない高さ/広さになっている。
- [x] 各ステージオブジェクトのscaleが箸/肉/waypointと合っている。
- [x] 各ステージオブジェクトのColliderが見た目と大きくズレていない。
- [x] 皿/鍋/卵/口Targetが、現在のwaypoint railから自然に届く位置にある。
- [x] 仮Primitiveから本番モデルへ差し替えやすい命名になっている。
- [x] 本番モデルへ差し替えてもTarget ZoneやColliderを再利用できる。

## 8.6 Skybox, Lighting, And Mood

- [ ] Lv.5専用またはChopChop共通のSkybox Materialが設定されている。
- [ ] SkyboxがUnity defaultのままではない。
- [ ] 明るく清潔な食卓/調理シーンとして見える色味になっている。
- [ ] 食材の赤、卵の黄色、皿の白が見分けやすいLightingになっている。
- [ ] Directional Lightの角度と強度が食材を見やすい。
- [ ] Ambient Lightが暗すぎない。
- [ ] 影が強すぎて箸先や肉の接触が見えなくならない。
- [ ] Cameraから皿/鍋/卵/口Target/箸が画面内に収まる。
- [ ] Game ViewでHUDがステージオブジェクトを隠しすぎない。
- [ ] Play ModeでSkybox/Material missingのWarningが出ない。

## 8.6.5 Phase Camera Direction

- [x] `Main Camera` に `SukiyakiCameraDirector` がアタッチされている。
- [x] `SukiyakiCameraDirector` が `SukiyakiStageController.StepChanged` を購読している。
- [x] `SukiyakiCameraDirector` が `SukiyakiStageLayout` の卵/肉/鍋/口座標を参照している。
- [ ] Calibration中は箸と全体の位置関係が分かる引きの画になる。
- [x] Mix Egg中は卵ボウルに寄り、箸先と卵Fluidが見える。
- [x] Pick Meat中は生肉トレイに寄り、箸先と肉の接触が見える。
- [x] Put In Broth中は鍋に寄り、肉を割下へ入れる場所が見える。
- [ ] Flip Meat中は鍋/割下に寄り、肉が破れる/破れない操作が見える。
- [ ] Dip Egg中は卵ボウルに戻り、肉を卵へ入れる場所が見える。
- [ ] Deliver To Mouth中は口Targetへ寄り、目的地が分かる。
- [x] 工程遷移時にカメラが瞬間移動せず、0.5-1.0秒程度で滑らかに移動する。
- [ ] カメラ移動中もHUDが画面外に出ず、主要オブジェクトを隠しすぎない。
- [ ] カメラ移動中も箸Rail移動と競合せず、箸が画面外へ消えない。
- [x] `SukiyakiStageLayout` でStation座標を変えると、カメラの注視点も追従する。
- [ ] Play ModeでCalibrationからMix Egg/Pick Meat/Put In Broth/Flip Meatまでカメラ遷移を確認する。

## 8.7 Required MVP Art Assets

- [x] 卵ボウルassetを用意する。
- [x] 卵ボウルassetは内側が見える深さで、白身/黄身Fluidが入っていることが分かる。
- [x] 卵ボウルassetは外径/内径/深さをUnity内のmeter scaleで調整済み。
- [x] 卵ボウルassetはFluid用Colliderを別途置けるように、見た目meshが薄すぎない。
- [x] 卵ボウルassetのpivotは底面中央またはステージ配置しやすい位置にある。
- [x] すき焼き鍋assetを用意する。
- [x] すき焼き鍋assetは黒/鉄鍋/浅鍋として認識できる。
- [x] すき焼き鍋assetは割下Fluidが入る内側のくぼみがある。
- [x] すき焼き鍋assetの縁がFluid Colliderより高く見える。
- [x] 生肉皿/トレイassetを用意する。
- [x] 生肉皿/トレイassetは薄切り肉を置ける広さがある。
- [x] Flip用の小皿/支持皿assetを用意する。
- [x] Flip用皿assetは肉を返す時に視覚的な基準になる。
- [ ] 口/顔Target assetを用意する。
- [ ] 口/顔Target assetは食べ物を運ぶ目的地として一目で分かる。
- [x] テーブル/ランチョンマット/木目板など、食卓の作業面assetまたはmaterialを用意する。
- [ ] 箸本体assetを用意する。
- [ ] 箸本体assetは上下2本を別Transformとして動かせる。
- [ ] 箸先assetまたはTip Collider位置が肉/卵/割下に自然に触れる。
- [x] 薄切り牛肉assetを用意する。
- [x] 薄切り牛肉assetはObi Tearable Cloth用に単純な静的meshとして使える。
- [x] 薄切り牛肉assetは赤身/脂身が見分けられるmaterialになっている。
- [x] 割下Fluid用materialは黒く沈まず、醤油ベースの茶色に見える。
- [x] 卵白Fluid用materialは透明すぎず、白身として見える。
- [x] 卵黄Fluid用materialは白身と明確に区別できる黄色/橙色に見える。
- [x] Asset導入後、Game Viewの最初の画面で「すき焼きの練習」だと分かる。
- [x] Asset導入後、Scene ViewだけでなくGame Viewで見た目確認を行う。
- [x] Asset導入後、各RendererのMaterial missingがない。
- [x] Asset導入後、mesh scaleが極端に大きい/小さいものがない。
- [x] Asset導入後、Colliderと見た目のズレが許容範囲内にある。
- [x] Asset導入後、`SukiyakiStageLayout` の座標を再キャプチャしてWaypointを調整する。

## 8.8 Asset Import Standards

- [ ] 外部modelは `Assets/Art/Imported/<AssetName>` に原本を置く。
- [x] Sceneで使う調整済みprefabは `Assets/Art/Props` または `Assets/Art/Food/Sukiyaki/Prefabs` に置く。
- [x] 原本modelを直接Sceneに置かず、調整済みprefabをSceneに置く。
- [x] prefab名は `SukiyakiEggBowl`, `SukiyakiNabePot`, `RawMeatTray` など用途が分かる名前にする。
- [x] 見た目用prefabにはRendererとMaterialだけを持たせ、Obi用ColliderはScene側の `Physics` 配下に置く。
- [x] 物理用Colliderは `EggFluidCollider_*`, `BrothFluidCollider_*`, `MeatSupportCollider_*` のように用途が分かる名前にする。
- [x] Model import scaleが1m基準に揃っている。
- [ ] Mesh Read/Write設定が必要なassetだけ有効になっている。
- [ ] 不要に重いmeshはdecimateまたはLOD化してMVP画面でfpsを落とさない。
- [ ] TextureはMVPでは最大2Kを基本にし、必要なものだけ4Kを使う。
- [ ] Albedo/Normal/RoughnessなどPBR textureの割り当て漏れがない。
- [x] URP Litで破綻しないMaterialになっている。
- [ ] 透明materialは必要なものだけに限定し、FluidやUIと描画順が競合しない。

## 9. Egg Mixing

- [x] 卵の混ざり度を表す状態値がある。
- [x] Mix Egg中に適切な回転速度で卵の混ざり度が増える。
- [x] 遅すぎる動きでは卵が混ざらない。
- [x] 荒すぎる動きでは卵がこぼれる/混ざり品質が下がる。
- [x] 卵の見た目が混ざり度に応じて変化する。
- [ ] Mix Egg完了時に混ざり品質がCareへ反映される。
- [x] 卵を混ぜずに肉工程へ進めない。

## 9.5 Liquid Visuals

- [x] Obi Fluidが導入され、Unity 6でコンパイルできる。
- [x] Obi Fluid URP Renderer FeatureがPC/Mobile Rendererに追加されている。
- [x] 卵用Obi Fluid Rendering Passが作成されている。
- [x] 割下用Obi Fluid Rendering Passが作成されている。
- [x] 卵白用Obi Fluid Blueprintが作成されている。
- [x] 卵黄用Obi Fluid Blueprintが作成されている。
- [x] 割下用Obi Fluid Blueprintが作成されている。
- [x] 卵白EmitterがLv.5 Sceneに配置されている。
- [x] 卵黄EmitterがLv.5 Sceneに配置されている。
- [x] 割下EmitterがLv.5 Sceneに配置されている。
- [x] 卵/割下の軽量 `LiquidSurfacePresenter` 表示が本番では無効化されている。
- [x] 卵/割下のFluid用Colliderが配置されている。
- [x] Play Modeで卵白/卵黄/割下Fluid粒子が発生する。
- [x] `ObiFluidMixMonitor` が卵白/卵黄の混ざり度を読める。
- [x] Mix Eggの進行がObi Fluidの混ざり度を参照する。
- [ ] 白身と黄身が箸操作で視覚的に混ざることをGame Viewで確認する。
- [ ] 箸TipのColliderが卵Fluidを自然にかき混ぜられる。
- [ ] 遅すぎる箸操作ではObi Fluidの混ざり度が上がらない。
- [ ] 荒すぎる箸操作では卵Fluidがこぼれる/品質が下がる。
- [ ] 卵Fluidが30秒間ボウルから不自然に漏れない。
- [ ] 割下Fluidが30秒間鍋から不自然に漏れない。
- [ ] 肉を割下へ入れた瞬間に割下Fluidが押し分けられる。
- [ ] 肉を卵へつけた瞬間に卵Fluidが押し分けられる。
- [ ] Fluidの粒子数/voxelSizeがMVP想定Macで十分なfpsを維持する。

## 10. Meat Obi Cloth

- [x] `Sukiyaki Meat Rig` がLv.5 Sceneに配置されている。
- [x] `Obi Solver - Sukiyaki` が配置されている。
- [x] `Tearable Beef Slice` に `ObiTearableCloth` が付いている。
- [x] `Tearable Beef Slice` に `ObiTearableClothRenderer` が付いている。
- [x] `Tearable Beef Slice` に `ObiTearableClothDriver` が付いている。
- [x] `SukiyakiBeefSlice_TearableBlueprint` が接続されている。
- [x] `DefaultSukiyakiMeatTearProfile` が `MeatTearController` に接続されている。
- [x] `MeatContactZone` が肉の接触範囲として配置されている。
- [x] 箸Tipが `MeatContactZone` に入るとcontact countが増える。
- [ ] Obi Clothと箸Tip Colliderが物理的に反応する。
- [ ] Play Modeで肉がObi Solverにloadされる。
- [ ] Play Modeで肉のmeshが表示される。
- [ ] Play ModeでObi関連のWarning/Errorが出ない。

## 11. Meat Stress Metrics

- [x] `poke` が箸先の表面法線方向の突き込み速度として計算される。
- [x] `shear` が肉表面に沿った滑り速度として計算される。
- [x] `twist` が許可Flip軸から外れた回転速度として計算される。
- [x] `tension` が掴み点と肉中心の距離変化として計算される。
- [x] `compression` がtip間距離/pinch/thetaから計算される。
- [x] 接触していない時はdamageが増えない。
- [x] 丁寧に長く触れているだけではdamageが増えない。
- [ ] 乱暴な突き込みで `Poke` がdominant reasonになる。
- [ ] 横滑りで `Shear` がdominant reasonになる。
- [ ] 無理なひねりで `Twist` がdominant reasonになる。
- [ ] 引っ張りで `Tension` がdominant reasonになる。
- [ ] 強く挟みすぎると `Compression` がdominant reasonになる。
- [x] Stress値がConsoleまたはDebug Inspectorで確認できる開発用手段がある。

## 12. Meat Tearing

- [x] Put In Broth/Flip工程中はゲーム側stress破れ判定が走る。
- [x] Pick/Dip/Deliver工程では基本的に破れ判定が強く働かない。
- [x] Flip工程中にcritical stressを超えると肉が破れる。
- [x] accumulated damageがdurabilityを超えると肉が破れる。
- [x] Obiの自然破れ `OnClothTorn` を `MeatTearController` が受け取る。
- [x] ゲーム側Stress破れで `ObiTearableClothDriver.RequestTearAt` が呼ばれる。
- [x] `RequestTearAt` が接触点近くのObi constraintを選んで破る。
- [x] 肉が破れた時に `MeatState.Torn` へ遷移する。
- [x] 肉が破れた時に `MeatTearEvent` が発火する。
- [x] `MeatTearEvent` にreason、world point、damage、physical/gameplay由来が入る。
- [ ] 肉の破れ位置が箸が触れていた位置に近い。
- [ ] 破れ方が毎回同じ直線ではなく、接触位置に応じて変わる。
- [ ] 破れた肉の見た目がプレイヤーに分かる。
- [x] 肉が破れた時に失敗回数が+1される。
- [x] 肉が破れた時にCareが下がる。

## 13. Meat Retry

- [x] Flip工程中に肉が破れたらFlip開始地点へ戻る。
- [x] タイマーは肉破れ時に止まらない。
- [ ] 破れた肉は新しい未破れ肉インスタンスへ差し替えられる。
- [ ] 差し替え後、Obi cloth stateが完全に初期化される。
- [x] 差し替え後、`MeatTearController.Damage` が0になる。
- [x] 差し替え後、`MeatState` がFlip retryに適した状態になる。
- [ ] 複数回破れてもRetryが安定する。
- [ ] 破れた古い肉インスタンスがSceneに残り続けない。

## 14. Visual Feedback

- [x] 日本語/英語それぞれの工程Cue画像が `Assets/Resources/UI/SukiyakiCues` に生成されている。
- [x] `SukiyakiCueOverlay` が現在言語に応じた工程Cue画像を読み込む。
- [x] `SukiyakiCueOverlay` が成功/失敗Cue画像を表示するコードを持つ。
- [x] Lv.5開始前に `3, 2, 1` のカウントダウン画像Overlayが表示される。
- [ ] Play Modeで工程Cue画像が画面上部に表示されることを確認する。
- [ ] Play Modeで開始カウントダウンが表示され、終了後にCalibrationが始まることを確認する。
- [ ] Play Modeで言語切替時にCue画像も日本語/英語で切り替わることを確認する。
- [x] Calibration中のフィードバックが表示される。
- [x] 工程名がHUDに表示される。
- [x] 工程進捗バーが表示される。
- [x] 成功時に短い視覚フィードバックがある。
- [x] 失敗時に短い視覚フィードバックがある。
- [x] 肉が破れた時に明確な視覚フィードバックがある。
- [x] 卵が混ざっていく視覚フィードバックがある。
- [ ] 肉が割下に入った視覚フィードバックがある。
- [ ] 肉を卵につけた視覚フィードバックがある。
- [ ] 口へ運んだ時の完了フィードバックがある。
- [ ] UIがゲーム画面を邪魔しすぎない。
- [x] 開発用ログ/ボタンがプレイヤーに見えない。

## 15. Scoring

- [x] Speed scoreが完了時間から計算される。
- [x] Care scoreが失敗回数から下がる。
- [x] Care scoreが肉stress/damageから下がる。
- [ ] Care scoreが卵の混ぜ品質から上下する。
- [x] 荒い箸操作でCareが下がる。
- [x] 丁寧だが遅い操作ではSpeedが下がりCareは維持される。
- [x] 速いが荒い操作ではSpeedが上がりCareが下がる。
- [x] Result画面でSpeed/Care/失敗回数/完了時間が正しく表示される。
- [x] Retry後に前回のScoreが残らない。

## 16. Production Art Minimum

- [ ] 本番差し替え用の箸モデルが用意されている。
- [ ] 本番差し替え用の薄切り肉モデルが用意されている。
- [ ] 肉モデルはObi Tearable Cloth向けの2-manifoldに近いmeshになっている。
- [ ] 肉モデルはskinned meshではなく静的meshとして使える。
- [x] 卵ボウルモデルが用意されている。
- [x] 卵液面/混ざり表現用materialが用意されている。
- [x] 鍋モデルが用意されている。
- [x] 割下materialが用意されている。
- [ ] 口/顔/食べるTarget表現が用意されている。
- [x] すべての本番モデルのscaleがScene内の箸/waypointと合っている。
- [ ] 卵ボウル/鍋/皿/箸/肉のassetがGame Viewで仮Primitiveに見えない。
- [ ] 卵ボウル/鍋/皿/箸/肉のassetにUV stretchingや極端なfacet感がない。
- [x] 卵ボウル/鍋/皿/箸/肉のassetにMaterial missingや真っ黒表示がない。
- [x] すき焼き鍋が現在の白い皿状placeholderから本番assetへ置き換わっている。
- [x] 卵ボウルが現在の円形壁placeholderから本番assetへ置き換わっている。
- [x] 生肉皿が現在の単純な板placeholderから本番assetへ置き換わっている。
- [x] Flip用皿が現在の単純な板placeholderから本番assetへ置き換わっている。
- [ ] 背景/テーブル/照明が本番assetを置いた時に浮かない品質になっている。

## 17. Error Handling And UX

- [ ] BLE未接続時にプレイヤー向けの案内が出る。
- [ ] BLE接続中にプレイヤー向けの案内が出る。
- [ ] BLE接続成功時にゲーム開始可能になる。
- [x] BLE受信途絶時にゲームがクラッシュしない。
- [x] BLE payload異常時にゲームがクラッシュしない。
- [ ] Obi初期化失敗時にゲームがクラッシュしない。
- [x] 必須Scene参照が欠けた場合、Setup Missing画面が出る。
- [ ] 本番Buildで不要なDebug.Logが大量に出ない。

## 18. Tests And Validation

- [x] `HashiBleSampleParser` のEditMode testがある。
- [x] screenshot形式JSONをparseするtestがある。
- [x] field欠落JSONをparseしてクラッシュしないtestがある。
- [x] malformed JSONを無視するtestがある。
- [x] `MeatContactAnalyzer` のpoke/shear/twist/tension/compression testがある。
- [x] `TearReasonClassifier` のreason分類testがある。
- [x] `SukiyakiStageController` の工程遷移testがある。
- [x] `SukiyakiStageController` の肉破れretry testがある。
- [x] Start画面のStart/Settings/Quitボタン生成testがある。
- [x] Level Select画面のBack/Settings/Level Card生成testがある。
- [x] Settings画面のLanguage/BGM/Backボタン生成testがある。
- [x] Result画面のRetry/Back/Languageボタン生成testがある。
- [x] `ChopChopMvpValidator` がBuild Settingsを検証する。
- [x] `ChopChopMvpValidator` がLevelCatalogを検証する。
- [x] `ChopChopMvpValidator` がLocalization key欠落を検証する。
- [x] `ChopChopMvpValidator` がSukiyaki Scene referencesを検証する。
- [x] `ChopChopMvpValidator` がMeatTearProfile接続を検証する。
- [x] `ChopChopMvpValidator` がMeatTearDriver接続を検証する。

## 19. Build Readiness

- [ ] macOS Editor Play ModeでStartからResultまで完走できる。
- [ ] macOS Standalone BuildでStartからResultまで完走できる。
- [ ] macOS Standalone BuildでBLE native pluginが読み込まれる。
- [ ] macOS Standalone BuildでObi Clothが動作する。
- [x] Build SettingsのScene順が正しい。
- [ ] Product名、bundle identifier、versionが設定されている。
- [ ] macOS BLE利用に必要な権限/Info.plist設定が確認済み。
- [ ] 初回起動時の権限許可フローが破綻しない。

## 20. Done Criteria For MVP Demo

- [ ] 実機箸を接続する。
- [ ] Start画面からLv.5を選ぶ。
- [ ] 箸を自然に持ってCalibrationする。
- [ ] BLEのthetaで画面上の箸が開閉する。
- [ ] BLEのgyroで画面上の箸姿勢が動く。
- [ ] 卵を混ぜる。
- [ ] 肉を掴む。
- [ ] 肉を割下へ入れる。
- [ ] 丁寧に肉を返すと破れず次工程へ進む。
- [ ] 乱暴に肉を返すと肉が破れる。
- [ ] 肉が破れると失敗回数が増える。
- [ ] 肉が破れると未破れ肉でFlip工程をやり直せる。
- [ ] 肉を卵につける。
- [ ] 肉を口へ運ぶ。
- [ ] Result画面にSpeed/Care/失敗回数/完了時間が出る。
- [ ] Retryでもう一度最初から遊べる。

## 21. Inspector Demo Mode

- [x] Demo modeの規則が `DEVELOPMENT_RULES.md` に書かれている。
- [x] Demo modeの完了条件とProduction pathの完了条件がTODO上で分かれている。
- [x] デモモードはゲーム画面のUIではなく、`Sukiyaki Demo Driver` のInspector設定で切り替えられる。
- [x] `Sukiyaki Demo Driver` がLv.5 Sceneに配置されている。
- [x] `Sukiyaki Demo Driver` が `HashiInputProvider` を参照している。
- [x] `Sukiyaki Demo Driver` が `SukiyakiStageController` を参照している。
- [x] Demo有効時、BLE scanを開始せずConsoleにBLE検出ログを出し続けない。
- [x] Demo有効時、`HashiInputProvider.CurrentFrame` に有効なsampleが流れる。
- [x] Demo有効時、Calibration中は箸が自然な姿勢で軽く揺れる。
- [x] Demo有効時、Mix Egg中はthetaを開き気味にして箸がかき混ぜるように回る。
- [x] Demo有効時、Pick Meat中はthetaを閉じて肉を掴む姿勢になる。
- [x] Demo有効時、Put In Broth中は肉を保持したまま鍋側へ運ぶ姿勢になる。
- [x] Demo有効時、Flip Meat中は閉じた箸のまま肉を返す回転入力になる。
- [x] Demo有効時、Dip Egg中は閉じた箸で卵ボウル側へ戻る姿勢になる。
- [x] Demo有効時、Deliver To Mouth中は閉じた箸で口Target側へ運ぶ姿勢になる。
- [x] Demo有効時、各工程に合わせて `WaypointRailMover` が通常の工程waypointへ移動する。
- [x] Demo有効時、卵Fluidのmix判定に詰まっても最低限のデモ完走確認ができるよう自動工程進行を持つ。
- [ ] Play ModeでDemo有効のままStartからResultまで完走できる。
- [ ] Play ModeでDemo有効のまま箸モデルが卵、肉、鍋、卵、口の順に移動して見える。
- [ ] Play ModeでDemo有効のままResult Sceneへ遷移する。
- [x] Demo modeをOFFに戻すとBLE本番入力で起動する。
- [x] Demo modeとBLE modeの切り替え時にneutral calibrationが古い入力源から引き継がれない。
- [x] Lv.5 Sceneの初期設定では `Sukiyaki Demo Driver.demoModeEnabled` がOFFになっている。
- [ ] Demo mode関連の設定が最終ビルドで誤って有効になっていないことをリリース前に確認する。
- [x] `ChopChop > Validate MVP Setup` がRelease build向けSceneでDemo有効状態をErrorとして検出する。

## 22. Hashi Collision And Food Contact

- [x] Egg/Meat/Broth/Flip/Mouth waypointの高さを、箸Tipが皿やボウル底より下へ潜らない位置へ調整している。
- [x] Calibration waypointの高さを、箸Tipがテーブル下へ潜らない位置へ調整している。
- [x] `ObiFluidMixMonitor` が箸Tipをstirrerとして参照し、箸近傍の卵粒子が混ざり値へ拡散する。
- [ ] Play ModeでMix Egg中に箸Tipが卵液面付近に入り、ボウル底を貫通しないことをGame Viewで確認する。
- [ ] Play ModeでMix Egg中に `EggMix01` が0から上昇し、卵の色が混ざっていくことを確認する。
- [ ] Play Modeで箸本体が卵ボウル、肉皿、鍋、Flip皿を視覚的に貫通しないことを確認する。
- [ ] 箸と食器/ステージ固体のCollision Layerを定義する。
- [ ] 食材接触用Tip colliderと、食器衝突回避用chopstick body colliderを役割分離する。
- [ ] `WaypointRailMover` の直接Transform移動だけに頼らず、移動前にCapsuleCast/ComputePenetrationで安全位置へ補正する `HashiCollisionGuard` を追加する。
- [ ] 各工程waypointはHashi root座標ではなく、Lower/Upper Tipの目標位置からrootを逆算できる設定へ移行する。
- [ ] Mix Egg用に、ボウル内の安全な円軌道/楕円軌道を定義し、箸Tipが壁や底に入りすぎない範囲で混ぜる。
- [ ] 本番BLE入力時も、箸姿勢を完全自由移動ではなく工程ごとのInteraction Volume内に制限する。

## 23. Tutorial And Lv.1-Lv.4 Level Completion

- [x] Tutorial Tempura用のサムネイル画像が `Assets/Resources/UI/LevelThumbnails/tutorial_tempura.png` にある。
- [x] Lv.1 Sashimi用のサムネイル画像が `Assets/Resources/UI/LevelThumbnails/level01_sashimi.png` にある。
- [x] Lv.2 Tamago Kake Gohan用のサムネイル画像が `Assets/Resources/UI/LevelThumbnails/level02_tamago_kake_gohan.png` にある。
- [x] Lv.3 Tofu用のサムネイル画像が `Assets/Resources/UI/LevelThumbnails/level03_tofu.png` にある。
- [x] Lv.4 Grilled Fish用のサムネイル画像が `Assets/Resources/UI/LevelThumbnails/level04_grilled_fish.png` にある。
- [x] Tutorial Sceneに天ぷら練習用の食材/皿/作業面が配置されている。
- [x] Lv.1 Sceneに刺身練習用の食材/皿/作業面が配置されている。
- [x] Lv.2 Sceneに卵かけご飯練習用の器/米/卵/作業面が配置されている。
- [x] Lv.3 Sceneに豆腐assetと、一枚豆腐から切り分けるプロトタイプが配置されている。
- [x] Lv.4 Sceneに焼き魚ほぐしプロトタイプが配置されている。
- [ ] Tutorial本番: 大きく持ちやすい天ぷらを、潰さずにつまむ/持ち上げる/離す工程がある。
- [ ] Tutorialデモ: 天ぷらを自動でつまむ/持ち上げる/離す動作が、Productionとは別Driverで確認できる。
- [ ] Lv.1本番: 刺身の滑り、横ずれ、清潔な持ち上げ判定がある。
- [ ] Lv.1デモ: 刺身を箸でつまみ、滑らせず皿から持ち上げる確認ができる。
- [ ] Lv.2本番: 米表面マスクと卵の粘性/混ざり度を使い、丁寧な混ぜ方を評価する。
- [ ] Lv.2デモ: 卵を割る/米と混ぜる/口へ運ぶ流れが自動で確認できる。
- [ ] Lv.3本番: 一枚豆腐を箸で押し切り、適切なstrokeなら溝が入り、切り分け後につまめる。
- [ ] Lv.3本番: 切る前の豆腐に最初から分割線や隙間が見えない。
- [ ] Lv.3本番: 切断後だけ、切断線に沿って分離した豆腐片が見える。
- [ ] Lv.3本番: 過大な圧縮/せん断では切り分けではなく崩れ判定になる。
- [ ] Lv.3デモ: 一枚豆腐へ自動strokeを入れ、溝の出現と切り分けを確認できる。
- [ ] Lv.4本番: 焼き魚の身だけを繊維方向に沿ってほぐし、骨/皮を避ける判定がある。
- [ ] Lv.4デモ: 焼き魚の身フレークが繊維方向に順番に剥がれる確認ができる。
- [ ] 全レベルでProduction pathはBLE箸入力を使用し、Demo driverの自動入力を参照しない。
- [ ] 全レベルでDemo完了をProduction完了として扱わない。

## 24. Grilled Fish R&D And Production Plan

- [x] `GrilledFishFlakePrototype` があり、身フレーク、皮、骨を別パーツとして生成できる。
- [x] `GrilledFishFlakePrototype.ApplyChopstickStroke` で接触点に近い身フレークを繊維方向へ剥がすAPIがある。
- [x] Lv.4 SceneにProduction用の焼き魚ほぐしプロトタイプが置かれている。
- [x] Lv.4 SceneにDemo Preview RootがProductionとは別に置かれている。
- [x] Lv.4 prototypeの魚がCapsule/Cubeではなく、魚形body、頭、尾、焦げ目、背骨、肋骨、身フレークで構成されている。
- [x] 骨prototypeはCube列ではなく、中心 spine と左右 rib bones の円柱パーツで構成されている。
- [ ] 箸Tipの接触点、移動方向、速度、pinchを `ApplyChopstickStroke` へ接続する。
- [ ] 皮、骨、身フレークを本番assetとして分割して用意する。
- [ ] 身フレークは魚の繊維方向に沿った細長いmeshで、UVとmaterialがしっとりした焼き魚に見える。
- [ ] 皮はObi Clothまたは薄いmeshで、箸に引っかかって少しめくれる。
- [ ] 大きな身の柔らかさはObi Softbodyまたは簡易springで表現する。
- [ ] ほぐれてよい部位と、箸で触ると減点される骨/皮部位を `FishInteractionZone` として分ける。
- [ ] 繊維方向に沿うstrokeは成功、横切るstrokeや骨への接触はCare低下になる。
- [ ] ほぐした身の総量、骨接触回数、皮破れ回数、操作時間からScoreを計算する。
- [ ] フレークが剥がれた後に不自然に飛ばず、皿上に柔らかく残る。
- [ ] Runtimeで完全なmesh破壊を主実装にしない。必要なら本番assetの事前分割/BlendShape/Softbodyを組み合わせる。
- [ ] 本番前に、Obi Softbodyで大きな身の押し込みが自然かを検証する。
- [ ] 本番前に、Obi Clothで皮のめくれだけを補助表現できるか検証する。

## 25. Tofu R&D And Production Plan

- [x] `TofuCutPrototype` があり、初期状態は一枚豆腐として表示できる。
- [x] `TofuCutPrototype.ApplyChopstickCut` で接触点に近い潜在cut lineを進めるAPIがある。
- [x] `TofuCutPrototype.ApplyChopstickPressure` は既存呼び出し互換としてcut入力へ委譲する。
- [x] Lv.3 SceneにProduction用の一枚豆腐切り分けプロトタイプが置かれている。
- [x] Lv.3 SceneにDemo Preview RootがProductionとは別に置かれている。
- [ ] 箸Tipの接触点、tip間距離、pinch、押し込み速度、stroke方向を `ApplyChopstickCut` へ接続する。
- [ ] 豆腐本番assetは表面が滑らかな一枚meshとして用意し、切断前の溝は見せない。
- [ ] 潜在cut lineは見えないデータとして持ち、箸strokeが進んだ時だけ溝mesh/凹みshaderを出す。
- [ ] 切断完了時に一枚meshから分割済みcell表示へ切り替え、ポップが目立たない演出にする。
- [ ] 圧縮が適切なら豆腐に細い溝が入り、切り分け可能になる。
- [ ] 圧縮しすぎると切断ではなく局所崩れ、横滑りしすぎると崩れ/減点になる。
- [ ] Obi Softbodyで豆腐全体の柔らかい揺れ/押し込みを検証する。
- [ ] 本番実装は「Softbodyで一枚豆腐の柔らかさ + 潜在cut line/分割cellで切断後の分離」を基本方針にする。
- [ ] Runtime boolean切断/任意mesh切断はMVPの主実装にしない。
- [ ] 豆腐が崩れた時、破片の位置とScore理由がプレイヤーに分かる。
- [ ] 豆腐を崩さず運べた時、CareとSpeedへ反映される。

## 26. Two Player Split Screen Versus

- [x] 2人対戦の基本仕様を「左: Player 1 / 右: Player 2 / 高い合計スコアが勝ち」に決める。
- [x] `LevelRunResult.TotalScore` があり、Speed + Care の合計で勝敗判定できる。
- [x] `VersusLevelRunResult` があり、P1/P2結果、勝者、引き分けを表現できる。
- [x] `LevelResultStore` がSingle ResultとVersus Resultを区別して保持できる。
- [x] `SceneNavigator.LoadVersusResult` で2人対戦結果をResult Sceneへ渡せる。
- [x] `ResultScreenController` が2人対戦結果の場合、P1/P2合計スコアと勝者を表示できる。
- [x] `SplitScreenCameraLayout` がP1 Cameraを左半分、P2 Cameraを右半分へ割り当てられる。
- [x] `SplitScreenVersusController` がP1/P2両方のStage完了を待ち、2人対戦結果へ遷移できる。
- [x] `HashiInputProvider` が `PlayerId` と `PlayerLabel` を持ち、入力ログをP1/P2で分けられる。
- [x] `BleHashiInputSource` は現macOS BLE pluginが単一receiverである間、2つ目のBLE起動をブロックして混線を防ぐ。
- [x] `TwoPlayerBleAssignment` がP1/P2のBLE設定を別々に持ち、空欄/同一設定/Native単一receiver制限を検出できる。
- [x] Lv.5 Sceneに通常1Pを壊さない無効状態の `Versus Prototype Root` があり、分割画面/2P BLE/Versus Controllerのplaceholderを保持している。
- [ ] 2P用SceneまたはMode選択を追加し、通常1PのLv.5フローを壊さずに起動できる。
- [ ] 2P用SceneにP1用Stage一式とP2用Stage一式を別rootとして配置する。
- [ ] P1/P2それぞれに独立した `HashiInputProvider`, `HashiPosePresenter`, `HashiTipInputBinder`, `WaypointRailMover` を配置する。
- [ ] P1/P2それぞれに独立した肉/卵/鍋/Target Zone/Score状態を持たせる。
- [ ] P1/P2でObi Solver/Obi Cloth/Obi Fluidが干渉しないよう、solverまたはcollision phaseを分離する。
- [ ] P1 CameraはP1 rootだけを自然に見せ、P2 CameraはP2 rootだけを自然に見せる。
- [ ] 画面中央のdividerがHUDや操作対象を邪魔しない。
- [ ] P1/P2それぞれのHUDが左右画面内に収まり、互いに重ならない。
- [ ] P1/P2両方が完了するまでResult Sceneへ遷移しない。
- [ ] 片方が先に完了した場合、完了済みプレイヤーは結果待ち表示になり、もう片方の画面はプレイ継続できる。
- [ ] 勝敗は `TotalScore = Speed + Care` で判定し、同点はDraw表示にする。
- [ ] Retry時はP1/P2両方の状態、neutral calibration、食材状態、タイマーが初期化される。
- [ ] Level Selectから「1人で練習」「2人で対戦」を選べる製品向けUIを追加する。

## 27. Two Player BLE Production Requirements

- [x] 現在のmacOS BLE pluginが単一receiver設計であることをコード上で明示している。
- [x] 単一receiverのまま2人分BLEを開始しようとした時、2つ目を止めてError logを出す。
- [x] P1/P2 BLE設定は `TwoPlayerBleAssignment` で個別に保持できる。
- [x] P1/P2 BLE設定が同一targetを指す場合、production validationで失敗する。
- [ ] Native macOS BLE pluginを複数receiver対応にする。
- [ ] Native callbackにreceiver idまたはplayer idを含め、P1/P2のpayload queueへ正しく配送する。
- [ ] Native側でP1/P2それぞれ別のPeripheralへ同時接続できる。
- [ ] Native側でP1/P2それぞれ別のCharacteristic通知を同時購読できる。
- [ ] C#側 `MacHashiBleReceiver` がreceiver idごとにqueueを持つ。
- [ ] P1/P2のdevice name filterは必ずユニークにする。例: `ChopChop-P1`, `ChopChop-P2`。
- [ ] device nameが同じ量産機では、peripheral UUID固定割当またはペアリング画面でP1/P2を明示選択する。
- [ ] 空欄スキャンはDiscovery専用とし、2P production play開始には使えない。
- [ ] P1/P2どちらかのBLEが未接続の場合、対戦開始前にプレイヤー向け案内を出して開始を止める。
- [ ] 片方だけ再接続した時、もう片方のneutral calibrationや入力状態を巻き戻さない。

## 28. UI Motion And Feedback Polish

- [x] 画面Rootに短いfade-inを入れ、Scene遷移時の唐突な表示を避ける。
- [x] `UiButtonAnimator` により、全ての `UiFactory.CreateButton` ボタンがhover/pressで軽く反応する。
- [x] `UiSliderValueAnimator` により、HUD progress barが瞬間変化ではなく滑らかに追従する。
- [x] Start画面のMain UIが左から軽く入る。
- [x] Level SelectのHeaderが上から軽く入り、Level Cardが順番にfade-inする。
- [x] Start/Level Selectの新背景spriteと主要ボタン生成をUnity MCPで検証する。
- [x] Settings画面とLevel SelectのレスポンシブViewport生成をUnity MCPで検証する。
- [x] 共通ボタンのhover/press対象Imageがsliced rounded surfaceになっていることをUnity MCPで検証する。
- [x] Sukiyaki HUDが上から軽く入り、工程変更時にStep textが軽くpopする。
- [x] 失敗数が変化した時、Failures textが軽くpopする。
- [x] カウントダウン画像が数字ごとにpopし、次工程開始前にfadeする。
- [x] Sukiyaki工程Cue画像と成功/失敗画像が表示時に軽くpopし、feedback後にfadeする。
- [x] Result panelが中央へ軽くfade/slide-inする。
- [x] Prototype level HUDにも共通fade-inを適用する。
- [x] UI motionは `Time.unscaledTime` / `Time.unscaledDeltaTime` を使い、ゲーム進行のtimeScaleに引っ張られない。
- [ ] Play ModeでStart -> LevelSelect -> Lv.5 -> ResultのUI motionが速すぎ/遅すぎないことを確認する。
- [ ] UIアニメーションがボタン押下、言語切替、カウントダウン、Cue画像表示を邪魔しない。
- [ ] 2P split-screen時、左右HUDのアニメーションが画面中央dividerをまたがない。
- [ ] 最終MVPでは過度な揺れ/点滅を避け、箸操作と食材接触に注意が向く程度に抑える。

## 29. BGM And Sound Effects

- [x] `AudioService` がAppRuntime配下で永続化され、Sceneを跨いでBGM/SEを管理する。
- [x] Start/Level Select/Result/PrototypeではMenu BGM、Lv.5 SukiyakiではSukiyaki BGMが自動で切り替わる。
- [x] BGM切り替え時に短いfade-inを行い、Scene遷移時に唐突に鳴らさない。
- [x] Settings画面でBGM ON/OFFを切り替え、PlayerPrefsへ保存できる。
- [x] 全 `UiFactory.CreateButton` ボタンにclick SEが付いている。
- [x] 全 `UiFactory.CreateButton` ボタンにhover SEが付いている。
- [x] カウントダウンの3/2/1表示ごとにtick SEが鳴る。
- [x] カウントダウン完了時にstart SEが鳴る。
- [x] Sukiyaki工程変更時に短い工程切替SEが鳴る。
- [x] Sukiyaki工程成功時に成功SEが鳴る。
- [x] Sukiyaki工程失敗/肉破れ時に失敗SEが鳴る。
- [x] Result遷移前に完了SEが鳴る。
- [ ] Unity Editor Play ModeでBGM/SEの音量バランスを確認する。
- [x] BGM/SEがプレイヤー向けUIに開発用トグルとして露出していないことを確認する。
- [ ] 2P split-screen時、P1/P2の成功/失敗SEが過剰に重ならないように設計する。
- [ ] 最終版では生成WAVを使い続けるか、商用利用可能な高品質assetへ差し替えるか判断する。

## 30. Final Open MVP Gaps

- [ ] 実機BLE箸でmacOS Editor接続、Characteristic notification購読、JSON受信、theta/gyro軸の実機方向確認を行う。
- [ ] Lv.5工程進行をTarget Zone接続込みにし、肉/卵/鍋/口の位置関係を見て成功判定する。
- [ ] Obi Cloth肉の破れ位置、破れ理由、破れ後の未破れ肉差し替えをPlay Modeで複数回検証する。
- [ ] Obi Fluid卵/割下が30秒以上こぼれず、卵白/黄身の混ざりがCare scoreへ反映されることを確認する。
- [ ] 箸本体とTip colliderを分離し、食器/鍋/皿を視覚的に貫通しないCollision Guardを実装する。
- [ ] Start -> LevelSelect -> Lv.5 -> ResultをDemo modeとProduction pathの両方でPlay Mode完走確認する。
- [ ] macOS Standalone BuildでBLE native plugin、Obi Cloth、Obi Fluid、Result Scene遷移が動作することを確認する。
- [ ] Lv.1-Lv.4/Tutorialは現状prototype中心なので、各レベルの本番 `ILevelController` とDemo専用Driverを別々に実装する。
- [ ] 2P対戦は基盤のみなので、2P専用Scene/Mode、左右HUD、P1/P2独立Stage、複数BLE receiver対応を実装する。
- [ ] Player-visible assetのライセンス確認、最終asset差し替え、AssetCredits更新をリリース前に完了する。
- [ ] UI/BGM/SE/Lighting/SkyboxをPlay Modeで目視・聴感確認し、MVP展示に耐える品質へ調整する。

## 31. Current Verification Queue

- [x] Runtime scriptsを `ChopChop.Runtime.asmdef` に切り出し、EditMode testsがRuntime assemblyを参照できる構成にする。
- [x] Unity ConsoleのCompile Errorが0件であることを確認する。
- [ ] Unity MCPの `Unity_RunCommand` がtimeoutしたため、Unity Editor menuから `ChopChop > Validate MVP Setup` を手動実行して結果を確認する。
- [ ] Unity Test RunnerでEditMode testsを実行し、parser / meat stress / sukiyaki stage transition / meat retry / target zone gateが通ることを確認する。
- [ ] Lv.5 Sceneに `SukiyakiStepTargetZone` をEgg/Meat/Broth/Flip/Mouth用に配置し、Lower/Upper Tipをtracked tipsへ接続する。
- [ ] Target Zone未到達では工程progressが進まず、Zone到達後にBLE操作品質が良い時だけ進むことをPlay Modeで確認する。
- [ ] BLE接続後に箸電源OFF/ONまたはperipheral切断/復帰を行い、自動reconnect後にtheta/gyro入力が復帰することを確認する。
- [ ] 実機未接続、Native plugin missing、Bluetooth unauthorizedの各状態でreconnect logが過剰に連打されないことを確認する。
