using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    public bool timerIsRunning = false;
    public TextMeshProUGUI timeText;
    public UnityEvent onTimerEnd;

    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
    }

    public void startTimer()
    {
        timerIsRunning = true;
    }

    public void stopTimer()
    {
        timerIsRunning = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                onTimerEnd.Invoke();
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
         
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        if(timeText != null)
        timeText.text = string.Format("{0:00}", seconds);
    }
}