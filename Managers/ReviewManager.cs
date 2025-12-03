using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ReviewManager : MonoBehaviour
{
    public static ReviewManager Instance;

    [Header("UI References")]
    public GameObject reviewPanel; 
    public TextMeshProUGUI reviewLogText; 
    // review storage
    private List<string> allReviews = new List<string>();
    // buffer to prevent reviews getting clogged up
    private Queue<string> recentMessages = new Queue<string>();
    private int antiSpamBufferSize = 5; 

    private string[] names = { 
        "Steve", "Sarah", "Mike", "Emma", "Chris", "Alex", "Sam", "Jo", 
        "FestivalFan99", "MusicLover", "PartyAnimal", "Guest_101" 
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ensure review pannel is closed at the start
        if (reviewPanel != null){
            reviewPanel.SetActive(false);
        } 
    }

    public void ToggleReviewPanel()
    {
        // show or hide review panel if button in ui is pressed
        bool isActive = reviewPanel.activeSelf;
        reviewPanel.SetActive(!isActive);
    }

    public void ClosePanel()
    {
        // allow user to close the review pannel from close button
        if (reviewPanel != null){
            reviewPanel.SetActive(false);
        }
    }

    public void AddReview(string text, int starRating)
    {
        // check if the message is in the previous 5 messages 
        // dont want loads of repreated messagges
        if (recentMessages.Contains(text))
        {
            return;
        }

        // add review to the buffer
        recentMessages.Enqueue(text);
        // maintian buffer size
        if (recentMessages.Count > antiSpamBufferSize)
        {
            recentMessages.Dequeue();
        }
        // generate stars for the review 
        string stars = "";
        for(int i=0; i<5; i++){
            stars += (i < starRating) ? "<color=yellow>★</color>" : "<color=grey>☆</color>";
        }
        
        // grab a random name from names list 
        string rName = names[Random.Range(0, names.Length)];

        // format the review
        string finalReview = $"<b>{rName}</b> {stars}\n<i>\"{text}\"</i>\n----------------\n";
        
        // add to reviews
        allReviews.Insert(0, finalReview);
        
        //cap log to 30 reviews loads of them slows game down
        if (allReviews.Count > 30)
        {
            allReviews.RemoveAt(allReviews.Count - 1);
        }
        
        UpdateReviewUI(); 
    }

    public void ClearReviews()
    {
        // clear all reviews from the log
        allReviews.Clear();
        recentMessages.Clear();
        UpdateReviewUI();
    }

    void UpdateReviewUI()
    {
        // update the review log in the pannel
        if (reviewLogText == null){
            return;
        }

        string fullLog = "";
        foreach (string r in allReviews)
        {
            fullLog += r;
        }

        reviewLogText.text = fullLog;
    }
}