using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Transitioner : MonoBehaviour {

    //load levels and stuff
    //TODO: could set the color of the circle according to the level state...
    //...for that to work would need a persistent color?

    public static Color circleColor = new Color(23f/255f, 133f/255f, 225f/255f, 1f);
    public Color circleResetColor = new Color(23f / 255f, 133f / 255f, 225f / 255f, 1f);

    public float transitionIn = 1f;
    public float transitionOut = 1f;
    public Image circle;
    public float circleMaxScale = 35f;
    public float circleMinScale = 0.1f;
    //reposition this circle where the click happened on the canvas
    public GameObject clickBlocker;

    delegate void TransitionDelegate();
    TransitionDelegate afterTransitionAction;

    int loadLevelIndex;

	void Start () {
        clickBlocker.SetActive(true);
        StartTransitionIn();
	}
	
    void Update() {
        //circle.rectTransform.position = Input.mousePosition;
    }



    public void RestartLevel() {
        afterTransitionAction = _RestartLevel;
        StartTransitionOut();
    }

    public void LoadNextLevel() {
        afterTransitionAction = _LoadNextLevel;
        StartTransitionOut();
    }

    public void LoadLevel(int levelIndex) {
        loadLevelIndex = levelIndex;
        afterTransitionAction = _LoadLevel;
        StartTransitionOut();
    }

    public void GoToMainMenu() {
        afterTransitionAction = _GoToMainMenu;
        StartTransitionOut();
    }

    public void Quit() {
        afterTransitionAction = _Quit;
        StartTransitionOut();
    }

    public void _RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void _LoadNextLevel() {
        int index = FindObjectOfType<Level>().levelIndex + 1;
        SceneManager.LoadScene(index);
    }

    public void _LoadLevel() {
        SceneManager.LoadScene(loadLevelIndex);
    }

    public void _GoToMainMenu() {
        SceneManager.LoadScene(0);
    }

    public void _Quit() {
        Application.Quit();
    }

    public void SetCircleColor(Color col) {
        circleColor = col;
    }

    void StartTransitionIn() {
        if (SceneManager.GetActiveScene().buildIndex != 0) {
            //if this isn't the main menu then reposition the fade in circle onto the player
            Vector3 pos = FindObjectOfType<Movement>().transform.position;
            pos = Camera.main.WorldToScreenPoint(pos);
            circle.rectTransform.position = pos;
        }
        circle.color = circleColor;
        clickBlocker.SetActive(true);
        StartCoroutine(TransitionIn());
    }

    void StartTransitionOut() {
        circle.enabled = true;
        circle.color = circleColor;
        circle.rectTransform.position = Input.mousePosition;
        clickBlocker.SetActive(true);
        StartCoroutine(TransitionOut());
    }

    public void TransitionOutFinished() {
        Debug.Log("Done with the fade-out!");
        if (afterTransitionAction != null) {
            afterTransitionAction();
        }
    }

    IEnumerator TransitionIn() {
        float timer = 0;
        float scale = 0;
        Color circleStartColor = circle.color;
        while (timer < transitionIn) {
            timer += Time.deltaTime;
            float t = (timer / transitionIn);
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            scale = Mathf.Lerp(circleMaxScale, circleMinScale, t);
            circle.transform.localScale = Vector3.one * scale;
            circle.color = Color.Lerp(circleStartColor, circleResetColor, t);
            yield return null;
        }

        circle.enabled = false;
        clickBlocker.SetActive(false);
        circleColor = circleResetColor;

        yield break;
    }

    IEnumerator TransitionOut() {
        float timer = 0;
        float scale = 0;
        while (timer < transitionOut) {
            timer += Time.deltaTime;
            float t = (timer / transitionIn);
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            scale = Mathf.Lerp(circleMinScale, circleMaxScale, t);
            circle.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        TransitionOutFinished();

        yield break;
    }
}
