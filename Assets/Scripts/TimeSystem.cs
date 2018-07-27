using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSystem : MonoBehaviour {

    private static TimeSystem instance = null;

    private float dawnOfTime = 0f;
    private float currentTime = 0f;

    private readonly float secondsPerDay = 4f;

    // Use this for initialization
    void Awake() {
        if (instance != null) {
            return;
        }
        instance = this;
    }

    void Update() {
        currentTime += Time.deltaTime;
    }

    public float DawnOfTime() {
        return dawnOfTime;
    }

    public float GetTime() {
        return currentTime;
    }

    public int CurrentDay() {
        return (int)Mathf.Floor(currentTime / secondsPerDay);
    }

    public int CurrentHour() {
        float currentDayTime = currentTime % secondsPerDay;
        return (int)Mathf.Floor(24 * currentDayTime / secondsPerDay);
    }

    // Probably move these to a time utility class
    public int TimeToDays(float time) {
        return (int)Mathf.Floor(currentTime / secondsPerDay);
    }

    // Lol yeah definitely move this to a utility class that returns "Instance" objects that hold days and hours, etc.
    public int HoursPortionOfTime(float time) {
        float currentDayTime = time % secondsPerDay;
        return (int)Mathf.Floor(24 * currentDayTime / secondsPerDay);
    }

    // now if we forget to put a GameController in the scene, we can still
    // call this and one will be dynamically created
    public static TimeSystem GetInstance() {
        if (instance == null) {
            instance = new GameObject().AddComponent<TimeSystem>();
            instance.name = "TimeSystem";
        }
        return instance;
    }
}
