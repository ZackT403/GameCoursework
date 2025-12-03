using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AttendeeAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Attraction currentTarget;
    private Attraction lastVisitedTarget;
    private SpriteRenderer mySprite;
    [Header("Attendee Stats")]
    public float buyingDesire = 1.0f;
    public float personalSatisfaction = 70f; 
    
    // traffic metrics
    public bool isCurrentlyStuck = false;
    public float timeSpentWalking = 0f;
    public float timeSpentEnjoying = 0f;

    // stuck logic
    private float stuckTimer = 0f;
    private bool hasBeenStuck = false; 
    private float stuckThreshold = 2.0f;
    private float repathCooldown = 0f;

    // hunger logic
    private bool isFull = false;
    private float hungerDuration = 60.0f; 
    private float lastAteTime = 0f;

    // visit limits & memory
    private int attractionsVisited = 0;
    private int maxAttractionsToVisit = 5; 
    private bool isLeaving = false; 
    private float distanceTraveled = 0f;
    private float maxDistanceWalked = 0f;      
    private Vector3 lastPosition;

    [Header("Settings")]
    public GameObject litterPrefab;
    [Range(0f, 1f)] public float litterChance = 0.05f; 
    public float minLitterTime = 5.0f; 
    public float maxLitterTime = 15.0f; 
    
    [Header("Bin Logic")]
    public float binSearchRadius = 15.0f; 
    [Range(0f, 1f)] public float binEffectiveness = 0.9f; 
    
    [Header("Review Settings")]
    [Range(0f, 1f)] public float reviewChance = 0.2f;
    public float clumpingRadius = 10.0f;

    // reviews that the attendee may leave
    private string[] reviewsNoToilets = { "I had to hold it all day! BUILD A TOILET!", "Zero bathrooms? Are you serious?" };
    private string[] reviewsLitter = { "This place is a dump!", "Disgusting. Trash everywhere." };
    private string[] reviewsDistance = { "My feet are killing me!", "Too much walking." };
    private string[] reviewsCrowded = { "Way too crowded!", "I got stuck in a crowd bottleneck." };
    private string[] reviewsStuck = { "Impossible to get around! Buildings too cramped!", "Claustrophobic! I couldn't even walk to the stage." };
    private string[] reviewsGood = { "Great music!", "Had an amazing time!", "Loved it." };
    private string[] reviewsMeh = { "It was okay.", "Nothing special." };

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mySprite = GetComponent<SpriteRenderer>();
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        
        // random priority to help with avoidance
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 70); 

        lastPosition = transform.position;
        // keeps personal satisfaction
        if (GameManager.Instance != null){
            personalSatisfaction = Random.Range(GameManager.Instance.globalAverageSatisfaction - 10f, GameManager.Instance.globalAverageSatisfaction );
        }
        else{
            personalSatisfaction = 70f;
        }  
        personalSatisfaction = Mathf.Clamp(personalSatisfaction, 10f, 100f);
        maxAttractionsToVisit = Random.Range(3, 8);

        Invoke("FindNewActivity", 0.1f);
        StartCoroutine(LitterLoop());
    }

    void Update()
    {
        // track metrics to inform reviews
        if (agent.velocity.magnitude > 0.1f)
        {
            float step = Vector3.Distance(transform.position, lastPosition);
            distanceTraveled += step;
            if (distanceTraveled > maxDistanceWalked){
                maxDistanceWalked = distanceTraveled;
            } 
            
            timeSpentWalking += Time.deltaTime;
        }
        else if (agent.remainingDistance < 0.5f && !isLeaving)
        {
            timeSpentEnjoying += Time.deltaTime;
        }
        lastPosition = transform.position;

        // check if the agent is stuck
        CheckIfStuck();
    

        // hunger logic
        if (isFull && Time.time > lastAteTime + hungerDuration){
            isFull = false;
        } 

        // check arrival at destination
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            //  if leaving festival and reached exit point despawn
            if (isLeaving){
                Destroy(gameObject); 
            }
            else if (currentTarget != null && lastVisitedTarget != currentTarget){
                StartCoroutine(VisitAttraction());
            }
        }
        // repath cooldown timer
        if (repathCooldown > 0){
            repathCooldown -= Time.deltaTime;
        }
    }

    void CheckIfStuck()
    {
        // check if an agent is currently stuck
        isCurrentlyStuck = false; 

        if (!agent.isStopped && agent.remainingDistance > 1.5f)
        {
            if (agent.velocity.magnitude < 0.3f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckThreshold)
                {
                    isCurrentlyStuck = true; 
                    HandleStuckLogic();
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
    }

    void HandleStuckLogic()
    {
        // if agents get stuck reduce satisfaction since the layout is bad
        if (!hasBeenStuck)
        {
            personalSatisfaction -= 5f; 
            hasBeenStuck = true;
        }

        // find a new path
        if (repathCooldown <= 0 && currentTarget != null)
        {
            Vector3 originalDest = currentTarget.GetRandomStandingSpot();
            Vector3 detour = originalDest + (Random.insideUnitSphere * 3.0f); 
            detour.z = 0; 
            
            agent.SetDestination(detour);
            repathCooldown = 5.0f; 
        }
    }

    void FindNewActivity()
    {
        // if not having fun then leave
        if (personalSatisfaction < 20f) {
            LeaveFestival();
            return; 
        }
        
        // leave once max attractions visited
        attractionsVisited++;
        if (attractionsVisited >= maxAttractionsToVisit) {
            LeaveFestival();
            return; 
        }

        distanceTraveled = 0f; 
        
        stuckTimer = 0f;
         // pick attraction to visit
        if (AttractionManager.Instance != null)
        {
            // find the weighted target
            Attraction potential = AttractionManager.Instance.GetWeightedTarget();
            if (potential == lastVisitedTarget && AttractionManager.Instance.activeAttractions.Count > 1){
                potential = AttractionManager.Instance.GetWeightedTarget();
            }
            // navigate to it
            currentTarget = potential;
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.GetRandomStandingSpot());
                agent.isStopped = false;
            }
        }
    }

    void LeaveFestival()
    {
        // if the agent didnt have a good time force a review so player knows how to impove
        if (personalSatisfaction < 30) 
        {
            float oldChance = reviewChance;
            reviewChance = 1.0f;
            GenerateReview();
            reviewChance = oldChance;
        }
        else
        {
            // leave general review 
            GenerateReview();
        }

        isLeaving = true;
        currentTarget = null;
        // return to spawn point and despawn
        if (CrowdManager.Instance != null && CrowdManager.Instance.spawnPoint != null) {
            agent.SetDestination(CrowdManager.Instance.spawnPoint.position);
            agent.isStopped = false;
        } else {
            Destroy(gameObject);
        }
    }

    IEnumerator VisitAttraction()
    {
        lastVisitedTarget = currentTarget;
        agent.isStopped = true;
        // add all food types here
        bool isFoodStall = (currentTarget.type == AttractionType.Popcorn || currentTarget.type == AttractionType.CandyFloss || currentTarget.type == AttractionType.BBQStand);
        // calculate if they buy the product
        float chance = currentTarget.profitChance * buyingDesire;
        // if the agent is full they wont buy fod
        if (isFoodStall && isFull){
            chance = 0f;
        }
        // buy food and set fullness
        if (Random.value < chance)
        {
            if (MoneyManager.Instance != null) 
            {
                int finalPrice = CalculateDynamicPrice(currentTarget.productPrice);
                MoneyManager.Instance.AddMoney(finalPrice);
            }
            buyingDesire *= 0.6f; 
            if (isFoodStall) {
                isFull = true;
                lastAteTime = Time.time; 
            }
        }
        // do a wiggle dance thing to show enjoyment
        float timer = 0;
        while (timer < 4.0f)
        {
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(timer * 15) * 15);
            yield return null;
        }
        // calculate and add to satisfaction
        int finalScore = CalculateSatisfaction();
        personalSatisfaction += finalScore;
        // ensure it doesnt exceed bounds
        personalSatisfaction = Mathf.Clamp(personalSatisfaction, 0, 100);
        // generate review if leaving soon
        GenerateReview();
        // reset state
        transform.rotation = Quaternion.identity;
        currentTarget = null;
        FindNewActivity();
    }

    void GenerateReview()
    {   // decide if leaving a review
        if (Random.value > reviewChance){
            return;
        }

        string finalText = "";
        int stars = 3;
        // leave different reviews based on satisfaction
        if (personalSatisfaction >= 75) {
            finalText = reviewsGood[Random.Range(0, reviewsGood.Length)];
            stars = 5;
        }
        else if (personalSatisfaction >= 30) {
            finalText = reviewsMeh[Random.Range(0, reviewsMeh.Length)];
            stars = 3;
        }
        else {
            // if its a bad review check for specific complaints
            // this lets the user know what to improve
            List<string> complaints = new List<string>();
            // if the agent got stuck leave that complaint
            if (hasBeenStuck){
                complaints.Add(reviewsStuck[Random.Range(0, reviewsStuck.Length)]);
            }
            // if there where no toilets
            int toilets = AttractionManager.Instance.CountAttractionsOfType(AttractionType.Toilet);
            if (toilets == 0){
                complaints.Add(reviewsNoToilets[0]);
            }
            // if there was rubbish around
            Collider2D[] trash = Physics2D.OverlapCircleAll(transform.position, 8f);
            foreach(var hit in trash) {
                // figure out if any litter neatby
                 if (hit.CompareTag("Litter")) {
                    complaints.Add(reviewsLitter[0]);
                    break; 
                } 
            }
            // if they walked alot complain about that
            if (maxDistanceWalked > 60){
                complaints.Add(reviewsDistance[0]);
            }
            // if there was a crowd clumping
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, clumpingRadius);
            int cloneCount = 0;
            if (currentTarget != null) {
                foreach (var hit in neighbors) {
                    Attraction n = hit.GetComponentInParent<Attraction>();
                    if (n != null && n != currentTarget && n.type == currentTarget.type){
                        cloneCount++;
                    }
                }
            }
            if (cloneCount > 2){
                complaints.Add(reviewsCrowded[0]);
            }
            // pick a random complaint to leave
            if (complaints.Count > 0) {
                finalText = complaints[Random.Range(0, complaints.Count)];
                stars = 1;
            } else {
                finalText = "Boring festival.";
                stars = 2;
            }
        }
        // add review to manager
        if (ReviewManager.Instance != null){
            ReviewManager.Instance.AddReview(finalText, stars);
        }
    }

    int CalculateDynamicPrice(int basePrice)
    {
        // calculate price based on satisfaction 
        // higher satisfaction = pay more
        float happinessFactor = personalSatisfaction / 100f;
        float bonus = basePrice * happinessFactor;
        return Mathf.RoundToInt(basePrice + bonus);
    }

    int CalculateSatisfaction()
    {
        // calculate satisfaction score
        int score = currentTarget.baseSatisfaction;
        // if they had to walk alot reduce score
        int walkPenalty = Mathf.FloorToInt(distanceTraveled / 15.0f);
        score -= walkPenalty;
        // if they are visiting the same type of attraction reduce score
        if (currentTarget.type != AttractionType.Bin && currentTarget.type != AttractionType.Toilet)
        {
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, clumpingRadius);
            int cloneCount = 0;
            int varietyCount = 0;
            foreach (var hit in neighbors)
            {
                Attraction n = hit.GetComponentInParent<Attraction>();
                if (n != null && n != currentTarget && n.type != AttractionType.Bin && n.type != AttractionType.Toilet)
                {
                    if (n.type == currentTarget.type){
                        cloneCount++;
                    }
                    else{
                        varietyCount++;
                    }
                }
            }
            // reduce score 
            // increase for varity looks more fun
            score -= (cloneCount * 2);
            score += (varietyCount * 1);
        }
        return Mathf.Clamp(score, -10, 30);
    }

    IEnumerator LitterLoop()
    {
        // determine if the attendee litters
        while (true)
        {   
            // wait a random amount of time between littering
            float waitTime = Random.Range(minLitterTime, maxLitterTime);
            yield return new WaitForSeconds(waitTime);
            // check for nearby bins if there is not one they dont litter
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, binSearchRadius);
            int nearbyBins = 0;
            // count nearby bins
            foreach (var hit in hits) {
                if (hit.CompareTag("Bin")){
                    nearbyBins++;
                }
            }
            // calcualte litter chance
            float safetyFactor = nearbyBins * binEffectiveness; 
            float currentChance = litterChance * Mathf.Clamp01(1.0f - safetyFactor);
            // attempt to litter
            if (!isLeaving && currentChance > 0 && Random.value < currentChance && litterPrefab != null)
            {
                Instantiate(litterPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}