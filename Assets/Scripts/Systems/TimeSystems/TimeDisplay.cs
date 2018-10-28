using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
    public Text timeText;

    private TimeSystem timeSystem;

    private void Start()
    {
        timeSystem = TimeSystem.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        TimeInstant currentTime = timeSystem.GetTime();

        timeText.text = currentTime.GetHours().ToString() + ":00";
    }
}
