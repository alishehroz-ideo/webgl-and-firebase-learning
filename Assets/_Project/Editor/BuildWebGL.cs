using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BookLab.EditorTools
{
    // One-click WebGL build into Build/WebGL, ready to deploy to Firebase Hosting.
    // Menu:  BookLab > Build WebGL
    // (This is correctly inside an "Editor" folder — it IS editor-only tooling.)
    public static class BuildWebGL
    {
        const string OutDir = "Build/WebGL";

        [MenuItem("BookLab/Build WebGL")]
        public static void Build()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.runInBackground = true;

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) scenes = new[] { "Assets/Scenes/SampleScene.unity" };

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log("[BuildWebGL] building… (the first WebGL build can take several minutes)");
            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;

            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[BuildWebGL] SUCCESS → {OutDir}  (~{s.totalSize / (1024 * 1024)} MB, {s.totalTime.TotalSeconds:F0}s). Ready to deploy.");
            else
                Debug.LogError($"[BuildWebGL] {s.result} — see the errors above.");
        }
    }
}
