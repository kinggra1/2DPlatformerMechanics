using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitions : MonoBehaviour {
    public enum TransitionType { PORTAL };
    private TransitionType currentTransitionType = TransitionType.PORTAL;

    private readonly float PORTAL_FADEOUT_TIME = 2f;
    private readonly float PORTAL_FADEIN_TIME = 1f;

    public Image portalFadeImage;

    private void Start() {
        FindNeededObjects();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void FindNeededObjects() {
        // uiData = GameObject.FindObjectOfType<CanvasElementsNeeded>();
        // portalFadeImage = uiData.portalFadeImage;
    }

    public void PortalToScene(string sceneName) {
        currentTransitionType = TransitionType.PORTAL;
        GameController.GetInstance().PauseGame();
        // Hard set overlap to be transparent before tweening.
        Color current = portalFadeImage.color;
        portalFadeImage.color = new Color(current.r, current.g, current.b, 0f);
        LeanTween.alpha(portalFadeImage.rectTransform, 1f, PORTAL_FADEOUT_TIME).setEase(LeanTweenType.linear).setOnComplete(() => SceneManager.LoadScene(sceneName));
    }

    public void PortalUncover() {
        // Hard set overlap to be opaque before tweening.
        Color current = portalFadeImage.color;
        portalFadeImage.color = new Color(current.r, current.g, current.b, 1f);
        LeanTween.alpha(portalFadeImage.rectTransform, 0f, PORTAL_FADEIN_TIME).setEase(LeanTweenType.linear).setOnComplete(GameController.GetInstance().ResumeGame);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        FindNeededObjects();

        switch (currentTransitionType) {
            case TransitionType.PORTAL:
                PortalUncover();
                break;
        }
    }
}
