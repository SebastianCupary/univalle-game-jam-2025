using System.Collections.Generic;
using UnityEngine;

// Gestiona el progreso de niveles desbloqueados y lo persiste con PlayerPrefs.
// Debe existir en alguna escena de arranque (p.ej. menú) o se crea on-demand (ver GetOrCreate).
public class LevelProgressManager : MonoBehaviour
{
 public static LevelProgressManager Instance { get; private set; }

 [Tooltip("Orden de escenas para progresión (primera siempre desbloqueada si no hay datos)")] public List<string> levelOrder = new List<string>();

 private HashSet<string> _unlocked = new HashSet<string>();
 private const string PrefKey = "levels.unlocked"; // formato: scene1,scene2,...

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

 private void Load()
 {
 _unlocked.Clear();
 string raw = PlayerPrefs.GetString(PrefKey, string.Empty);
 if (!string.IsNullOrEmpty(raw))
 {
 var parts = raw.Split(',');
 for (int i =0; i < parts.Length; i++)
 {
 string s = parts[i].Trim();
 if (!string.IsNullOrEmpty(s)) _unlocked.Add(s);
 }
 }
 }

 private void Save()
 {
 string raw = string.Join(",", _unlocked);
 PlayerPrefs.SetString(PrefKey, raw);
 PlayerPrefs.Save();
 }
}
