using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour {

    //TODO: separate out most of this into a UI class

    Transitioner transitioner;
    Level level;

    float maxEnergy;
    float energyLeft;
    public float EnergyLeft { get { return energyLeft; } }

    [Header("Rewinding")]
    public int rewindSteps = 5;
    int rewindsLeft;
    public Text rewindButtonText;

    [Header("Colors")]
    public float colorLerpSpeed = 1.5f;
    Color tar;
    Color cur;
    Color col;
    Color targetColor;

    public Color startEnergyColor;
    public Color noEnergyColor;
    public Color failedConnection;
    public Color connectionMadeColor;
    public Color connectionMadeColor2;
    public float connectionMadePulseTime;
    public Color successColor;
    public Color stuckColor;
    public bool rainbowMode = false;
    int rainbowIndex = 0;
    public Color[] rainbowColors;
    float rainbowTimer = 0;
    public float rainbowSpeed = 1f;

    float timer = -1f;

    [Header("Renderers")]
    public SpriteRenderer startCirleRend;
    public SpriteRenderer plugRend;
    public SpriteRenderer plugBackRend;
    public Material lineMaterial;
    public SpriteRenderer socketPlugRend;
    public Image menuButton;
    public Image rewindButton;

    public Material obstacleMat;

    public ParticleSystem sparkBurstPS;
    public ParticleSystem sparksPS;

    [Header("Line")]
    public LineRenderer line;
    bool createLine = true;
    public Transform player;
    public float linePointDistance = 0.1f;
    Vector3 lastPoint;
    int linePoints = 0;

    [Header("UI")]
    public Slider energyBar;
    public Image energyBarFill;

    public GameObject menuPanel;
    public GameObject successPanel;
    public GameObject failedPanel;
    public float panelDelay = 2f;
    public float panelDelayFail = 0.75f;
    public GameObject clickBlocker;

    public GameObject canvas;


    List<float> energyLeftAtPoint = new List<float>();



    void Start () {
        transitioner = FindObjectOfType<Transitioner>();
        level = FindObjectOfType<Level>();

        canvas.SetActive(true);
        clickBlocker.SetActive(false);
        menuPanel.SetActive(false);
        failedPanel.SetActive(false);
        successPanel.SetActive(false);

        socketPlugRend.enabled = false;

        //set the energy level at the start according to the level object
        energyLeft = FindObjectOfType<Level>().startingEnergy;
        maxEnergy = energyLeft;
        if (energyLeft == 0) {
            Debug.Log("There is no energy, can't play! Abort!");
            //TODO: this should end the level/kick you back to the main menu/give error
        }

        energyBar.maxValue = maxEnergy;
        energyBar.value = energyLeft;

        rewindsLeft = level.rewinds;
        if (rewindsLeft < 100) {
            rewindButtonText.text = "Rewind\n" + rewindsLeft + " left";
        }

        linePoints = 2;
        line.positionCount = linePoints;
        line.SetPosition(0, player.position - (player.forward * 1.0f) + (Vector3.up * (0.1f + (0.001f * linePoints))));
        line.SetPosition(1, player.position - (player.forward * 0.5f) + (Vector3.up * (0.1f + (0.001f * linePoints))));
        energyLeftAtPoint.Add(maxEnergy);
        energyLeftAtPoint.Add(maxEnergy);
        //line.SetPosition(2, player.position + (Vector3.up * (0.1f + (0.001f * linePoints))));

        SetTargetColor(startEnergyColor);
        ChangeColor(targetColor);
	}
	
	void Update () {
        float distance = (lastPoint - player.position).sqrMagnitude;
        if (distance >= (linePointDistance * linePointDistance) && createLine) {
            //Debug.Log("LINE: creating point #" + (linePoints + 1));
            linePoints++;
            line.positionCount = linePoints;
            Vector3 pointPosition = player.position + (Vector3.up * (0.1f + (0.001f * linePoints)));
            line.SetPosition(linePoints - 1, pointPosition);

            lastPoint = player.position;

            energyLeftAtPoint.Add(energyLeft);
            Debug.Log("Energy at point " + linePoints + " was " + energyLeft);
        }

        if (timer >= 0) {
            timer += Time.deltaTime;
            float t = 0.5f * (Mathf.Sin(timer / connectionMadePulseTime) + 1);
            Color _color = Color.Lerp(connectionMadeColor, connectionMadeColor2, t);
            SetTargetColor(_color);
        }

        tar = targetColor;
        cur = lineMaterial.color;
        //Debug.Log(tar.r + " - " + cur.r + " / " + tar.g + " - " + cur.g + " / " + tar.b + " - " + cur.b);
        if (tar.r != cur.r || tar.g != cur.g || tar.b != cur.b) {
            float t = colorLerpSpeed * Time.deltaTime;
            col = new Color(Mathf.MoveTowards(cur.r, tar.r, t), Mathf.MoveTowards(cur.g, tar.g, t), Mathf.MoveTowards(cur.b, tar.b, t));
            ChangeColor(col);
        }

        if (rainbowMode) {
            if (rainbowTimer % rainbowSpeed == 0) {
                rainbowIndex++;
                rainbowIndex = rainbowIndex % rainbowColors.Length;
            }
            int lastIndex = rainbowIndex - 1;
            if (rainbowIndex <= -1) {
                rainbowIndex = rainbowColors.Length - 1;
            }
            Color _col = Color.Lerp(rainbowColors[lastIndex], rainbowColors[rainbowIndex], (rainbowTimer % rainbowSpeed) / rainbowSpeed);
            ChangeColor(_col);
        }
	}

    public void Stuck(bool isStuck) {
        //Debug.Log("Are we stuck? " + isStuck);
        if (isStuck) {
            SetTargetColor(stuckColor);
        } else {
            Color _color = Color.Lerp(startEnergyColor, noEnergyColor, 1 - (energyLeft / maxEnergy));
            SetTargetColor(_color);
        }
    }

    public void SetEnergyLevel(float energyLevel) {
        maxEnergy = energyLevel;
        energyLeft = energyLevel;
    }

    public bool SpendEnergy( float energyToSpend ) {
        if (energyLeft > energyToSpend) {
            energyLeft -= energyToSpend;
            energyBar.value = energyLeft;
            //Debug.Log("Energy left: " + energyLeft);
            Color _color = Color.Lerp(startEnergyColor, noEnergyColor, 1 - (energyLeft / maxEnergy));
            SetTargetColor(_color);
            return true;
        } else {
            energyLeft = 0;
            energyBar.value = 0;
            //energyBar.value = energyBar.maxValue; //TODO: this looks pretty jarring

            SetTargetColor(failedConnection);
            transitioner.SetCircleColor(failedConnection);

            clickBlocker.SetActive(true);
            Invoke("ActivateFailurePanel", panelDelayFail);
            return false;
        }
    }

    public void PlayerHasReachedSocket(Vector3 socketPos) {
        linePoints++;
        line.positionCount = linePoints;
        Vector3 pointPosition = socketPos + (Vector3.up * (0.1f + (0.001f * linePoints)));
        line.SetPosition(linePoints - 1, pointPosition);

        createLine = false;

        plugRend.enabled = false;
        plugBackRend.enabled = false;

        socketPlugRend.enabled = true;

        sparkBurstPS.Play();
        sparksPS.Play();

        FindObjectOfType<Movement>().maxMoveSpeed = 0;

        SetTargetColor(connectionMadeColor);
        timer = 0.001f;

        transitioner.SetCircleColor(successColor);

        //energyBar.value = energyBar.maxValue; //TODO: this looks pretty jarring

        clickBlocker.SetActive(true);
        Invoke("ActivateSuccessPanel", panelDelay);        
    }

    //these should be the same thing with a delegate for what to activate
    void ActivateSuccessPanel() {
        successPanel.SetActive(true);
    }

    void ActivateFailurePanel() {
        failedPanel.SetActive(true);
    }

    public void Rewind() {
        if (rewindsLeft <= 0 || line.positionCount <= rewindSteps + 2) {
            return;
        }
        rewindsLeft--;
        if (rewindsLeft > 100) {
            rewindsLeft++;
        } else {
            rewindButtonText.text = "Rewind\n" + rewindsLeft + " left";
        }
        //go back a few positions, also align the plug with the previous position
        //also delete the points that are no longer needed
        if (line.positionCount > rewindSteps + 2) {
            //TODO: calculate the length of each segment, add them
            float rewindDistance = rewindSteps * linePointDistance;
            //energyLeft += rewindDistance * 0.9f;
            energyLeft = energyLeftAtPoint[line.positionCount - rewindSteps - 1];
            energyBar.value = energyLeft;
            energyLeftAtPoint.RemoveRange(line.positionCount - rewindSteps, rewindSteps);
            Debug.Log("Rewound to point " + energyLeftAtPoint.Count + ", reset energy to " + energyLeft);
            //TODO: make sure all this list stuff works properly(uses correct positions, snips list to correct length)

            Vector3 pos = line.GetPosition(line.positionCount - rewindSteps - 1);
            Vector3 posVector = line.GetPosition(line.positionCount - rewindSteps - 2);
            FindObjectOfType<Movement>().SetPosition(pos, posVector);
            line.positionCount = line.positionCount - rewindSteps;
            lastPoint = pos;
            linePoints = line.positionCount;
        }
    }

    void SetTargetColor(Color col) {
        targetColor = col;
    }

    void ChangeColor(Color col) {
        plugRend.color = col;
        startCirleRend.color = col;
        lineMaterial.color = col;
        socketPlugRend.color = col;
        energyBarFill.color = col;
        menuButton.color = col;
        obstacleMat.color = Color.Lerp(col, Color.black, 0.1f);
        //obstacleMat.color = Color.Lerp(col, Color.black, 0.2f);
        rewindButton.color = col;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.black;
        for (int i = 0; i < line.positionCount; i++) {
            Gizmos.DrawCube(line.GetPosition(i), Vector3.one * 0.1f);
        }
    }
}
