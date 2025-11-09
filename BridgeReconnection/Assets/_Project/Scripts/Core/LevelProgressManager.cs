using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Gestiona el progreso de niveles desbloqueados y lo persiste en JSON
// Archivo en: Application.persistentDataPath/levels_progress.json
public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    [Tooltip("Orden de escenas para progresión (primera siempre desbloqueada si no hay datos)")]
    public List<string> levelOrder = new List<string>();

    [Serializable]
    private class ProgressData
    {
        public List<string> unlocked = new List<string>();
    }

    private HashSet<string> _unlocked = new HashSet<string>();
    private const string JsonFileName = "levels_progress.json";
    private const string LegacyPrefKey = "levels.unlocked"; // para migración desde PlayerPrefs

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        // Asegurar primer nivel desbloqueado
        if (levelOrder.Count >0 && _unlocked.Count ==0)
        {
            _unlocked.Add(levelOrder[0]);
            Save();
        }
    }

    public static LevelProgressManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("LevelProgressManager");
        return go.AddComponent<LevelProgressManager>();
    }

    public bool IsUnlocked(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        return _unlocked.Contains(sceneName);
    }

    public bool Unlock(string sceneName, bool alsoSave = true)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        if (_unlocked.Add(sceneName))
        {
            if (alsoSave) Save();
            return true;
        }
        return false; // ya estaba
    }

    public void UnlockNext(string currentScene)
    {
        if (levelOrder == null || levelOrder.Count ==0) return;
        int idx = levelOrder.IndexOf(currentScene);
        if (idx >=0 && idx +1 < levelOrder.Count)
        {
            Unlock(levelOrder[idx +1]);
        }
    }

    public void ResetProgress(bool keepFirst = true)
    {
        _unlocked.Clear();
        if (keepFirst && levelOrder.Count >0) _unlocked.Add(levelOrder[0]);
        Save();
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, JsonFileName);
    }

    private void Load()
    {
        _unlocked.Clear();
        string path = GetSavePath();
        try
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonUtility.FromJson<ProgressData>(json);
                    if (data != null && data.unlocked != null)
                    {
                        for (int i =0; i < data.unlocked.Count; i++)
                        {
                            string s = data.unlocked[i];
                            if (!string.IsNullOrEmpty(s)) _unlocked.Add(s);
                        }
                    }
                }
            }
            else
            {
                // Migrar desde PlayerPrefs (legacy) si existe
                string raw = PlayerPrefs.GetString(LegacyPrefKey, string.Empty);
                if (!string.IsNullOrEmpty(raw))
                {
                    var parts = raw.Split(',');
                    for (int i =0; i < parts.Length; i++)
                    {
                        string s = parts[i].Trim();
                        if (!string.IsNullOrEmpty(s)) _unlocked.Add(s);
                    }
                    Save(); // guardar inmediatamente en JSON
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LevelProgressManager Load error: {e.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var data = new ProgressData { unlocked = new List<string>(_unlocked) };
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LevelProgressManager Save error: {e.Message}");
        }
    }
}
