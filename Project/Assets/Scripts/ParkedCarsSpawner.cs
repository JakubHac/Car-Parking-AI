using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParkedCarsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] Prefabs;
    [SerializeField] private int FreeSpots = 8;

    public IEnumerable<SpawnPoint> GetFreeSpawnPoints => _spawnPoints[..FreeSpots];
    private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();

    public record SpawnPoint
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public SpawnPoint(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    public void ClearSpawnedCars()
    {
        if (_spawnPoints.Length == 0)
        {
            SaveSpawnPoints();
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void SaveSpawnPoints()
    {
        _spawnPoints = new SpawnPoint[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            _spawnPoints[i] = new SpawnPoint(position: child.position, rotation: child.rotation);
        }
        // int[] randomValues = new int[]
        // {
        //     -255, -200, -150, -100, -50, 0, 50, 100, 150, 200, 255, -254, -199, -149, -99, -49, 1, 51, 101, 151, 201, 254, -253, -198, -148, -98, -48, 2, 52, 102, 152, 202, 253, -252, -197, -147, -97, -47, 3, 53, 103, 153, 203, 252, -251, -196, -146, -96, -46, 4, 54, 104, 154, 204, 251, -250, -195, -145, -95, -45, 5, 55, 105, 155, 205, 250
        // };
        // _spawnPoints = _spawnPoints.Select((x, i) => (point: x, random: randomValues[i])).OrderBy(x => x.random).Select(x => x.point).ToArray();
    }

    public void SpawnCars()
    {
        var toSpawn = _spawnPoints.Length - FreeSpots;
        _spawnPoints = _spawnPoints.Select(x => (point: x, random: Random.value)).OrderBy(x => x.random).Select(x => x.point).ToArray();
        for (int i = 0; i < toSpawn; i++)
        {
            var prefab = Prefabs[Random.Range(0, Prefabs.Length)];
            var spawnPoint = _spawnPoints[i + FreeSpots];
            Instantiate(prefab, spawnPoint.Position, spawnPoint.Rotation, transform);
        }
    }
}
