using UnityEngine;
using System.Collections;

public class CrowdManager : MonoBehaviour
{
    public static CrowdManager Instance;

    [Header("References")]
    public GameObject attendeePrefab;
    public Transform spawnPoint;

    [Header("Settings")]
    public int baseVisitors = 10;
    public float spawnDelay = 0.5f;
    public int entranceFee = 10;

    void Awake()
    {
        Instance = this;
    }

    public void StartSpawning()
    {
        // spawn crowd at start of day
        if (attendeePrefab != null && spawnPoint != null)
        {
            StartCoroutine(SpawnCrowdRoutine());
        }
    }

    public void StopSpawningAndClear()
    {
        // stop spawning and despawn all attendees
        StopAllCoroutines();
        GameObject[] attendees = GameObject.FindGameObjectsWithTag("Attendee");
        foreach (GameObject person in attendees){
            Destroy(person);
        }
    }

    IEnumerator SpawnCrowdRoutine()
    {
        // calculate number of attendees to spawn based on the number of attractions
        int attractionCount = 0;
        // get attraction count
        if (AttractionManager.Instance != null)
        {
            attractionCount = AttractionManager.Instance.activeAttractions.Count;
        }
        // calculate total attendees to spawn
        int totalToSpawn = baseVisitors + (attractionCount * 5);
        
        for (int i = 0; i < totalToSpawn; i++)
        {
            // create attendee at spawn point
            Instantiate(attendeePrefab, spawnPoint.position, Quaternion.identity);
            // add entrance fee to money manager            
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(entranceFee);
            }
            // wait spawn delay time before next spawn
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}