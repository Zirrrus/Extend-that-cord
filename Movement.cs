using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public static Utility.Terrain currentTerrain = Utility.Terrain.normal;
    //normal terrain does not affect velocity
    //slopes add a constant decelleration depending on the steepness
    //oil reduces deceleration, acceleration, input control

    public bool useSmoothAxes = true;

    bool canMove = true;

    //we might need these squared too so we can compare
    Vector3 velocity = Vector3.zero;

    public float energyUsedPerUnitMoved = 1f;

    public float maxMoveSpeed = 10f;
    public float maxMoveSpeedBack = 0f;
    public float moveAcceleration = 5f;
    public float moveFriction = 4f;
    float currentMoveSpeed = 0f;

    public float maxTurnSpeed = 100f;
    public float turnAcceleration = 15f;
    public float turnFriction = 10f;
    float currentTurnSpeed = 0f;

    float angle;

    //RIGIDBODY MOVEMENT
    public bool useRigidbody = false;
    Rigidbody rb;
    float targetAngle;
    float targetMovement = 0;

    Energy energy;

    List<TerrainZone> activeTerrainZones = new List<TerrainZone>();

    public ParticleSystem sparksPS;

	void Start () {
        rb = GetComponent<Rigidbody>();
        targetAngle = transform.eulerAngles.y;
        energy = FindObjectOfType<Energy>();

        currentTerrain = Utility.Terrain.normal;
	}
	
	void Update () {
        //input
        //tank controls - makes it easier to control the turn speed and speed in general
        //rotate with a and d = horizontal axis
        //accel/decel with w and s = vertical axis
        float speedInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");
        if (useSmoothAxes) {
            speedInput = Input.GetAxis("Vertical");
            turnInput = Input.GetAxis("Horizontal");
        }
        float _turnFriction = turnFriction;
        float _moveFriction = moveFriction;

        if (currentTerrain == Utility.Terrain.oil) {
            speedInput = speedInput * 0.05f;
            turnInput = turnInput * 0.05f;
            _turnFriction = turnFriction * 1f;
            _moveFriction = moveFriction * 0.01f;
        }

        //Debug.Log("speedInput: " + speedInput + ", turnInput: " + turnInput + " - turnFriction: " + _turnFriction + ", moveFriction: " + _moveFriction);
        //modify the current move and turn speeds, according to input and
        //is friction only applied when no input is given on the axis?

        //MOVING
        currentMoveSpeed += speedInput * moveAcceleration * Time.deltaTime;
        if (currentMoveSpeed != 0 && speedInput == 0) {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, 0, _moveFriction * Time.deltaTime);            
        }
        currentMoveSpeed = Mathf.Clamp(currentMoveSpeed, -maxMoveSpeedBack, maxMoveSpeed);

        //TURNING
        if (currentMoveSpeed > 0.1f) {
            turnInput = turnInput * (currentMoveSpeed / maxMoveSpeed);
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
            if (currentTurnSpeed != 0 && turnInput == 0) {
                currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0, _turnFriction * Time.deltaTime);
            }
            currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

            if (currentTurnSpeed > 0) {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, transform.eulerAngles.y + 90, Mathf.Abs(currentTurnSpeed) * Time.deltaTime);
            } else if (currentTurnSpeed < 0) {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, transform.eulerAngles.y - 90, Mathf.Abs(currentTurnSpeed) * Time.deltaTime);
            }
        }

        if (!canMove) {
            //Debug.Log("Can't move, setting input and speeds to 0");
            currentMoveSpeed = 0;
            currentTurnSpeed = 0;
            speedInput = 0;
            turnInput = 0;
        }
        //calculate the distance moved this frame, report this and the terraintype to the energy script - or do calculation here?
        //TODO: we need to make sure
        float distanceThisFrame = (transform.position - (transform.position + transform.forward * (currentMoveSpeed * Time.deltaTime))).magnitude;
        if (maxMoveSpeed > 0 && speedInput != 0) {
            if (energy.SpendEnergy(distanceThisFrame * energyUsedPerUnitMoved)) {
                //we can move
            } else {
                //we can't move anymore, game over man game over
                maxMoveSpeed = 0;
            }
        }

        if (!useRigidbody) {
            transform.eulerAngles = new Vector3(0, angle, 0);           
            transform.position = transform.position + transform.forward * (currentMoveSpeed * Time.deltaTime);
        }

        //TODO: this has to report to the energy thing 


        //Debug.Log("Turning: " + currentTurnSpeed + ", Moving: " + currentMoveSpeed);
        /*
        Vector3 inputVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //now we need to figure out how much this input will change the current velocity
        float velocityChangeSqr = (velocity + inputVelocity).sqrMagnitude;
        transform.position = transform.position + (inputVelocity * Time.deltaTime);

        velocity += inputVelocity;

        //align the player with the velocity
        float lookAngle = Mathf.Atan2(velocity.x, velocity.z);
        transform.eulerAngles = new Vector3(0, lookAngle, 0);
        //TODO: this needs to take the slope into account! that could actually be done using just the player model(this is a moving empty)
        */
	}

    public void SetPosition(Vector3 pos, Vector3 posVector) {
        transform.position = pos;
        currentMoveSpeed = 0;
        currentTurnSpeed = 0;
        //TODO: calculate the vector from vecpos to pos, shift it over to pos
        //then turn it into an angle
        Vector3 angleVector = pos - posVector;
        //TODO: angle is wrong! if over 180 degrees messes up.
        //float _angle = Vector3.Angle(Vector3.forward, angleVector);
        float _angle = Vector3.SignedAngle(Vector3.forward, angleVector, Vector3.up);
        //float vAngle = Vector3.Angle(transform.forward, Vector3.Scale(transform.InverseTransformPoint(target.position), Vector3(1, 0, 1));
        //vAngle = Vector3.Dot(Vector3.right, transform.InverseTransformPoint(target.position)) > 0.0 ? vAngle : -vAngle;
        //Debug.Log("Angle: " + _angle);
        transform.rotation = Quaternion.Euler(0, _angle, 0);
        angle = _angle;

        //TODO:
        canMove = true;
        sparksPS.Stop();
        energy.Stuck(false);
    }

    public void OnTinesCollision(Collision collision) {
        if (collision.gameObject.tag == "Obstacle") {
            
        }
    }

    public void OnTinesTrigger(Collider other) {
        if (other.tag == "Obstacle") {
            Debug.Log("Tines have bumped into an obstacle!");
            canMove = false;
            sparksPS.Play();
            energy.Stuck(true);           
        }
    }

    void OnCollisionEnter(Collision collision) {
        //Debug.Log("We've entered a collider! " + collision.gameObject.name);
    }

    public void HaveEnteredTerrain(TerrainZone terrain) {
        activeTerrainZones.Add(terrain);
    }

    public void HaveLeftTerrain(TerrainZone terrain) {
        activeTerrainZones.Remove(terrain);
    }

    void FixedUpdate() {
        if (useRigidbody) {
            rb.MoveRotation(Quaternion.Euler(0, angle, 0));
            rb.MovePosition(transform.position + transform.forward * (currentMoveSpeed * Time.deltaTime));
        }
    }
}
