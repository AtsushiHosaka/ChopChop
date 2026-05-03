# ChopChop MVP Completion Checklist

このTODOは、Lv.5すき焼きMVPを「製品として遊べる」と言える状態まで進めるための完了条件です。

開発規則は `DEVELOPMENT_RULES.md` に置く。特にDemo modeとProduction pathは別物として扱い、Demo完了をProduction完了に読み替えない。

## 0. MVP Definition

- [x] Demo modeとProduction pathを混ぜない規則が `DEVELOPMENT_RULES.md` に明文化されている。
- [ ] Start画面からResult画面まで、Unity EditorのPlay Modeで一度も手動Scene切替せず完走できる。
- [ ] 開発用Mock入力、Mock切替UI、デバッグ用ボタンがプレイヤー向け画面に表示されない。
- [ ] Lv.5すき焼き以外のLv.1-Lv.4はLevel Selectに表示されるが、Coming Soonとして選択不可になっている。
- [ ] Lv.5は本番用BLE箸、食材、工程UI、スコア、Result Sceneを使って進行する。
- [ ] `ChopChop > Validate MVP Setup` がError/Warningなしで通る。

## 0.5 Production Asset Policy

- [ ] プレイヤーに見える器/皿/鍋/食材/箸/背景は、仮Primitiveではなく本番用assetまたは本番品質の自作meshになっている。
- [ ] Unityで生成した仮Primitiveは、MVPでは「見えない物理Collider」「検証用placeholder」に限定されている。
- [x] 見た目用assetと物理用Colliderが別GameObjectとして分離されている。
- [x] 見た目用assetのRendererは有効、物理用ColliderのRendererは存在しないか無効になっている。
- [x] Obi Fluid/Obi Cloth用のCollider壁がGame Viewで見えない。
- [x] 本番asset差し替え後も `SukiyakiStageLayout` から座標を一括調整できる。
- [x] 本番asset差し替え後もTarget ZoneとWaypoint Railを再利用できる。
- [ ] asset導入時は `Assets/Art/Food/Sukiyaki`, `Assets/Art/Props`, `Assets/Art/Environment` など用途別に配置されている。
- [ ] 外部assetのライセンスがMVP展示/配布用途で問題ないことを確認済み。
- [ ] 外部assetを改変した場合、元asset名/入手元/改変内容を `Assets/Docs/AssetCredits.md` に記録している。

## 1. Scene Flow

- [ ] `StartScene` を起動するとタイトル、Start、Language、Quitが表示される。
- [ ] `Start` を押すと `LevelSelectScene` に遷移する。
- [ ] `LevelSelectScene` でLv.1-Lv.4がLocked/Coming Soon表示になる。
- [ ] Lv.5カードのみ選択可能で、押すと `Level05_SukiyakiScene` に遷移する。
- [ ] `Level05_SukiyakiScene` 完了後、ゲーム内Overlayではなく `ResultScene` に遷移する。
- [ ] `ResultScene` にSpeed、Care、失敗回数、完了時間が表示される。
- [ ] `ResultScene` のRetryでLv.5を再開始できる。
- [ ] `ResultScene` のLevel Selectで `LevelSelectScene` に戻れる。

## 2. Localization

- [ ] Unity Localization packageがPackage Managerで解決済み。
- [ ] `Assets/Localization/Locales/Japanese.asset` が存在する。
- [ ] `Assets/Localization/Locales/English.asset` が存在する。
- [ ] `Assets/Localization/StringTables/ChopChopText_ja.asset` が存在する。
- [ ] `Assets/Localization/StringTables/ChopChopText_en.asset` が存在する。
- [ ] すべてのUIテキストが `ChopChopText` String Table経由で表示される。
- [ ] Start画面でLanguageを押すと日本語/英語が切り替わる。
- [ ] Level Select画面でLanguageを押すと日本語/英語が切り替わる。
- [ ] Sukiyaki画面でLanguageを押すとHUDと工程表示が切り替わる。
- [ ] Result画面でLanguageを押すと結果ラベルとボタンが切り替わる。
- [ ] 日本語/英語のString Tableで欠けているkeyがない。
- [ ] 画面内テキストがボタンやHUDからはみ出さない。

## 3. BLE Chopsticks Input

- [ ] `HashiInputProvider` がLv.5 Sceneの `Hashi` オブジェクトにアタッチされている。
- [x] `HashiInputProvider` が内部入力源をBLE/Demoで差し替えられる。
- [x] Demo modeを有効にしてもプレイヤー向け画面にMock/BLE切替ボタンが表示されない。
- [ ] `HashiInputProvider` に本番device name filterを設定できる。
- [ ] `HashiInputProvider` に本番service UUIDを設定できる。
- [ ] `HashiInputProvider` に本番characteristic UUIDを設定できる。
- [ ] device/service/characteristicが空欄の場合、検出したBLE候補がUnity Consoleに出る。
- [ ] 実機箸をONにした状態でmacOS Editorから接続できる。
- [ ] BLE characteristic notificationを購読できる。
- [ ] スクリーンショット形式のJSON payloadを受信できる。
- [ ] `theta`, `pinch`, `vel`, `acc`, `ax`, `ay`, `az`, `gx`, `gy`, `gz`, `ts` をparseできる。
- [ ] 欠けているfieldがあってもクラッシュしない。
- [ ] 不正JSONが来てもクラッシュせず、throttled warningだけが出る。
- [ ] 2秒以上受信が途切れた場合、最後の姿勢を保持する。
- [ ] 受信途絶時にConsoleへ状態ログが出る。
- [ ] BLE再接続時に入力が復帰する。
- [ ] Native plugin missing時に本番画面がクラッシュしない。

## 4. Hashi Pose And Model

- [ ] `Hashi` 親オブジェクトがLv.5 Sceneに配置されている。
- [ ] `Lower Chopstick` と `Upper Chopstick` が `Hashi` 配下にある。
- [ ] `HashiPosePresenter` が `HashiInputProvider` を参照している。
- [ ] `HashiPosePresenter` が上下の箸Transformを参照している。
- [ ] BLEの `theta` が上側の箸の開閉角として見える。
- [ ] 下側の箸は基準箸として安定して見える。
- [ ] `gx/gy/gz` によって `Hashi` 全体の姿勢が回転する。
- [ ] 加速度 `ax/ay/az` によってgyro driftが弱く補正される。
- [ ] 初回有効なBLE sampleでneutral poseをキャプチャできる。
- [ ] neutral pose後、自然な持ち方が画面上の自然な箸姿勢になる。
- [ ] thetaの正負/開閉方向が実機の動きと一致する。
- [ ] gyro軸の向きが実機の回転方向と一致する。
- [ ] 30秒操作しても姿勢が許容できないほど drift しない。
- [ ] 箸モデルはCube仮モデルから本番モデルへ差し替え可能な構造になっている。

## 5. Chopstick Tips And Contact

- [ ] `Lower Tip` が下側箸の先端に配置されている。
- [ ] `Upper Tip` が上側箸の先端に配置されている。
- [ ] 各Tipに `ChopstickTip` が付いている。
- [ ] 各TipにColliderが付いている。
- [ ] 各Tipにkinematic Rigidbodyが付いている。
- [ ] 各Tipに `ObiCollider` が付いている。
- [ ] `HashiTipInputBinder` がBLEのpinch/thetaをTipに渡している。
- [ ] Tipのworld velocityが箸の動きに応じて更新される。
- [ ] Tipが肉の `MeatContactZone` に入ると接触として検出される。
- [ ] Tipが肉から離れると接触が解除される。

## 6. Waypoint Rail

- [ ] `WaypointRailMover` が `Hashi` にアタッチされている。
- [ ] Rail targetが `Hashi` rootになっている。
- [ ] Calibration waypointがある。
- [ ] Egg waypointがある。
- [ ] Meat waypointがある。
- [ ] Broth waypointがある。
- [ ] Flip waypointがある。
- [ ] Mouth waypointがある。
- [ ] 工程が変わるたびにHashiが対応waypointへ移動する。
- [ ] waypoint移動中にHashiのBLE姿勢操作が破綻しない。
- [ ] waypointの位置が卵/肉/鍋/口モデルと視覚的に一致する。
- [ ] 将来Lv.1-Lv.4でもRailを再利用できる。

## 7. Sukiyaki Stage Logic

- [ ] Lv.5開始時にCalibration工程から始まる。
- [ ] Calibration中は「箸を自然に持ってください」が表示される。
- [ ] Calibration完了後、Mix Eggへ進む。
- [ ] Mix Eggは適切な回転速度で卵を混ぜると進行する。
- [ ] Mix Eggで荒すぎる操作をするとCareが下がる。
- [ ] Pick Meatは箸を閉じて肉を掴むと進行する。
- [ ] Put In Brothは肉を掴んだまま鍋/割下へ移動すると進行する。
- [ ] Flip Meatは肉を返す動きで進行する。
- [ ] Flip Meatで乱暴に動かすと失敗回数が増える。
- [ ] Dip Eggは肉を掴んだまま卵へ入れると進行する。
- [ ] Deliver To Mouthは肉を口Targetへ運ぶと完了する。
- [ ] 各工程の進行条件がBLE値だけでなく、対応Target Zoneとの位置関係も見る。
- [ ] 工程が成功した時だけ次工程へ進む。
- [ ] 工程失敗時はプレイヤーに分かるフィードバックが出る。
- [ ] 完了時に `LevelRunResult` が作成される。
- [ ] 完了時に `ResultScene` へ遷移する。

## 8. Food And Target Zones

- [x] 本番用の卵ボウルassetがSceneに配置されている。
- [x] 卵液面または卵メッシュがSceneに配置されている。
- [x] 本番用のすき焼き鍋assetがSceneに配置されている。
- [x] 割下/ broth 表現がSceneに配置されている。
- [ ] 本番用またはMVP品質の口/顔Target assetがSceneに配置されている。
- [ ] Raw meat spawn位置がSceneに定義されている。
- [ ] Egg target zoneが配置されている。
- [ ] Meat pick target zoneが配置されている。
- [ ] Broth target zoneが配置されている。
- [ ] Flip target zoneが配置されている。
- [ ] Mouth target zoneが配置されている。
- [ ] それぞれのTarget Zoneが工程ロジックに接続されている。
- [ ] Zoneに入っていないのに工程が進まない。
- [ ] Zoneに入っていても箸操作が不適切なら工程が進まない。

## 8.5 Stage Props And Environment

- [x] `SukiyakiStageLayout` から卵/肉/鍋/Flip/口の座標を一括編集できる。
- [x] `SukiyakiStageLayout` のEgg座標を変えると卵ボウル、卵液面、Egg waypointが一緒に動く。
- [x] `SukiyakiStageLayout` のRaw Meat座標を変えると生肉皿、肉初期位置、Meat waypointが一緒に動く。
- [x] `SukiyakiStageLayout` のBroth座標を変えると鍋、割下、Broth waypointが一緒に動く。
- [x] `SukiyakiStageLayout` のFlip座標を変えるとFlip用支持皿、Flip waypointが一緒に動く。
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
- [x] `SukiyakiCameraDirector` が `SukiyakiStageLayout` の卵/肉/鍋/Flip/口座標を参照している。
- [ ] Calibration中は箸と全体の位置関係が分かる引きの画になる。
- [x] Mix Egg中は卵ボウルに寄り、箸先と卵Fluidが見える。
- [x] Pick Meat中は生肉トレイに寄り、箸先と肉の接触が見える。
- [x] Put In Broth中は鍋に寄り、肉を割下へ入れる場所が見える。
- [ ] Flip Meat中はFlip皿/鍋周辺に寄り、肉が破れる/破れない操作が見える。
- [ ] Dip Egg中は卵ボウルに戻り、肉を卵へ入れる場所が見える。
- [ ] Deliver To Mouth中は口Targetへ寄り、目的地が分かる。
- [x] 工程遷移時にカメラが瞬間移動せず、0.5-1.0秒程度で滑らかに移動する。
- [ ] カメラ移動中もHUDが画面外に出ず、主要オブジェクトを隠しすぎない。
- [ ] カメラ移動中も箸Rail移動と競合せず、箸が画面外へ消えない。
- [ ] `SukiyakiStageLayout` でStation座標を変えると、カメラの注視点も追従する。
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
- [ ] 薄切り牛肉assetを用意する。
- [ ] 薄切り牛肉assetはObi Tearable Cloth用に単純な静的meshとして使える。
- [ ] 薄切り牛肉assetは赤身/脂身が見分けられるmaterialになっている。
- [ ] 割下Fluid用materialは黒く沈まず、醤油ベースの茶色に見える。
- [ ] 卵白Fluid用materialは透明すぎず、白身として見える。
- [ ] 卵黄Fluid用materialは白身と明確に区別できる黄色/橙色に見える。
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
- [ ] Mix Egg中に適切な回転速度で卵の混ざり度が増える。
- [ ] 遅すぎる動きでは卵が混ざらない。
- [ ] 荒すぎる動きでは卵がこぼれる/混ざり品質が下がる。
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

- [ ] `Sukiyaki Meat Rig` がLv.5 Sceneに配置されている。
- [ ] `Obi Solver - Sukiyaki` が配置されている。
- [ ] `Tearable Beef Slice` に `ObiTearableCloth` が付いている。
- [ ] `Tearable Beef Slice` に `ObiTearableClothRenderer` が付いている。
- [ ] `Tearable Beef Slice` に `ObiTearableClothDriver` が付いている。
- [ ] `SukiyakiBeefSlice_TearableBlueprint` が接続されている。
- [ ] `DefaultSukiyakiMeatTearProfile` が `MeatTearController` に接続されている。
- [ ] `MeatContactZone` が肉の接触範囲として配置されている。
- [ ] 箸Tipが `MeatContactZone` に入るとcontact countが増える。
- [ ] Obi Clothと箸Tip Colliderが物理的に反応する。
- [ ] Play Modeで肉がObi Solverにloadされる。
- [ ] Play Modeで肉のmeshが表示される。
- [ ] Play ModeでObi関連のWarning/Errorが出ない。

## 11. Meat Stress Metrics

- [ ] `poke` が箸先の表面法線方向の突き込み速度として計算される。
- [ ] `shear` が肉表面に沿った滑り速度として計算される。
- [ ] `twist` が許可Flip軸から外れた回転速度として計算される。
- [ ] `tension` が掴み点と肉中心の距離変化として計算される。
- [ ] `compression` がtip間距離/pinch/thetaから計算される。
- [ ] 接触していない時はdamageが増えない。
- [ ] 丁寧に長く触れているだけではdamageが増えない。
- [ ] 乱暴な突き込みで `Poke` がdominant reasonになる。
- [ ] 横滑りで `Shear` がdominant reasonになる。
- [ ] 無理なひねりで `Twist` がdominant reasonになる。
- [ ] 引っ張りで `Tension` がdominant reasonになる。
- [ ] 強く挟みすぎると `Compression` がdominant reasonになる。
- [ ] Stress値がConsoleまたはDebug Inspectorで確認できる開発用手段がある。

## 12. Meat Tearing

- [ ] Flip工程以外では基本的に破れ判定が強く働かない。
- [ ] Flip工程中にcritical stressを超えると肉が破れる。
- [ ] accumulated damageがdurabilityを超えると肉が破れる。
- [ ] Obiの自然破れ `OnClothTorn` を `MeatTearController` が受け取る。
- [ ] ゲーム側Stress破れで `ObiTearableClothDriver.RequestTearAt` が呼ばれる。
- [ ] `RequestTearAt` が接触点近くのObi constraintを選んで破る。
- [ ] 肉が破れた時に `MeatState.Torn` へ遷移する。
- [ ] 肉が破れた時に `MeatTearEvent` が発火する。
- [ ] `MeatTearEvent` にreason、world point、damage、physical/gameplay由来が入る。
- [ ] 肉の破れ位置が箸が触れていた位置に近い。
- [ ] 破れ方が毎回同じ直線ではなく、接触位置に応じて変わる。
- [ ] 破れた肉の見た目がプレイヤーに分かる。
- [ ] 肉が破れた時に失敗回数が+1される。
- [ ] 肉が破れた時にCareが下がる。

## 13. Meat Retry

- [ ] Flip工程中に肉が破れたらFlip開始地点へ戻る。
- [ ] タイマーは肉破れ時に止まらない。
- [ ] 破れた肉は新しい未破れ肉インスタンスへ差し替えられる。
- [ ] 差し替え後、Obi cloth stateが完全に初期化される。
- [ ] 差し替え後、`MeatTearController.Damage` が0になる。
- [ ] 差し替え後、`MeatState` がFlip retryに適した状態になる。
- [ ] 複数回破れてもRetryが安定する。
- [ ] 破れた古い肉インスタンスがSceneに残り続けない。

## 14. Visual Feedback

- [ ] Calibration中のフィードバックが表示される。
- [ ] 工程名がHUDに表示される。
- [ ] 工程進捗バーが表示される。
- [ ] 成功時に短い視覚フィードバックがある。
- [ ] 失敗時に短い視覚フィードバックがある。
- [ ] 肉が破れた時に明確な視覚フィードバックがある。
- [x] 卵が混ざっていく視覚フィードバックがある。
- [ ] 肉が割下に入った視覚フィードバックがある。
- [ ] 肉を卵につけた視覚フィードバックがある。
- [ ] 口へ運んだ時の完了フィードバックがある。
- [ ] UIがゲーム画面を邪魔しすぎない。
- [ ] 開発用ログ/ボタンがプレイヤーに見えない。

## 15. Scoring

- [ ] Speed scoreが完了時間から計算される。
- [ ] Care scoreが失敗回数から下がる。
- [ ] Care scoreが肉stress/damageから下がる。
- [ ] Care scoreが卵の混ぜ品質から上下する。
- [ ] 荒い箸操作でCareが下がる。
- [ ] 丁寧だが遅い操作ではSpeedが下がりCareは維持される。
- [ ] 速いが荒い操作ではSpeedが上がりCareが下がる。
- [ ] Result画面でSpeed/Care/失敗回数/完了時間が正しく表示される。
- [ ] Retry後に前回のScoreが残らない。

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
- [ ] BLE受信途絶時にゲームがクラッシュしない。
- [ ] BLE payload異常時にゲームがクラッシュしない。
- [ ] Obi初期化失敗時にゲームがクラッシュしない。
- [ ] 必須Scene参照が欠けた場合、Setup Missing画面が出る。
- [ ] 本番Buildで不要なDebug.Logが大量に出ない。

## 18. Tests And Validation

- [ ] `HashiBleSampleParser` のEditMode testがある。
- [ ] screenshot形式JSONをparseするtestがある。
- [ ] field欠落JSONをparseしてクラッシュしないtestがある。
- [ ] malformed JSONを無視するtestがある。
- [ ] `MeatContactAnalyzer` のpoke/shear/twist/tension/compression testがある。
- [ ] `TearReasonClassifier` のreason分類testがある。
- [ ] `SukiyakiStageController` の工程遷移testがある。
- [ ] `SukiyakiStageController` の肉破れretry testがある。
- [ ] `ChopChopMvpValidator` がBuild Settingsを検証する。
- [ ] `ChopChopMvpValidator` がLevelCatalogを検証する。
- [ ] `ChopChopMvpValidator` がLocalization key欠落を検証する。
- [ ] `ChopChopMvpValidator` がSukiyaki Scene referencesを検証する。
- [ ] `ChopChopMvpValidator` がMeatTearProfile接続を検証する。
- [ ] `ChopChopMvpValidator` がMeatTearDriver接続を検証する。

## 19. Build Readiness

- [ ] macOS Editor Play ModeでStartからResultまで完走できる。
- [ ] macOS Standalone BuildでStartからResultまで完走できる。
- [ ] macOS Standalone BuildでBLE native pluginが読み込まれる。
- [ ] macOS Standalone BuildでObi Clothが動作する。
- [ ] Build SettingsのScene順が正しい。
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
- [ ] Demo modeをOFFに戻すとBLE本番入力で起動する。
- [x] Demo modeとBLE modeの切り替え時にneutral calibrationが古い入力源から引き継がれない。
- [ ] Demo mode関連の設定が最終ビルドで誤って有効になっていないことをリリース前に確認する。
- [ ] `ChopChop > Validate MVP Setup` がRelease build向けSceneでDemo有効状態をErrorとして検出する。
