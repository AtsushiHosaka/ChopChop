using UnityEngine;

namespace ChopChop.Multiplayer
{
    [AddComponentMenu("ChopChop/Multiplayer/Split Screen Camera Layout")]
    public sealed class SplitScreenCameraLayout : MonoBehaviour
    {
        [SerializeField] private Camera playerOneCamera;
        [SerializeField] private Camera playerTwoCamera;
        [Range(0.25f, 0.75f)]
        [SerializeField] private float splitNormalized = 0.5f;
        [SerializeField] private bool showDivider = true;
        [SerializeField] private Color dividerColor = new Color(0f, 0f, 0f, 0.86f);
        [SerializeField] private int dividerPixels = 6;

        public void Configure(Camera playerOne, Camera playerTwo)
        {
            playerOneCamera = playerOne;
            playerTwoCamera = playerTwo;
            if (isActiveAndEnabled)
            {
                ApplyLayout();
            }
        }

        public void ApplyLayout()
        {
            if (playerOneCamera != null)
            {
                playerOneCamera.rect = new Rect(0f, 0f, splitNormalized, 1f);
                playerOneCamera.depth = 0f;
            }

            if (playerTwoCamera != null)
            {
                playerTwoCamera.rect = new Rect(splitNormalized, 0f, 1f - splitNormalized, 1f);
                playerTwoCamera.depth = 0f;
            }
        }

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && isActiveAndEnabled)
            {
                ApplyLayout();
            }
        }

        private void OnGUI()
        {
            if (!showDivider)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = dividerColor;
            var x = Mathf.RoundToInt(Screen.width * splitNormalized - dividerPixels * 0.5f);
            GUI.DrawTexture(new Rect(x, 0f, dividerPixels, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
