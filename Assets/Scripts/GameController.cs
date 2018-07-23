using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static GameController instance = null;

	// Use this for initialization
	void Awake () {
        if (instance != null) {
            return;
        }
        instance = this;
	}

    // now if we forget to put a GameController in the scene, we can still
    // call this and one will be dynamically created
    public static GameController getInstance() {
        if (instance == null) {
            instance = Instantiate(new GameObject()).AddComponent<GameController>();
        }
        return instance;
    }
}
