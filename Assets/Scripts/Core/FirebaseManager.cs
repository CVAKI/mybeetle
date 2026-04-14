// FirebaseManager.cs
// Place: Assets/Scripts/Core/FirebaseManager.cs
// Handles all Firebase Realtime Database REST calls — NO SDK required

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    // ── Firebase Config ──────────────────────────────────────────────
    private const string DB_URL  = "https://mybettle-default-rtdb.asia-southeast1.firebasedatabase.app";
    private const string API_KEY = "AIzaSyDQ6BJkbRM--oxopV0JyHzS6w_YBu6Ts80";
    // ────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // ── Public REST Methods ──────────────────────────────────────────

    /// PUT — write/overwrite a node
    public void Put(string path, string json, Action<bool> onDone = null)
        => StartCoroutine(Request("PUT", path, json, onDone));

    /// PATCH — update specific fields without overwriting whole node
    public void Patch(string path, string json, Action<bool> onDone = null)
        => StartCoroutine(Request("PATCH", path, json, onDone));

    /// GET — read a node
    public void Get(string path, Action<string> onData)
        => StartCoroutine(GetRequest(path, onData));

    /// DELETE — remove a node
    public void Delete(string path, Action<bool> onDone = null)
        => StartCoroutine(Request("DELETE", path, null, onDone));

    // ── Internal Coroutines ──────────────────────────────────────────

    private IEnumerator Request(string method, string path, string json, Action<bool> onDone)
    {
        string url = $"{DB_URL}/{path}.json?auth={API_KEY}";
        byte[] body = json != null ? Encoding.UTF8.GetBytes(json) : null;

        using var req = new UnityWebRequest(url, method);
        if (body != null) req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        bool ok = req.result == UnityWebRequest.Result.Success;
        if (!ok) Debug.LogWarning($"[Firebase] {method} {path} failed: {req.error}");
        onDone?.Invoke(ok);
    }

    private IEnumerator GetRequest(string path, Action<string> onData)
    {
        string url = $"{DB_URL}/{path}.json?auth={API_KEY}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onData?.Invoke(req.downloadHandler.text);
        else
        {
            Debug.LogWarning($"[Firebase] GET {path} failed: {req.error}");
            onData?.Invoke(null);
        }
    }
}
