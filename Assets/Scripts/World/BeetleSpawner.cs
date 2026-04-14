// BeetleSpawner.cs
// Place: Assets/Scripts/World/BeetleSpawner.cs
// Spawns the initial beetle population and handles rebirth

using System.Collections.Generic;
using UnityEngine;

public class BeetleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject maleChildPrefab;
    public GameObject femaleChildPrefab;

    [Header("Spawn Settings")]
    public int    initialMaleCount   = 3;
    public int    initialFemaleCount = 3;
    public float  spawnRadius        = 20f;
    public int    generation         = 1;

    [Header("Tracked Beetle (Camera Beetle)")]
    public BeetleCameraController cameraController;

    private List<BeetleController> _beetles = new();

    void Start()
    {
        SpawnInitialPopulation();
    }

    private void SpawnInitialPopulation()
    {
        // Spawn males
        for (int i = 0; i < initialMaleCount; i++)
        {
            var go = SpawnBeetle(maleChildPrefab, BeetleGender.Male);
            // First male is the tracked (camera) beetle
            if (i == 0)
            {
                go.isTracked = true;
                cameraController?.AttachTo(go.transform);
            }
        }

        // Spawn females
        for (int i = 0; i < initialFemaleCount; i++)
            SpawnBeetle(femaleChildPrefab, BeetleGender.Female);
    }

    private BeetleController SpawnBeetle(GameObject prefab, BeetleGender gender,
                                          string parentId = "", int gen = 1)
    {
        Vector3 pos = transform.position + Random.insideUnitSphere.With(y: 0f) * spawnRadius;
        var go = Instantiate(prefab, pos, Quaternion.identity);

        var ctrl = go.GetComponent<BeetleController>();
        ctrl.spawner = this;

        var id = go.GetComponent<BeetleIdentity>();
        id.Initialise(gender, gen, parentId);

        _beetles.Add(ctrl);
        return ctrl;
    }

    // ── Rebirth (adult dies without offspring, new identity + new DNA) ──

    public void RebornBeetle(BeetleController old)
    {
        var gender = old.identity.Gender;
        int  gen   = old.identity.Generation + 1;
        bool tracked = old.isTracked;

        // Destroy old
        _beetles.Remove(old);
        Destroy(old.gameObject, 3f);

        // Spawn fresh
        var prefab = gender == BeetleGender.Male ? maleChildPrefab : femaleChildPrefab;
        var newCtrl = SpawnBeetle(prefab, gender, "", gen);

        if (tracked)
        {
            newCtrl.isTracked = true;
            cameraController?.AttachTo(newCtrl.transform);
        }
    }

    // ── Hatch egg (camera passes to child) ───────────────────────────

    public BeetleController HatchEgg(BeetleGender childGender, string parentId,
                                      int parentGen, bool attachCamera)
    {
        var prefab = childGender == BeetleGender.Male ? maleChildPrefab : femaleChildPrefab;
        var child  = SpawnBeetle(prefab, childGender, parentId, parentGen + 1);

        if (attachCamera)
        {
            // Remove tracked from parent
            foreach (var b in _beetles)
                if (b.isTracked && b != child) b.isTracked = false;

            child.isTracked = true;
            cameraController?.AttachTo(child.transform);
        }

        return child;
    }

    // ── Utility ───────────────────────────────────────────────────────

    public Vector3 GetRandomSpawnPoint()
    {
        Vector3 p = transform.position + Random.insideUnitSphere.With(y: 0f) * spawnRadius;
        return p;
    }
}

// Helper extension
public static class VectorExtensions
{
    public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        => new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
}
