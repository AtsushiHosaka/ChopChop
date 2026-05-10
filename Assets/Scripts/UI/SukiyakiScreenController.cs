using ChopChop.App;
using ChopChop.Audio;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiScreenController : MonoBehaviour
    {
        private const string StageBackdropName = "Wa Fusuma Backdrop";
        private const string RuntimeTableName = "Runtime Sukiyaki Table";
        private GameLocalization localization;
        private SceneNavigator navigator;
        private SukiyakiStageController stage;
        private Text timeValueText;
        private Text failureValueText;
        private Text timeLabelText;
        private Text failureLabelText;
        private SukiyakiCueOverlay cueOverlay;
        private UiPunchScale failurePunch;
        private int lastFailureCount = -1;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator)
        {
            var root = UiFactory.CreateScreen("Sukiyaki Screen", Color.clear, false);
            var controller = root.AddComponent<SukiyakiScreenController>();
            controller.Initialize(localization, navigator);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator)
        {
            localization = appLocalization;
            navigator = sceneNavigator;

            var references = FindAnyObjectByType<SukiyakiSceneReferences>();
            if (references == null || !references.IsReady)
            {
                CreateSetupMissingScreen();
                return;
            }

            stage = references.StageController;
            stage.Configure(references.HashiInputProvider, references.RailMover, references.MeatTearController);
            stage.StepChanged += HandleStepChanged;
            stage.Completed += HandleCompleted;

            ApplyStageBackdrop();
            ApplyRuntimeTable();
            HideMouthTarget();
            CreateHud();
            CreateCueOverlay();
            RemoveTopRightLanguageWidget();
            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
            StartCountdown();
        }

        private void Update()
        {
            RemoveTopRightLanguageWidget();

            if (stage == null || timeValueText == null)
            {
                return;
            }

            timeValueText.text = $"{stage.ElapsedSeconds:0.0}s";
            failureValueText.text = stage.FailureCount.ToString();
            if (lastFailureCount >= 0 && stage.FailureCount != lastFailureCount)
            {
                failurePunch?.Play();
            }

            lastFailureCount = stage.FailureCount;
        }

        private void OnDestroy()
        {
            if (localization != null)
            {
                localization.LanguageChanged -= UpdateTexts;
            }

            if (stage != null)
            {
                stage.StepChanged -= HandleStepChanged;
                stage.Completed -= HandleCompleted;
            }
        }

        private void CreateHud()
        {
            CreateInfoPanel();
        }

        private void CreateInfoPanel()
        {
            var panel = new GameObject("HUD Info", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(28f, -28f);
            UiFactory.SetSize(panelRect, 408f, 132f);
            UiFactory.AddFadeSlideIn(panel, 0.28f, 0.04f, new Vector2(-12f, 0f), 0.99f);

            var panelImage = UiFactory.CreateSpriteImage(panel.transform, "Panel", WaUiCatalog.SukiyakiHudPanel, false);
            UiFactory.Stretch(panelImage.rectTransform, 0f);

            var statsRow = new GameObject("Stats Row", typeof(RectTransform));
            statsRow.transform.SetParent(panel.transform, false);
            var statsRowRect = statsRow.GetComponent<RectTransform>();
            statsRowRect.anchorMin = new Vector2(0.5f, 0f);
            statsRowRect.anchorMax = new Vector2(0.5f, 0f);
            statsRowRect.pivot = new Vector2(0.5f, 0f);
            statsRowRect.anchoredPosition = new Vector2(0f, 40f);
            UiFactory.SetSize(statsRowRect, 340f, 52f);

            CreateInfoStat(statsRow.transform, "Time", new Vector2(-84f, 0f), LocalizationKeys.Hud.Time, out timeLabelText, out timeValueText, 108f);
            CreateInfoStat(statsRow.transform, "Failures", new Vector2(84f, 0f), LocalizationKeys.Hud.Failures, out failureLabelText, out failureValueText, 82f);
            failurePunch = failureValueText.gameObject.AddComponent<UiPunchScale>();
        }

        private void CreateCueOverlay()
        {
            cueOverlay = gameObject.AddComponent<SukiyakiCueOverlay>();
            cueOverlay.Initialize(localization, stage);

            var audioBinder = gameObject.AddComponent<SukiyakiAudioBinder>();
            audioBinder.Initialize(stage);
        }

        private void StartCountdown()
        {
            var countdown = gameObject.AddComponent<SukiyakiCountdownOverlay>();
            countdown.Begin(() =>
            {
                if (stage == null)
                {
                    return;
                }

                stage.StartLevel();
                UpdateTexts();
            });
        }

        private void CreateSetupMissingScreen()
        {
            var panel = new GameObject("Setup Missing", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.SetSize(panelRect, 760f, 320f);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.3f, 0f, new Vector2(0f, -18f), 0.96f);
            var panelImage = UiFactory.CreateSpriteImage(panel.transform, "Panel", WaUiCatalog.ResultPanel, false);
            UiFactory.Stretch(panelImage.rectTransform, 0f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            UiFactory.Stretch(content.GetComponent<RectTransform>(), 0f);
            UiFactory.AddVerticalLayout(content, 18f, TextAnchor.MiddleCenter, new RectOffset(48, 48, 40, 40));

            var message = UiFactory.CreateText(content.transform, "Message", 32, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            message.text = localization.Text(LocalizationKeys.Level.SetupMissing);
            UiFactory.SetPreferredHeight(message.rectTransform, 86f);

            var backButton = UiFactory.CreateSpriteButton(content.transform, "Level Select Button",
                WaUiCatalog.ButtonSecondary, WaUiCatalog.ButtonSecondaryHover, WaUiCatalog.ButtonSecondaryPressed);
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 360f, 64f);
            var backLabel = UiFactory.CreateText(backButton.transform, "Back Label", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.1f, 0.05f, 1f));
            backLabel.text = localization.Text(LocalizationKeys.Result.LevelSelect);
            UiFactory.Stretch(backLabel.rectTransform, 8f);
            backButton.onClick.AddListener(navigator.LoadLevelSelect);
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            UpdateTexts();
        }

        private void HandleCompleted(LevelRunResult result)
        {
            navigator.LoadResult(result, AppScenes.SukiyakiLevel);
        }

        private void UpdateTexts()
        {
            if (timeLabelText != null)
            {
                timeLabelText.text = localization.Text(LocalizationKeys.Hud.Time);
            }

            if (failureLabelText != null)
            {
                failureLabelText.text = localization.Text(LocalizationKeys.Hud.Failures);
            }
        }

        private void CreateInfoStat(Transform parent, string name, Vector2 anchoredPosition, string localizationKey, out Text labelText, out Text valueText, float labelWidth)
        {
            var stat = new GameObject(name, typeof(RectTransform));
            stat.transform.SetParent(parent, false);
            var statRect = stat.GetComponent<RectTransform>();
            statRect.anchorMin = new Vector2(0.5f, 0f);
            statRect.anchorMax = new Vector2(0.5f, 0f);
            statRect.pivot = new Vector2(0.5f, 0f);
            statRect.anchoredPosition = anchoredPosition;
            UiFactory.SetSize(statRect, 148f, 52f);

            labelText = UiFactory.CreateText(stat.transform, "Label", 16, FontStyle.Bold, TextAnchor.UpperCenter, new Color(0.66f, 0.44f, 0.16f, 1f));
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, 8f);
            UiFactory.SetSize(labelRect, labelWidth, 22f);

            valueText = UiFactory.CreateText(stat.transform, "Value", 30, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            var valueRect = valueText.rectTransform;
            valueRect.anchorMin = new Vector2(0.5f, 0f);
            valueRect.anchorMax = new Vector2(0.5f, 0f);
            valueRect.pivot = new Vector2(0.5f, 0f);
            valueRect.anchoredPosition = new Vector2(0f, 8f);
            UiFactory.SetSize(valueRect, 136f, 28f);
        }

        private void ApplyStageBackdrop()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = FindAnyObjectByType<Camera>();
            }

            if (camera == null || camera.transform.Find(StageBackdropName) != null)
            {
                return;
            }

            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = StageBackdropName;
            backdrop.transform.SetParent(camera.transform, false);
            backdrop.transform.localPosition = new Vector3(0f, 0.1f, 7.6f);
            backdrop.transform.localRotation = Quaternion.identity;

            var collider = backdrop.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = backdrop.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                return;
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", BuildFusumaBackdropTexture());
            }
            else if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", BuildFusumaBackdropTexture());
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            var distance = backdrop.transform.localPosition.z;
            var height = 2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance * 1.18f;
            var width = height * camera.aspect * 1.12f;
            backdrop.transform.localScale = new Vector3(width, height, 1f);
        }

        private void ApplyRuntimeTable()
        {
            if (GameObject.Find(RuntimeTableName) != null)
            {
                return;
            }

            var legacyDiningTable = GameObject.Find("Dining Table");
            if (legacyDiningTable != null)
            {
                legacyDiningTable.SetActive(false);
            }

            var oldTable = GameObject.Find("Pandazole Kitchen Table Asset");
            Transform parent = null;
            Vector3 position = new Vector3(0f, -0.18f, 0.06f);
            Quaternion rotation = Quaternion.identity;

            if (oldTable != null)
            {
                parent = oldTable.transform.parent;
                position = oldTable.transform.position;
                rotation = oldTable.transform.rotation;
                oldTable.SetActive(false);
            }

            var tableRoot = new GameObject(RuntimeTableName);
            if (parent != null)
            {
                tableRoot.transform.SetParent(parent, false);
                tableRoot.transform.position = position;
                tableRoot.transform.rotation = rotation;
            }
            else
            {
                tableRoot.transform.position = position;
                tableRoot.transform.rotation = rotation;
            }

            var material = CreateTableMaterial();
            CreateTablePart(tableRoot.transform, "Top", new Vector3(0f, 1.03f, 0f), new Vector3(4.45f, 0.16f, 2.52f), material);
            CreateTablePart(tableRoot.transform, "Leg Front Left", new Vector3(-1.92f, 0.46f, 1.0f), new Vector3(0.18f, 0.98f, 0.18f), material);
            CreateTablePart(tableRoot.transform, "Leg Front Right", new Vector3(1.92f, 0.46f, 1.0f), new Vector3(0.18f, 0.98f, 0.18f), material);
            CreateTablePart(tableRoot.transform, "Leg Back Left", new Vector3(-1.92f, 0.46f, -1.0f), new Vector3(0.18f, 0.98f, 0.18f), material);
            CreateTablePart(tableRoot.transform, "Leg Back Right", new Vector3(1.92f, 0.46f, -1.0f), new Vector3(0.18f, 0.98f, 0.18f), material);
            CreateTablePart(tableRoot.transform, "Apron Front", new Vector3(0f, 0.86f, 1.03f), new Vector3(4.06f, 0.14f, 0.12f), material);
            CreateTablePart(tableRoot.transform, "Apron Back", new Vector3(0f, 0.86f, -1.03f), new Vector3(4.06f, 0.14f, 0.12f), material);
            CreateTablePart(tableRoot.transform, "Apron Left", new Vector3(-1.97f, 0.86f, 0f), new Vector3(0.12f, 0.14f, 2.0f), material);
            CreateTablePart(tableRoot.transform, "Apron Right", new Vector3(1.97f, 0.86f, 0f), new Vector3(0.12f, 0.14f, 2.0f), material);
        }

        private static void CreateTablePart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            var renderer = part.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static Material CreateTableMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            var brown = new Color(0.45f, 0.28f, 0.16f, 1f);

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", brown);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", brown);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.22f);
            }

            return material;
        }

        private static Texture2D BuildFusumaBackdropTexture()
        {
            const int width = 1024;
            const int height = 576;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var paper = new Color(0.90f, 0.85f, 0.72f, 1f);
            var paperShade = new Color(0.86f, 0.81f, 0.69f, 1f);
            var wood = new Color(0.40f, 0.18f, 0.07f, 1f);
            var transom = new Color(0.63f, 0.61f, 0.54f, 1f);
            var tatami = new Color(0.29f, 0.44f, 0.35f, 1f);
            var line = new Color(0.12f, 0.09f, 0.07f, 1f);

            for (var y = 0; y < height; y++)
            {
                var t = y / (float)(height - 1);
                var row = Color.Lerp(paper, paperShade, Mathf.Abs(t - 0.5f) * 0.35f);
                for (var x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, row);
                }
            }

            FillRect(texture, 0, 0, width, 62, tatami);
            FillRect(texture, 0, height - 86, width, 86, transom);
            FillRect(texture, 0, height - 100, width, 10, wood);
            FillRect(texture, 0, 0, width, 10, wood);
            FillRect(texture, 0, 0, 12, height, wood);
            FillRect(texture, width - 12, 0, 12, height, wood);
            FillRect(texture, 0, 420, width, 4, wood);
            FillRect(texture, 272, 62, 6, 358, line);
            FillRect(texture, 548, 62, 6, 358, line);
            FillRect(texture, 764, 62, 6, 358, line);
            DrawKnob(texture, 906, 278);

            texture.Apply();
            return texture;
        }

        private static void FillRect(Texture2D texture, int startX, int startY, int rectWidth, int rectHeight, Color color)
        {
            for (var y = Mathf.Max(0, startY); y < Mathf.Min(texture.height, startY + rectHeight); y++)
            {
                for (var x = Mathf.Max(0, startX); x < Mathf.Min(texture.width, startX + rectWidth); x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        private static void DrawEllipse(Texture2D texture, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            for (var y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                if (y < 0 || y >= texture.height)
                {
                    continue;
                }

                var dy = (y - centerY) / (float)radiusY;
                for (var x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    if (x < 0 || x >= texture.width)
                    {
                        continue;
                    }

                    var dx = (x - centerX) / (float)radiusX;
                    if ((dx * dx) + (dy * dy) <= 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawLine(Texture2D texture, Vector2 start, Vector2 end, int thickness, Color color)
        {
            var steps = Mathf.CeilToInt(Vector2.Distance(start, end) * 1.5f);
            for (var i = 0; i <= steps; i++)
            {
                var point = Vector2.Lerp(start, end, i / (float)steps);
                DrawEllipse(texture, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), thickness, thickness, color);
            }
        }

        private static void DrawKnob(Texture2D texture, int centerX, int centerY)
        {
            DrawEllipse(texture, centerX, centerY, 5, 5, new Color(0.18f, 0.18f, 0.18f, 1f));
            DrawEllipse(texture, centerX + 1, centerY + 1, 2, 2, new Color(0.45f, 0.45f, 0.45f, 1f));
        }

        private static void HideMouthTarget()
        {
            var target = GameObject.Find("Mouth Target");
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        private static void RemoveTopRightLanguageWidget()
        {
            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in texts)
            {
                if (text == null || !MatchesLanguageLabel(text.text))
                {
                    continue;
                }

                var selectable = text.GetComponentInParent<Selectable>();
                var target = selectable != null ? selectable.gameObject : text.gameObject;
                var rect = target.GetComponent<RectTransform>();
                if (rect != null && IsTopRightWidget(rect))
                {
                    Destroy(target);
                }
            }
        }

        private static bool MatchesLanguageLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            return String.Equals(trimmed, "Japanese", StringComparison.Ordinal)
                || String.Equals(trimmed, "English", StringComparison.Ordinal)
                || String.Equals(trimmed, "日本語", StringComparison.Ordinal)
                || String.Equals(trimmed, "한국어", StringComparison.Ordinal)
                || String.Equals(trimmed, "中文(简体)", StringComparison.Ordinal)
                || String.Equals(trimmed, "Français", StringComparison.Ordinal)
                || String.Equals(trimmed, "Español", StringComparison.Ordinal);
        }

        private static bool IsTopRightWidget(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var minX = corners[0].x;
            var minY = corners[0].y;
            var maxX = corners[2].x;
            var maxY = corners[2].y;
            var width = maxX - minX;
            var height = maxY - minY;

            if (width <= 0f || height <= 0f || width > 420f || height > 220f)
            {
                return false;
            }

            return minX >= Screen.width * 0.78f
                && minY >= Screen.height * 0.78f
                && maxX <= Screen.width + 12f
                && maxY <= Screen.height + 12f;
        }
    }
}
