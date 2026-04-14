// BeetleIdentity.cs
// Place: Assets/Scripts/Beetle/BeetleIdentity.cs
// Generates 12-digit hex ID and calls Gemini Flash 2.5 for a name

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BeetleIdentity : MonoBehaviour
{
    // ── Gemini Config ─────────────────────────────────────────────────
    // Replace with your Gemini API key
    private const string GEMINI_KEY   = "YOUR_GEMINI_API_KEY_HERE";
    private const string GEMINI_MODEL = "gemini-2.5-flash";
    private const string GEMINI_URL   =
        "https://generativelanguage.googleapis.com/v1beta/models/"
        + GEMINI_MODEL + ":generateContent?key=" + GEMINI_KEY;
    // ─────────────────────────────────────────────────────────────────

    public string HexId   { get; private set; }
    public string BeetleName { get; private set; } = "...";
    public BeetleGender Gender { get; private set; }
    public int Generation { get; private set; }
    public string ParentId { get; private set; }

    // ── Initialise identity ───────────────────────────────────────────

    public void Initialise(BeetleGender gender, int generation = 1, string parentId = "")
    {
        HexId      = GenerateHexId();
        Gender     = gender;
        Generation = generation;
        ParentId   = parentId;
        StartCoroutine(FetchGeminiName());
    }

    // ── 12-digit hex ID ───────────────────────────────────────────────

    private string GenerateHexId()
    {
        byte[] bytes = new byte[6]; // 6 bytes = 12 hex chars
        new System.Random(Guid.NewGuid().GetHashCode()).NextBytes(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
    }

    // ── Gemini name fetch ─────────────────────────────────────────────

    private IEnumerator FetchGeminiName()
    {
        string prompt = $"Generate a single short fantasy/nature-inspired name for a " +
                        $"{Gender} beetle creature. Just the name, nothing else. " +
                        $"Examples: Kron, Velira, Dusk, Braxis, Lyren. Max 8 characters.";

        string bodyJson = "{\"contents\":[{\"parts\":[{\"text\":\"" +
                          EscapeJson(prompt) + "\"}]}]}";

        using var req = new UnityWebRequest(GEMINI_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string raw = req.downloadHandler.text;
            // Parse: "text": "NameHere"
            int idx = raw.IndexOf("\"text\":");
            if (idx >= 0)
            {
                int start = raw.IndexOf('"', idx + 7) + 1;
                int end   = raw.IndexOf('"', start);
                BeetleName = raw.Substring(start, end - start).Trim();
            }
        }
        else
        {
            // Fallback name from hex
            BeetleName = "Beetle_" + HexId.Substring(0, 4);
        }

        Debug.Log($"[BeetleIdentity] ID={HexId}  Name={BeetleName}  Gen={Generation}");

        // Save to Firebase
        SaveToFirebase();
    }

    private void SaveToFirebase()
    {
        if (FirebaseManager.Instance == null) return;

        string json = $"{{" +
            $"\"hexId\":\"{HexId}\"," +
            $"\"name\":\"{BeetleName}\"," +
            $"\"gender\":\"{Gender}\"," +
            $"\"generation\":{Generation}," +
            $"\"parentId\":\"{ParentId}\"" +
            $"}}";

        FirebaseManager.Instance.Patch($"beetles/{HexId}/identity", json);
    }

    private string EscapeJson(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
}
