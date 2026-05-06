using System;
using ChopChop.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public static class UiFactory
    {
        private const string PreferredFontResourcePath = "Fonts/YujiSyuku-Regular";
        private static Font defaultFont;

        public static GameObject CreateScreen(string name, Color background, bool drawBackground)
        {
            EnsureEventSystem();

            var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (drawBackground)
            {
                var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
                backgroundObject.transform.SetParent(root.transform, false);
                Stretch(backgroundObject.GetComponent<RectTransform>(), 0f);
                backgroundObject.GetComponent<Image>().color = background;
            }

            AddFadeSlideIn(root, 0.22f, 0f, Vector2.zero, 1f);
            return root;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel.GetComponent<RectTransform>();
        }

        public static Text CreateText(Transform parent, string name, int fontSize, FontStyle style, TextAnchor anchor, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var text = textObject.GetComponent<Text>();
            text.font = DefaultFont();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Color background, Color foreground)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = background;

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = background * 1.12f;
            colors.pressedColor = background * 0.82f;
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.55f);
            button.colors = colors;

            var text = CreateText(buttonObject.transform, "Label", 28, FontStyle.Bold, TextAnchor.MiddleCenter, foreground);
            text.text = label;
            Stretch(text.rectTransform, 12f);

            buttonObject.AddComponent<UiButtonAnimator>();
            buttonObject.AddComponent<UiButtonAudio>();
            return button;
        }

        public static Button CreateSpriteButton(
            Transform parent,
            string name,
            Sprite normalSprite,
            Sprite highlightedSprite = null,
            Sprite pressedSprite = null,
            Sprite disabledSprite = null)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.sprite = normalSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            button.targetGraphic = image;
            button.spriteState = new SpriteState
            {
                highlightedSprite = highlightedSprite != null ? highlightedSprite : normalSprite,
                pressedSprite = pressedSprite != null ? pressedSprite : highlightedSprite != null ? highlightedSprite : normalSprite,
                disabledSprite = disabledSprite != null ? disabledSprite : normalSprite
            };

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.74f);
            button.colors = colors;

            buttonObject.AddComponent<UiButtonAnimator>();
            buttonObject.AddComponent<UiButtonAudio>();
            return button;
        }

        public static Image CreateSpriteImage(Transform parent, string name, Sprite sprite, bool preserveAspect = true)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            ApplySprite(image, sprite, preserveAspect);
            image.raycastTarget = false;
            return image;
        }

        public static void ApplySprite(Image image, Sprite sprite, bool preserveAspect = true)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            image.preserveAspect = preserveAspect;
        }

        public static Slider CreateProgressBar(Transform parent, string name, Color background, Color fill)
        {
            var sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            var slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;

            var backgroundRect = CreatePanel(sliderObject.transform, "Background", background);
            Stretch(backgroundRect, 0f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>(), 0f);

            var fillRect = CreatePanel(fillArea.transform, "Fill", fill);
            Stretch(fillRect, 0f);
            slider.fillRect = fillRect;
            slider.targetGraphic = fillRect.GetComponent<Image>();
            sliderObject.AddComponent<UiSliderValueAnimator>();
            return slider;
        }

        public static UiFadeSlideIn AddFadeSlideIn(GameObject target, float duration, float delay, Vector2 offset, float startScale = 0.98f)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            var animation = target.GetComponent<UiFadeSlideIn>();
            if (animation == null)
            {
                animation = target.AddComponent<UiFadeSlideIn>();
            }

            animation.Configure(duration, delay, offset, startScale);
            animation.Play();
            return animation;
        }

        public static VerticalLayoutGroup AddVerticalLayout(GameObject target, float spacing, TextAnchor alignment, RectOffset padding)
        {
            var layout = target.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(GameObject target, float spacing, TextAnchor alignment, RectOffset padding)
        {
            var layout = target.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static void SetSize(RectTransform rectTransform, float width, float height)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static void Stretch(RectTransform rectTransform, float margin)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(margin, margin);
            rectTransform.offsetMax = new Vector2(-margin, -margin);
        }

        public static void Anchor(RectTransform rectTransform, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static Font DefaultFont()
        {
            if (defaultFont != null)
            {
                return defaultFont;
            }

            defaultFont = Resources.Load<Font>(PreferredFontResourcePath);
            if (defaultFont != null)
            {
                return defaultFont;
            }

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return defaultFont;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            var inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null)
            {
                var module = eventSystem.AddComponent(inputSystemModule);
                var assignDefaults = inputSystemModule.GetMethod(
                    "AssignDefaultActions",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                assignDefaults?.Invoke(module, null);
            }
            else
            {
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
