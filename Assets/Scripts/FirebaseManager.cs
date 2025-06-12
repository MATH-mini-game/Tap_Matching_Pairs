using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    // Accès public à la référence DB
    public DatabaseReference DbReference => dbReference;

    public static FirebaseManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("Initialisation de Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase est prêt.");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                TestWriteAndRead();
            }
            else
            {
                Debug.LogError($"Erreur de dépendances Firebase : {dependencyStatus}");
            }
        });
    }

    void TestWriteAndRead()
    {
        string testKey = "unityTest";
        string testValue = "Bonjour Firebase";

        dbReference.Child(testKey).SetValueAsync(testValue).ContinueWithOnMainThread(writeTask =>
        {
            if (writeTask.IsCompleted)
            {
                Debug.Log("Écriture réussie dans Firebase.");
                dbReference.Child(testKey).GetValueAsync().ContinueWithOnMainThread(readTask =>
                {
                    if (readTask.IsCompleted)
                    {
                        DataSnapshot snapshot = readTask.Result;
                        Debug.Log($"Lecture réussie depuis Firebase : {snapshot.Value}");
                    }
                    else
                    {
                        Debug.LogError("Erreur lors de la lecture depuis Firebase.");
                    }
                });
            }
            else
            {
                Debug.LogError("Erreur lors de l'écriture dans Firebase.");
            }
        });
    }
}
