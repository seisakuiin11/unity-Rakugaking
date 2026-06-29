using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    TitleScene,
    SelectScene,
    GameScene,
    ResultScene,
    JoinScene,
    TestScene
}

public class SceneLoder
{

    public void ChangeScene(SceneName _scene)
    {
        if (CanLoadScene(_scene)) {
            SceneManager.LoadScene(_scene.ToString());
        }
        else {
            Debug.LogError("シーンが存在しません");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    bool CanLoadScene(SceneName _scene)
    {
        int count = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);

            if (name == _scene.ToString())
                return true;
        }

        return false;
    }
}
