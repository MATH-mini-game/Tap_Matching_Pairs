using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class GameManager : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform gridParent;
    public GameObject successPanel;
    public TMP_Text endMessageText;
    public Button nextButton;
    public Button restartButton;
    public Button returnToMenuButton; 
    public TMP_Text levelText;
    public TMP_Text timerText;

    private float elapsedTime;
    private bool isTimerRunning = false;

    public AudioSource audioSource;
    public AudioClip clickClip;
    public AudioClip successClip;
    public AudioClip failClip;

    private bool isSoundEnabled => SoundManager.isSoundEnabled;

    private List<Button> createdButtons = new();
    private Dictionary<int, string> arabicNumbers = new()
    {
        {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"},
        {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"},
        {11, "eleven"}, {12, "twelve"}, {13, "thirteen"}, {14, "fourteen"}, {15, "fifteen"},
        {16, "sixteen"}, {17, "seventeen"}, {18, "eighteen"}, {19, "nineteen"}, {20, "twenty"},
        {21, "twenty-one"}, {22, "twenty-two"}, {23, "twenty-three"}, {24, "twenty-four"}, {25, "twenty-five"},
        {26, "twenty-six"}, {27, "twenty-seven"}, {28, "twenty-eight"}, {29, "twenty-nine"}, {30, "thirty"},
        {31, "thirty-one"}, {32, "thirty-two"}, {33, "thirty-three"}, {34, "thirty-four"}, {35, "thirty-five"},
        {36, "thirty-six"}, {37, "thirty-seven"}, {38, "thirty-eight"}, {39, "thirty-nine"}, {40, "forty"},
        {41, "forty-one"}, {42, "forty-two"}, {43, "forty-three"}, {44, "forty-four"}, {45, "forty-five"},
        {46, "forty-six"}, {47, "forty-seven"}, {48, "forty-eight"}, {49, "forty-nine"}, {50, "fifty"},
        {51, "fifty-one"}, {52, "fifty-two"}, {53, "fifty-three"}, {54, "fifty-four"}, {55, "fifty-five"},
        {56, "fifty-six"}, {57, "fifty-seven"}, {58, "fifty-eight"}, {59, "fifty-nine"}, {60, "sixty"},
        {61, "sixty-one"}, {62, "sixty-two"}, {63, "sixty-three"}, {64, "sixty-four"}, {65, "sixty-five"},
        {66, "sixty-six"}, {67, "sixty-seven"}, {68, "sixty-eight"}, {69, "sixty-nine"}, {70, "seventy"},
        {71, "seventy-one"}, {72, "seventy-two"}, {73, "seventy-three"}, {74, "seventy-four"}, {75, "seventy-five"},
        {76, "seventy-six"}, {77, "seventy-seven"}, {78, "seventy-eight"}, {79, "seventy-nine"}, {80, "eighty"},
        {81, "eighty-one"}, {82, "eighty-two"}, {83, "eighty-three"}, {84, "eighty-four"}, {85, "eighty-five"},
        {86, "eighty-six"}, {87, "eighty-seven"}, {88, "eighty-eight"}, {89, "eighty-nine"}, {90, "ninety"},
        {91, "ninety-one"}, {92, "ninety-two"}, {93, "ninety-three"}, {94, "ninety-four"}, {95, "ninety-five"},
        {96, "ninety-six"}, {97, "ninety-seven"}, {98, "ninety-eight"}, {99, "ninety-nine"}
    };

    private string firstSelection = null;
    private Button firstButton = null;

    private int matchedPairs = 0;
    private int totalPairs = 0;

    private int currentLevel = 1;
    private int correctMatches = 0;
    private int totalAttempts = 0;

    private Dictionary<int, int> levelThresholds = new()
    {
        {1, 50}, {2, 75}, {3, 75}, {4, 75}, {5, 75}, {6, 100}
    };

    void Start()
    {
        StartLevel(currentLevel);
        returnToMenuButton.gameObject.SetActive(false);
    }

    void StartLevel(int level)
    {
        ClearGrid();
        correctMatches = 0;
        totalAttempts = 0;
        matchedPairs = 0;
        levelText.text = $"Level {level}";

        elapsedTime = 0f;
        isTimerRunning = true;

        LoadMinMaxFromFirebase(level);
    }

    void ClearGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);
        createdButtons.Clear();
    }

    void CreateButtons(int numPairs, int min, int max)
    {
        totalPairs = numPairs;
        List<int> availableNumbers = new();

        for (int i = min; i <= max; i++)
        {
            if (arabicNumbers.ContainsKey(i))
                availableNumbers.Add(i);
        }

        availableNumbers = Shuffle(availableNumbers);
        availableNumbers = availableNumbers.GetRange(0, Mathf.Min(numPairs, availableNumbers.Count));

        List<string> texts = new();

        foreach (int n in availableNumbers)
        {
            texts.Add(n.ToString());
            texts.Add(arabicNumbers[n]);
        }

        texts = Shuffle(texts);

        foreach (string text in texts)
        {
            GameObject btn = Instantiate(buttonPrefab, gridParent);
            TMP_Text tmpText = btn.GetComponentInChildren<TMP_Text>();
            tmpText.text = text;
            tmpText.fontSize = 50;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.black;

            Button uiButton = btn.GetComponent<Button>();
            uiButton.onClick.AddListener(() => OnButtonClick(text, uiButton));
            createdButtons.Add(uiButton);
        }
    }

    void OnButtonClick(string content, Button button)
    {
        if (!button.interactable || button == firstButton)
            return;

        if (firstSelection == null)
        {
            PlaySound(clickClip);
            firstSelection = content;
            firstButton = button;
            HighlightButton(button, Color.yellow);
        }
        else
        {
            totalAttempts++;

            if (IsMatch(firstSelection, content))
            {
                HighlightButton(firstButton, Color.green);
                HighlightButton(button, Color.green);
                firstButton.interactable = false;
                button.interactable = false;
                matchedPairs++;
                correctMatches++;

                if (matchedPairs == totalPairs)
                {
                    int scorePercent = Mathf.RoundToInt((float)correctMatches / totalAttempts * 100);
                    int requiredScore = levelThresholds[currentLevel];

                    SaveScore(scorePercent);
                    if (scorePercent >= requiredScore)
                    {
                        string message = currentLevel == 6
                            ? $"Dernier niveau atteint!\nScore : {scorePercent}%"
                            : $"Niveau suivant!\nScore : {scorePercent}%";

                        ShowEndMessage(message, true);
                        PlaySound(successClip);
                    }

                    else
                    {
                        ShowEndMessage($"Fin du jeu\nScore insuffisant : {scorePercent}%", false);
                        PlaySound(failClip);
                    }
                }
            }
            else
            {
                HighlightButton(firstButton, Color.red);
                HighlightButton(button, Color.red);
                Invoke(nameof(ResetSelections), 1f);
            }

            firstSelection = null;
            firstButton = null;
        }
    }

    bool IsMatch(string a, string b)
    {
        foreach (var pair in arabicNumbers)
        {
            if ((a == pair.Key.ToString() && b == pair.Value) || (b == pair.Key.ToString() && a == pair.Value))
                return true;
        }
        return false;
    }

    void ResetSelections()
    {
        foreach (var btn in createdButtons)
        {
            if (btn.interactable)
                HighlightButton(btn, Color.white);
        }
    }

    void HighlightButton(Button btn, Color color)
    {
        var image = btn.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
        return list;
    }

    void ShowEndMessage(string message, bool success)
    {
        isTimerRunning = false;
        successPanel.SetActive(true);
        endMessageText.text = message;

        nextButton.gameObject.SetActive(success && currentLevel < 6);
        restartButton.gameObject.SetActive(success);
        returnToMenuButton.gameObject.SetActive(!success); // ✅ Afficher uniquement en cas d’échec
    }

    public void OnNextButton()
    {
        currentLevel++;
        successPanel.SetActive(false);
        StartLevel(currentLevel);
    }

    public void OnRestartButton()
    {
        successPanel.SetActive(false);
        StartLevel(currentLevel);
    }

    public void OnReturnToMenuButton() // Méthode appelée par le bouton menu
    {
        Debug.Log("Back button clicked");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void SaveScore(int scorePercent)
    {
        float timeTaken = elapsedTime;
        string date = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"); // format ISO 8601
        ScoreEntry newEntry = new ScoreEntry(date, currentLevel, scorePercent, timeTaken);

        // Sauvegarde locale JSON
        string path = Application.persistentDataPath + "/scores.json";
        ScoreData data = LoadScoreData();
        data.scores.Add(newEntry);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        //  Enregistrement dans Firebase
        SaveScoreToFirebase(scorePercent, timeTaken, date);
    }
    void SaveScoreToFirebase(int scorePercent, float totalTime, string date)
    {
        string userId = UserSession.userId;
        string testId = UserSession.testId;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(testId))
        {
            Debug.LogError("Impossible d'enregistrer le score : userId ou testId manquant.");
            return;
        }

        int answeredLevels = currentLevel; // nombre de niveaux complétés
        int numberOfLevels = 6;
        bool passed = scorePercent >= levelThresholds[1]; // On suppose le même seuil pour tous les niveaux

        var tapMatchingPairsResult = new Dictionary<string, object>
    {
        { "answeredQuestions", answeredLevels },
        { "completedAt", date },
        { "miniGameDuration", Mathf.RoundToInt(totalTime) },
        { "numberOfQuestions", numberOfLevels },
        { "passed", passed },
        { "score", correctMatches }
    };

        string path = $"users/{userId}/testResults/{testId}/miniGames/tap_matching_pairs";

        FirebaseManager.Instance.DbReference
            .Child(path)
            .SetValueAsync(tapMatchingPairsResult)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Erreur lors de l'enregistrement du score dans Firebase.");
                }
                else
                {
                    Debug.Log("Score enregistré avec succès dans Firebase.");
                }
            });
    }


    ScoreData LoadScoreData()
    {
        string path = Application.persistentDataPath + "/scores.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ScoreData>(json);
        }
        return new ScoreData();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (isSoundEnabled && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void LoadMinMaxFromFirebase(int level)
    {
        if (string.IsNullOrEmpty(UserSession.testId))
        {
            Debug.LogError("testId non défini dans UserSession !");
            return;
        }

        string path = $"tests/{UserSession.testId}/miniGameConfigs/tap_matching_pairs/levels/level{level}";

        FirebaseManager.Instance.DbReference
            .Child(path)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.IsCompleted)
                {
                    Debug.LogError("Erreur lors du chargement du niveau depuis Firebase.");
                    return;
                }

                DataSnapshot snap = task.Result;
                int min = int.Parse(snap.Child("minNumber").Value.ToString());
                int max = int.Parse(snap.Child("maxNumber").Value.ToString());

                int numPairs = GetNumPairsForLevel(level);
                CreateButtons(numPairs, min, max);
            });
    }

    int GetNumPairsForLevel(int level)
    {
        return level switch
        {
            1 => 3,
            2 => 4,
            3 => 5,
            4 => 6,
            5 => 7,
            6 => 8,
            _ => 3
        };
    }
}
