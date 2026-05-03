using ChopChop.Levels;
using ChopChop.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChopChop.App
{
    public sealed class AppRuntime : MonoBehaviour
    {
        private static AppRuntime instance;

        private readonly SceneNavigator navigator = new SceneNavigator();
        private GameLocalization localization;
        private LevelCatalog levelCatalog;
        private GameObject currentScreen;
        private string builtSceneName;

        public static AppRuntime Instance => instance;
        public SceneNavigator Navigator => navigator;
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
        }

        private void DestroyCurrentScreen()
        {
            if (currentScreen != null)
            {
                Destroy(currentScreen);
                currentScreen = null;
            }
        }
    }
}
