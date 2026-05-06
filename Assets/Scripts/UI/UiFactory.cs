using System;
using ChopChop.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public static class UiFactory
    {
        private static Font defaultFont;
        private static Sprite roundedSurfaceSprite;

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
            var image = buttonObject.GetComponent<Image>();
            image.sprite = RoundedSurfaceSprite();
            image.type = Image.Type.Sliced;
            image.color = background;

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Blend(background, new Color(1f, 0.82f, 0.44f, background.a), 0.18f);
            colors.pressedColor = Blend(background, Color.black, 0.2f);
            colors.disabledColor = new Color(0.12f, 0.105f, 0.09f, 0.58f);
            button.colors = colors;
            button.targetGraphic = image;

            AddButtonChrome(buttonObject.transform);

            var text = CreateText(buttonObject.transform, "Label", 28, FontStyle.Bold, TextAnchor.MiddleCenter, foreground);
            text.text = label;
            Stretch(text.rectTransform, 12f);

            buttonObject.AddComponent<UiButtonAnimator>();
            buttonObject.AddComponent<UiButtonAudio>();
            return button;
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

            var layoutElement = EnsureLayoutElement(rectTransform);

            layoutElement.minWidth = width;
            layoutElement.minHeight = height;
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = height;
        }

        public static void SetPreferredHeight(RectTransform rectTransform, float height)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);

            var layoutElement = EnsureLayoutElement(rectTransform);
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = -1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
        }

        public static void SetFlexibleFill(RectTransform rectTransform)
        {
            var layoutElement = EnsureLayoutElement(rectTransform);
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = -1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 0f;
            layoutElement.preferredHeight = -1f;
            layoutElement.flexibleHeight = 1f;
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

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return defaultFont;
        }

        private static LayoutElement EnsureLayoutElement(RectTransform rectTransform)
        {
            var layoutElement = rectTransform.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            }

            return layoutElement;
        }

        private static void AddButtonChrome(Transform parent)
        {
            var topHighlight = CreatePanel(parent, "Top Highlight", new Color(1f, 0.86f, 0.5f, 0.18f));
            Anchor(topHighlight, new Vector2(0f, 1f), Vector2.one, new Vector2(12f, -3f), new Vector2(-12f, 0f));
            topHighlight.GetComponent<Image>().raycastTarget = false;

            var bottomShadow = CreatePanel(parent, "Bottom Shadow", new Color(0f, 0f, 0f, 0.24f));
            Anchor(bottomShadow, Vector2.zero, new Vector2(1f, 0f), new Vector2(12f, 0f), new Vector2(-12f, 3f));
            bottomShadow.GetComponent<Image>().raycastTarget = false;

            var accent = CreatePanel(parent, "Gold Accent", new Color(0.94f, 0.64f, 0.22f, 0.9f));
            Anchor(accent, new Vector2(0f, 0.18f), new Vector2(0f, 0.82f), new Vector2(0f, 0f), new Vector2(5f, 0f));
            accent.GetComponent<Image>().raycastTarget = false;
        }

        private static Sprite RoundedSurfaceSprite()
        {
            if (roundedSurfaceSprite != null)
            {
                return roundedSurfaceSprite;
            }

            const int size = 64;
            const int radius = 7;
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                name = "ChopChop Rounded UI Surface",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x < radius ? radius - x : x >= size - radius ? x - (size - radius - 1) : 0;
                    var dy = y < radius ? radius - y : y >= size - radius ? y - (size - radius - 1) : 0;
                    var outsideCorner = dx > 0 && dy > 0 && dx * dx + dy * dy > radius * radius;
                    pixels[y * size + x] = outsideCorner ? new Color32(255, 255, 255, 0) : Color.white;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            roundedSurfaceSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            roundedSurfaceSprite.hideFlags = HideFlags.HideAndDontSave;
            return roundedSurfaceSprite;
        }

        private static Color Blend(Color from, Color to, float amount)
        {
            return new Color(
                Mathf.Lerp(from.r, to.r, amount),
                Mathf.Lerp(from.g, to.g, amount),
                Mathf.Lerp(from.b, to.b, amount),
                Mathf.Lerp(from.a, to.a, amount));
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
