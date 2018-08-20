using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSystem : MonoBehaviour {

    private static TimeSystem instance = null;

    private TimeInstant dawnOfTime = new TimeInstant(0f);
    private TimeInstant currentTime = new TimeInstant(0f);

    // Use this for initialization
    void Awake() {
        if (instance != null) {
            return;
        }
        instance = this;
    }

    void Update() {
        currentTime.IncrementSeconds(Time.deltaTime);
    }

    public TimeInstant DawnOfTime() {
        return new TimeInstant(dawnOfTime);
    }

    public TimeInstant GetTime() {
        // defensive copying or else this is just the same object (C# is all references like Java)
        return new TimeInstant(currentTime);
    }

    public int CurrentDay() {
        return currentTime.GetDays();
    }

    public int CurrentHour() {
        return currentTime.GetHours();
    }

    // now if we forget to put a TimeSystem in the scene, we can still
    // call this and one will be dynamically created
    public static TimeSystem GetInstance() {
        if (instance == null) {
            instance = new GameObject().AddComponent<TimeSystem>();
            instance.name = "TimeSystem";
        }
        return instance;
    }
}
