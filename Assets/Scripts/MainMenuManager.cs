
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OpenDashboard()
    {
        SceneManager.LoadScene("DashboardScene");
    }
}

