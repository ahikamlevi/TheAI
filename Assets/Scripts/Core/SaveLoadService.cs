using System.IO;
using TheAI.Models;
using UnityEngine;

namespace TheAI.Core
{
    public class SaveLoadService
    {
        public void SaveGame(GlobalGameState state, string path)
        {
            if (state == null)
            {
                Debug.LogWarning("Cannot save because the game state is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogWarning("Cannot save because the provided path is invalid.");
                return;
            }

            var json = JsonUtility.ToJson(state, true);
            File.WriteAllText(path, json);
            Debug.Log($"Game saved to {path}");
        }

        public GlobalGameState LoadGame(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogWarning("Cannot load because the provided path is invalid.");
                return null;
            }

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Save file not found at path: {path}");
                return null;
            }

            var json = File.ReadAllText(path);
            var state = JsonUtility.FromJson<GlobalGameState>(json);
            Debug.Log($"Game loaded from {path}");
            return state;
        }
    }
}
