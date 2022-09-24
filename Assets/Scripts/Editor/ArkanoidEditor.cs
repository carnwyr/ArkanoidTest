using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArkanoidTest.Editor
{
    public class ArkanoidEditor : EditorWindow
    {
        private static float _ballSpeed;
        private static float _playerSpeed;
        
        [MenuItem("Arkanoid/Config")]
        private static void Init()
        {
            LoadData();
            
            var window = GetWindow<ArkanoidEditor>(false, "Arkanoid config");
            window.Show();
        }
        
        private void OnGUI()
        {
            _ballSpeed = EditorGUILayout.Slider("Ball speed", _ballSpeed, 0, 50);
            _playerSpeed = EditorGUILayout.Slider("Player speed", _playerSpeed, 0, 50);
            if (GUILayout.Button("Save"))
            {
                SaveData();
            }
        }

        private static void LoadData()
        {
            var path = Path.Combine(Application.dataPath, "Resources", "config.json");

            if (!File.Exists(path)) return;
            
            var text = File.ReadAllText(path);
            var data = JsonUtility.FromJson<ConfigData>(text);
            _ballSpeed = data.BallSpeed;
            _playerSpeed = data.PlayerSpeed;
        }

        private static void SaveData()
        {
            var path = Path.Combine(Application.dataPath, "Resources", "config.json");
            var data = new ConfigData
            {
                BallSpeed = _ballSpeed,
                PlayerSpeed = _playerSpeed
            };
            
            var text = JsonUtility.ToJson(data);
            File.WriteAllText(path, text);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}