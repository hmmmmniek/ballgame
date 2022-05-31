using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System;
using System.IO;
using System.Collections.Generic;

public static class PostScene {
    [PostProcessScene]
    static void OnPostProcessScene() {
        string[] files = Directory.GetFiles("Assets/UI", "*.uxml", SearchOption.AllDirectories);
        Dictionary<string, int> occurrences = new Dictionary<string, int>();
        Directory.CreateDirectory("Resources");
        foreach (var file in files) {
            string fileName = Path.GetFileName(file);
            int value = 0;
            occurrences.TryGetValue(fileName, out value);
            occurrences[fileName] = value + 1;

            if (occurrences[fileName] > 1) {
                Debug.LogError("Duplicate template selector: " + fileName);
            }
            FileUtil.ReplaceFile(file, $"{Directory.GetCurrentDirectory()}/Assets/Resources/{fileName}");
        }

    }
}