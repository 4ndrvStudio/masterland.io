using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace masterland.Manager
{
    public static class SceneName 
    {
        public static string Scene_Boostrap = "scene_boostrap";
        public static string Scene_Building = "scene_building";
    }

    public class SceneController : Singleton<SceneController>
    {
        public bool LoadSceneAsync(string sceneName)
        {
            if (IsSceneLoaded(sceneName)) return true;
          
            StartCoroutine(LoadSceneCoroutine(sceneName));

            return true;
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        public void UnloadSceneAsync(string sceneName)
        {
            if (!IsSceneLoaded(sceneName)) return;

            StartCoroutine(UnloadSceneCoroutine(sceneName));
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }

        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }
    }
}
