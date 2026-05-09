using ChopChop.App;
using ChopChop.Audio;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiScreenController : MonoBehaviour
    {
        private const string StageBackdropName = "Wa Fusuma Backdrop";

        private GameLocalization localization;
        private SceneNavigator navigator;
        private SukiyakiStageController stage;
        private Text timeValueText;
        private Text failureValueText;
        private Image timeLabelImage;
        private Image failureLabelImage;
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
            CreateHud();
            CreateCueOverlay();
            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
            StartCountdown();
        }

        private void Update()
        {
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

            CreateInfoStat(statsRow.transform, "Time", new Vector2(-84f, 0f), LocalizationKeys.Hud.Time, out timeLabelImage, out timeValueText, 108f);
            CreateInfoStat(statsRow.transform, "Failures", new Vector2(84f, 0f), LocalizationKeys.Hud.Failures, out failureLabelImage, out failureValueText, 82f);
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

            var message = UiFactory.CreateSpriteImage(content.transform, "Message", WaUiCatalog.LoadLabel(localization, "setup_missing"));
            UiFactory.SetSize(message.rectTransform, 620f, 86f);

            var backButton = UiFactory.CreateSpriteButton(content.transform, "Level Select Button",
                WaUiCatalog.ButtonSecondary, WaUiCatalog.ButtonSecondaryHover, WaUiCatalog.ButtonSecondaryPressed);
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 360f, 64f);
            var backLabel = UiFactory.CreateSpriteImage(backButton.transform, "Back Label",
                WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Result.LevelSelect));
            UiFactory.Anchor(backLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(32f, 14f), new Vector2(-32f, -14f));
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
            UiFactory.ApplySprite(timeLabelImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Hud.Time));
            UiFactory.ApplySprite(failureLabelImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Hud.Failures));
        }

        private void CreateInfoStat(Transform parent, string name, Vector2 anchoredPosition, string localizationKey, out Image labelImage, out Text valueText, float labelWidth)
        {
            var stat = new GameObject(name, typeof(RectTransform));
            stat.transform.SetParent(parent, false);
            var statRect = stat.GetComponent<RectTransform>();
            statRect.anchorMin = new Vector2(0.5f, 0f);
            statRect.anchorMax = new Vector2(0.5f, 0f);
            statRect.pivot = new Vector2(0.5f, 0f);
            statRect.anchoredPosition = anchoredPosition;
            UiFactory.SetSize(statRect, 148f, 52f);

            labelImage = UiFactory.CreateSpriteImage(stat.transform, "Label", null, false);
            var labelRect = labelImage.rectTransform;
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

            var sprite = WaUiCatalog.SukiyakiStageBackground;
            if (sprite == null)
            {
                return;
            }

            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = StageBackdropName;
            backdrop.transform.SetParent(camera.transform, false);
            backdrop.transform.localPosition = new Vector3(0f, 0.18f, 7.6f);
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
                material.SetTexture("_BaseMap", sprite.texture);
            }
            else if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", sprite.texture);
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
    }
}
