using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeInstant {
    private float seconds;

    private static readonly float secondsPerDay = 8f;
    private static readonly float hoursPerDay = 10f;

    public static TimeInstant operator -(TimeInstant a, TimeInstant b) {
        return new TimeInstant(a.seconds - b.seconds);
    }

    public override string ToString() {
        return this.GetDays().ToString() + " " + this.GetHours().ToString() + " (" + this.seconds + ")";
    }

    public static TimeInstant DAY = new TimeInstant(secondsPerDay);
    public static TimeInstant HOUR = new TimeInstant(secondsPerDay / hoursPerDay);

    public TimeInstant() {
        this.seconds = 0f;
    }

    public TimeInstant(float millis) {
        this.seconds = millis;
    }

    public TimeInstant(TimeInstant other) {
        this.seconds = other.seconds;
    }

    public int GetDays() {
        return (int)Mathf.Floor(seconds / secondsPerDay);
    }

    public int GetHours() {
        float currentDayTime = seconds % secondsPerDay;
        return (int)Mathf.Floor(hoursPerDay * currentDayTime / secondsPerDay);
    }

    public float GetRawTime() {
        return seconds;
    }

    public void IncrementSeconds(float duration) {
        this.seconds += duration;
    }

}
