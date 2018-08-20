
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectible : MonoBehaviour {

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

    // protected so that external classes won't just blindly call Collect
    protected virtual void Collect() {
        Destroy(this.gameObject);
    }

    public abstract bool CanCollect();

    public void CollectIfPossible() {
        if (CanCollect()) {
            Collect();
        }
    }
}
