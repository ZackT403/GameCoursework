using UnityEngine;
using System.Collections.Generic;

public class AttractionManager : MonoBehaviour
{
    public static AttractionManager Instance;
    
    // list of all active attractions
    public List<Attraction> activeAttractions = new List<Attraction>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterAttraction(Attraction a)
    {
        // add attractions to list
        if (!activeAttractions.Contains(a))
        {
            activeAttractions.Add(a);
        }
    }

    public void UnregisterAttraction(Attraction a)
    {
        // remove attractions from list
        activeAttractions.Remove(a);
    }

    // helper function to count number of attractions of a type
    public int CountAttractionsOfType(AttractionType type)
    {
        int count = 0;
        foreach(Attraction a in activeAttractions)
        {
            if (a.type == type){
                count++;
            }
        }
        return count;
    }

    // Weighted Randomness to pick a destination
    public Attraction GetWeightedTarget()
    {   
        // if no attractions exist then return null
        if (activeAttractions.Count == 0){
            return null;
        }
        // calculate total appeal
        int totalAppeal = 0;
        foreach (Attraction a in activeAttractions){
            totalAppeal += a.appealScore;
        }
        // pick random point
        int randomPoint = Random.Range(0, totalAppeal);
        int currentSum = 0;
        // find which attraction correspons to the point
        foreach (Attraction a in activeAttractions)
        {
            currentSum += a.appealScore;
            if (randomPoint <= currentSum){
                return a;
            }
        }
        return activeAttractions[0];
    }
}