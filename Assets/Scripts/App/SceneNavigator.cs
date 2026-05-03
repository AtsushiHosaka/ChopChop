using UnityEngine;
using UnityEngine.SceneManagement;
using ChopChop.Levels;

namespace ChopChop.App
{
    public sealed class SceneNavigator
    {
        public void LoadStart()
        {
            Load(AppScenes.Start);
        }

        public void LoadLevelSelect()
        {
            Load(AppScenes.LevelSelect);
        }

        public void LoadLevel(string sceneName)
        {
            Load(sceneName);
        }

        public void LoadResult(LevelRunResult result, string retrySceneName)
        {
            LevelResultStore.Set(result, retrySceneName);
            Load(AppScenes.Result);
        }

        public void LoadVersusResult(VersusLevelRunResult result, string retrySceneName)
        {
            LevelResultStore.SetVersus(result, retrySceneName);
            Load(AppScenes.Result);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("[ChopChop] Quit requested.");
#else
            Application.Quit();
#endif
        }

        private static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[ChopChop] Cannot load an empty scene name.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
