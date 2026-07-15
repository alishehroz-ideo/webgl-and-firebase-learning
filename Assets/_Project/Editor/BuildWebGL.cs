using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BookLab.EditorTools
{
    // One-click WebGL builds — one menu item per task, each into its OWN folder so they never
    // overwrite each other. Each build includes ONLY that task's scene.
    //   BookLab > Build WebGL - Task 1 (BookLab) -> Build/WebGL-Task1  (SampleScene)
    //   BookLab > Build WebGL - Task 2 (Search)  -> Build/WebGL-Task2  (task2)
    // (Correctly inside an "Editor" folder — it IS editor-only tooling.)
    public static class BuildWebGL
    {
        const string Task1Scene  = "Assets/Scenes/SampleScene.unity";
        const string Task2Scene  = "Assets/Scenes/task2.unity";
        const string Task1OutDir = "Build/WebGL-Task1";
        const string Task2OutDir = "Build/WebGL-Task2";

        [MenuItem("BookLab/Build WebGL - Task 1 (BookLab)")]
        public static void BuildTask1() => BuildScene(Task1Scene, Task1OutDir);

        [MenuItem("BookLab/Build WebGL - Task 2 (Search)")]
        public static void BuildTask2() => BuildScene(Task2Scene, Task2OutDir);

        static void BuildScene(string scenePath, string outDir)
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.runInBackground = true;

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },   // ONLY this task's scene goes into the build
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildWebGL] building {scenePath} → {outDir} … (the first WebGL build can take several minutes)");
            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;

            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[BuildWebGL] SUCCESS → {outDir}  (~{s.totalSize / (1024 * 1024)} MB, {s.totalTime.TotalSeconds:F0}s). Ready to deploy.");
            else
                Debug.LogError($"[BuildWebGL] {s.result} — see the errors above.");
        }
    }
}
