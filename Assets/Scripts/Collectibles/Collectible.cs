
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour {

    public ParticleSystem collectedEffect;

    private void OnDestroy() {
        if (collectedEffect) {
            // DEBUG, this might need to be a GameObject prefab
            ParticleSystem ps = Instantiate(collectedEffect, transform);
            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            ps.Play();

            // one-off for the duration of the effect
            Destroy(ps.gameObject, ps.main.duration);
        }
    }

    // Classes that inherit from this should override for custom collection behavior
    public virtual void Collect() {
        Destroy(this.gameObject);
    }
}
