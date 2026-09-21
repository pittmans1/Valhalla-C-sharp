using UnityEngine;
using UnityEngine.SceneManagement; //cruical for scene routing and managment

public class MenuController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}