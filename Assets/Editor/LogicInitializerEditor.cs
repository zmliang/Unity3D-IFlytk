using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using JinkeGroup.Util;

namespace JinkeGroup.Logic
{
    [InitializeOnLoad]
    public static class LogicInitializerEditor
    {
        // Constants
        private const string PreviousSceneKey = "UnityEditorInitializerPreviousSceneKey";
        private const string StartUpScenePath = "Assets/Scenes/Main.unity";
        private const string PreviousLevelBlockKey = "UnityEditorInitializerPreviousLevelBlockKey";
        private static float SaveAllTimeout = 1.0f;

        private static float LastSaveAllTime = 0.0f;
        private static Stopwatch Timer = Stopwatch.StartNew();

        private static Object LastSelection = null;

        static LogicInitializerEditor()
        {
            EditorApplication.playmodeStateChanged -= onPlaymodeStateChanged;
            EditorApplication.playmodeStateChanged += onPlaymodeStateChanged;
            EditorApplication.update += OnUpdate;
        }

        private static void onPlaymodeStateChanged()
        {
            // Play pressed to start playing
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                string path = SceneManager.GetActiveScene().path;
                PlayerPrefs.SetString(PreviousSceneKey, path);
                PlayerPrefs.Save();
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

                bool runStartUpScene = string.IsNullOrEmpty(path);

                if (!runStartUpScene)
                {
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorApplication.isPlaying = false;
                        return;
                    }
                }

                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].path == path)
                    {
                        runStartUpScene = true;
                        break;
                    }
                }

                if (runStartUpScene && File.Exists(StartUpScenePath))
                {
                    EditorSceneManager.OpenScene(StartUpScenePath);
                }
            }
            // Change to the scene that was launched from
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                string scene = PlayerPrefs.GetString(PreviousSceneKey);
                if (!string.IsNullOrEmpty(scene))
                {
                    EditorSceneManager.OpenScene(scene);
                }
            }
        }

        private static void OnUpdate()
        {
            if (LastSelection != Selection.activeObject)
            {
                string path = LastSelection == null ? string.Empty : AssetDatabase.GetAssetPath(LastSelection);
                LastSelection = Selection.activeObject;
                if (!string.IsNullOrEmpty(path))
                {
                    OnSaveAll();
                }
            }
        }

        private static void OnSaveAll()
        {
            if (Application.isPlaying)
            {
                return;
            }
            float time = (float)Timer.Elapsed.TotalSeconds;
            if ((time - LastSaveAllTime) > SaveAllTimeout)
            {
                LastSaveAllTime = time;
                JinkeGroup.Util.Logger.Warn("Saving all!");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
