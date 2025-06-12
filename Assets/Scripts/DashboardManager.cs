using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class DashboardManager : MonoBehaviour
{
    public Transform scoreContainer;
    public GameObject scoreEntryPrefab;

    void Start()
    {
        LoadAndDisplayScores();
    }

    void LoadAndDisplayScores()
    {
        string path = Application.persistentDataPath + "/scores.json";
        if (!File.Exists(path))
        {
            Debug.Log("Aucun score trouvé.");
            return;
        }

        string json = File.ReadAllText(path);
        ScoreData data = JsonUtility.FromJson<ScoreData>(json);

        foreach (var entry in data.scores)
        {
            GameObject go = Instantiate(scoreEntryPrefab, scoreContainer);

            // Cherche chaque champ texte dans les enfants
            TMP_Text dateText = go.transform.Find("DateText")?.GetComponent<TMP_Text>();
            TMP_Text levelText = go.transform.Find("LevelText")?.GetComponent<TMP_Text>();
            TMP_Text scoreText = go.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
            TMP_Text timeText = go.transform.Find("TimeText")?.GetComponent<TMP_Text>();

            // Sécurité : vérifie que tous les composants sont trouvés
            if (dateText == null || levelText == null || scoreText == null || timeText == null)
            {
                Debug.LogError("Un ou plusieurs champs texte sont introuvables dans le prefab !");
                continue;
            }

            // Remplit les champs avec les données du score
            dateText.text = entry.date;
            levelText.text = entry.level.ToString();
            scoreText.text = $"{entry.scorePercent}%";
            timeText.text = FormatTime(entry.timeTaken);
        }
    }


    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
