using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BookLab.Core.Net
{
    // The ONE place that actually talks to the internet (same role as your GameBullClient).
    // Three jobs:  read JSON (GetJson), write JSON (PutJson), download an image (GetTexture).
    // Everything else in the app goes through these — nobody else touches UnityWebRequest.
    public static class FirebaseClient
    {
        // GET: read JSON text from a Firebase path.
        public static async Task<string> GetJson(string url)
        {
            using (var req = UnityWebRequest.Get(url))
            {
                await SendAsync(req);
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Firebase] GET failed {url} → {req.responseCode} {req.error}");
                    return null;
                }
                return req.downloadHandler.text;
            }
        }

        // PUT: write JSON to a Firebase path (creates it, or overwrites what's there).
        public static async Task<bool> PutJson(string url, string json)
        {
            using (var req = new UnityWebRequest(url, "PUT"))
            {
                req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                await SendAsync(req);
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Firebase] PUT failed {url} → {req.responseCode} {req.error}");
                    return false;
                }
                return true;
            }
        }

        // GET raw bytes — used for images. AssetService downloads through this, saves the
        // bytes to disk (cache), then decodes them into a Sprite.
        public static async Task<byte[]> GetBytes(string url)
        {
            using (var req = UnityWebRequest.Get(url))
            {
                await SendAsync(req);
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Firebase] BYTES failed {url} → {req.responseCode} {req.error}");
                    return null;
                }
                return req.downloadHandler.data;
            }
        }

        // Turns Unity's "send and wait" into something we can 'await'
        // (works fine in single-threaded WebGL — same trick your GameBullClient uses).
        static Task SendAsync(UnityWebRequest req)
        {
            var tcs = new TaskCompletionSource<bool>();
            req.SendWebRequest().completed += _ => tcs.SetResult(true);
            return tcs.Task;
        }
    }
}
