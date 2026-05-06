using System.Collections;
using ChopChop.Levels;
using ChopChop.Audio;
using ChopChop.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChopChop.App
{
    public sealed class AppRuntime : MonoBehaviour
    {
        private static AppRuntime instance;

        private readonly SceneNavigator navigator = new SceneNavigator();
        private AudioService audioService;
        private GameLocalization localization;
        private LevelCatalog levelCatalog;
        private GameObject currentScreen;
        private string builtSceneName;
        private Coroutine bgmUpdateCoroutine;

        public static AppRuntime Instance => instance;
        public SceneNavigator Navigator => navigator;
        public AudioService Audio => audioService;
        public GameLocalization Localization => localization;
        public LevelCatalog LevelCatalog => levelCatalog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            var root = new GameObject("[ChopChop App Runtime]");
            instance = root.AddComponent<AppRuntime>();
            DontDestroyOnLoad(root);
        }

        private void Awake()
        {
            audioService = GetComponent<AudioService>();
            if (audioService == null)
            {
                audioService = gameObject.AddComponent<AudioService>();
            }

            audioService.Initialize();
            localization = new GameLocalization();
            levelCatalog = LevelCatalogLoader.Load();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            BuildForScene(SceneManager.GetActiveScene().name);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BuildForScene(scene.name);
        }

        private void BuildForScene(string sceneName)
        {
            if (builtSceneName == sceneName && currentScreen != null)
            {
                return;
            }

            DestroyCurrentScreen();
            builtSceneName = sceneName;
            UpdateBgmForScene(sceneName);

            if (sceneName == AppScenes.Start)
            {
                currentScreen = StartScreenController.Create(localization, navigator);
            }
            else if (sceneName == AppScenes.LevelSelect)
            {
                currentScreen = LevelSelectScreenController.Create(localization, navigator, levelCatalog);
            }
            else if (sceneName == AppScenes.SukiyakiLevel)
            {
                currentScreen = SukiyakiScreenController.Create(localization, navigator);
            }
            else if (sceneName == AppScenes.Result)
            {
                currentScreen = ResultScreenController.Create(localization, navigator, LevelResultStore.CurrentResult, LevelResultStore.RetrySceneName);
            }
            else if (TryFindLevel(sceneName, out var levelDefinition))
            {
                currentScreen = PrototypeLevelScreenController.Create(localization, navigator, levelDefinition);
            }
        }

        private void UpdateBgmForScene(string sceneName)
        {
            if (audioService == null)
            {
                return;
            }

            if (bgmUpdateCoroutine != null)
            {
                StopCoroutine(bgmUpdateCoroutine);
            }

            bgmUpdateCoroutine = StartCoroutine(ApplyBgmAfterSceneLoad(sceneName));
        }

        private IEnumerator ApplyBgmAfterSceneLoad(string sceneName)
        {
            // Delay playback until the loaded scene and audio stack have fully settled.
            yield return null;
            yield return new WaitForEndOfFrame();

            if (audioService != null)
            {
                audioService.PlayBgm(sceneName == AppScenes.SukiyakiLevel
                    ? AudioBgmId.Sukiyaki
                    : AudioBgmId.Menu);
            }

            bgmUpdateCoroutine = null;
        }

        private void DestroyCurrentScreen()
        {
            if (currentScreen != null)
            {
                Destroy(currentScreen);
                currentScreen = null;
            }
        }

        private bool TryFindLevel(string sceneName, out LevelDefinition levelDefinition)
        {
            levelDefinition = null;
            if (levelCatalog == null)
            {
                return false;
            }

            foreach (var level in levelCatalog.Levels)
            {
                if (level != null && level.sceneName == sceneName)
                {
                    levelDefinition = level;
                    return true;
                }
            }

            return false;
        }
    }
}
