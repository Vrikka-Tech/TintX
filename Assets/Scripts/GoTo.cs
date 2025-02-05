using UnityEngine;

public class GoTo : MonoBehaviour
{
    public void RedirectToScene(string sceneName) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
