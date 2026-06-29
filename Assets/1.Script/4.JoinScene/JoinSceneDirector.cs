using UnityEngine;

public class JoinSceneDirector : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) StartScene();
    }
    public void StartScene()
    {
        ProjectManager.Instance.ChangeScene(ProjectManager.Instance.NextScene);
    }
}
