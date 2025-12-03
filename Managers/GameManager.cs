using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int maxDays = 5;
    public bool isLive = false; 
    public int currentDay = 1;
    
    [Header("Timer")]
    public float dayDuration = 60.0f;
    private float currentTimer = 0f;

    [Header("Simulation Stats")]
    public int globalAverageSatisfaction = 50; 
    private float nextUpdateTimer = 0;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // intialize the ui
        if (UIManager.Instance != null)
        {
            //UIManager.Instance.UpdateSatisfactionUI(globalAverageSatisfaction);
            UIManager.Instance.UpdateDayUI(currentDay);
            UIManager.Instance.SetStartButtonState(true, "START DAY " + currentDay);
            UIManager.Instance.UpdateTimerUI(dayDuration);
        }
    }

    void Update()
    {
        if (isLive)
        {
            // update the day timer
            currentTimer -= Time.deltaTime;
            if (UIManager.Instance != null) {
                UIManager.Instance.UpdateTimerUI(currentTimer);
            }

            // end event if timer runs out
            if (currentTimer <= 0)
            {
                EndDay(); 
            }

            if (Time.time > nextUpdateTimer && currentTimer < dayDuration - 1f)
            {
                CalculateGlobalHappiness();
                CheckToiletCrisis();
                CheckLitterPenalty();
                nextUpdateTimer = Time.time + 0.5f; 
            }
        }
    }

    public void StartDay()
    {
        if (isLive){
            return;
        }
        if (globalAverageSatisfaction < 30)
        {
            globalAverageSatisfaction = 30;
            
            // Force UI update immediately so you see it change
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateSatisfactionUI(globalAverageSatisfaction);
        }
        // check if attractions have been built when the day gets started
        if (AttractionManager.Instance.activeAttractions.Count == 0)
        {
            if (UIManager.Instance != null){
                UIManager.Instance.ShowError("Build something first!");
            }
            return; 
        }

        // start the day
        isLive = true;
        currentTimer = dayDuration; 
        // spawn in the crowd
        if (CrowdManager.Instance != null){
            CrowdManager.Instance.StartSpawning();
        }

        // prevent the start button from being pressed again
        // stops them just collecting entry fee repeatedly
        if (UIManager.Instance != null) 
            UIManager.Instance.SetStartButtonState(false, "FESTIVAL LIVE...");
    }

    void EndDay()
    {
        // end day
        isLive = false;
        currentTimer = 0;


        // stop spawning crowd and clear all attendes
        if (CrowdManager.Instance != null) 
        {
            CrowdManager.Instance.StopSpawningAndClear();
        }


        // try break toilets for mini game
        if (AttractionManager.Instance != null)
        {
            foreach (Attraction a in AttractionManager.Instance.activeAttractions){
                a.TryClogToilet();
            }
        }

        // add to day count
        currentDay++;
        // unlock button so next day can be started
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDayUI(currentDay);
            UIManager.Instance.SetStartButtonState(true, "START DAY " + currentDay);
            UIManager.Instance.UpdateTimerUI(dayDuration);
        }
    }

 

    void CalculateGlobalHappiness() {
        GameObject[] people = GameObject.FindGameObjectsWithTag("Attendee");
        if (people.Length == 0){
            return;
        }
        float totalScore = 0;
        int count = 0;
        // get average satisfaction accross all attendees
        foreach (GameObject person in people) {
            AttendeeAI ai = person.GetComponent<AttendeeAI>();
            if (ai != null) {
                totalScore += ai.personalSatisfaction;
                count++;
            }
        }
        if (count > 0){
            globalAverageSatisfaction = Mathf.RoundToInt(totalScore / count);
        }
        // show satsifaction in ui
        if (UIManager.Instance != null){
            UIManager.Instance.UpdateSatisfactionUI(globalAverageSatisfaction);
        }
    }
    void ApplyGlobalPenalty(float amount) {
        GameObject[] people = GameObject.FindGameObjectsWithTag("Attendee");
        // reduce satisfaction of all attendees by amount
        foreach (GameObject person in people) {
            AttendeeAI ai = person.GetComponent<AttendeeAI>();
            if (ai != null) {
                ai.personalSatisfaction -= amount;
                if (ai.personalSatisfaction < 0){
                    ai.personalSatisfaction = 0;
                }
            }
        }
    }
    void CheckToiletCrisis() {
        // if no toilets exist apply penalty
        if (AttractionManager.Instance != null) {
            int toilets = AttractionManager.Instance.CountAttractionsOfType(AttractionType.Toilet);
            if (toilets == 0){
                ApplyGlobalPenalty(2f);
                return;
            } 
        }
    }
    void CheckLitterPenalty() {
        // if too much litter exists apply penalty
        GameObject[] trash = GameObject.FindGameObjectsWithTag("Litter");
        if (trash.Length > 5){
            ApplyGlobalPenalty(0.5f);
        }
    }
}