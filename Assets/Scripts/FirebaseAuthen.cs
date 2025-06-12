using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class FirebaseAuthen : MonoBehaviour
{
    private DatabaseReference dbReference;

    [Header("UI Elements")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    public Button closeButton;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialisé avec succès");
            }
            else
            {
                Debug.LogError("Impossible d'initialiser Firebase: " + task.Result);
                ShowPopup("Erreur d'initialisation Firebase.");
            }
        });

        popupPanel.SetActive(false);
        closeButton.onClick.AddListener(ClosePopup);
    }

    public void RechercherUtilisateur(string uid)
    {
        Debug.Log($"Recherche utilisateur uid={uid}");

        FirebaseDatabase.DefaultInstance.GetReference("users").Child(uid).GetValueAsync().ContinueWith(taskUser => {
            if (taskUser.IsCompleted && taskUser.Result.Exists)
            {
                DataSnapshot userSnapshot = taskUser.Result;
                string role = userSnapshot.Child("role").Value?.ToString();
                UserSession.userId = uid;
                if (role == "Student")
                {
                    string schoolGradeStr = userSnapshot.Child("schoolGrade").Value?.ToString();
                    if (!int.TryParse(schoolGradeStr, out int schoolGrade))
                    {
                        ShowPopup("Grade scolaire invalide dans la base de données.");
                        return;
                    }

                    FirebaseDatabase.DefaultInstance.GetReference("tests").GetValueAsync().ContinueWith(taskTests => {
                        if (taskTests.IsCompleted && taskTests.Result.Exists)
                        {
                            bool testFound = false;

                            foreach (DataSnapshot testSnapshot in taskTests.Result.Children)
                            {
                                if (int.TryParse(testSnapshot.Child("grade").Value.ToString(), out int testGrade))
                                {
                                    if (testGrade == schoolGrade)
                                    {
                                        testFound = true;
                                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                            UserSession.testId = testSnapshot.Key;
                                            SceneManager.LoadScene("MainMenu");
                                        });
                                        break;
                                    }
                                }
                            }

                            if (!testFound)
                                ShowPopup("Aucun test disponible pour votre niveau scolaire.");
                        }
                        else
                        {
                            ShowPopup("Erreur lors de la vérification des tests.");
                        }
                    });
                }
                else
                {
                    ShowPopup("Seuls les élèves peuvent accéder à ce jeu.");
                }
            }
            else
            {
                ShowPopup("Utilisateur introuvable dans la base Firebase.");
            }
        });
    }

    void ShowPopup(string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            popupText.text = message;
            popupPanel.SetActive(true);
        });
    }

    void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
