using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarDriverAgent : Agent
{
    Rigidbody rBody;
    WheelDrive wd;
    RoadBuilding roadbuilder;
    float time = 0.0f;
    float slowTime = 0.0f;
    bool isSlow = false;

    public bool useRoadBuilder;

    // Current spawnpoint is manually created
    public List<Transform> spawnPoints;
    public List<Transform> targetPoints;
    public int curriculum = 1;

    private int currTargetPointIndex;
    public Transform targetPoint
    {
        get
        {
            if (targetPoints.Count <= 0) return gameObject.transform;
            return targetPoints[currTargetPointIndex];
        }
    }

  private Transform incrementTargetPoint()
  {
    if (currTargetPointIndex + 1 >= targetPoints.Count)
    {
      return null;
    }
    else
    {
      return targetPoints[++currTargetPointIndex];
    }
  }

  void Start()
  {
    rBody = GetComponent<Rigidbody>();
    wd = GetComponent<WheelDrive>();
    time = 0.0f;
    roadbuilder = GetComponent<RoadBuilding>();
  }

  public override void OnEpisodeBegin()
  {
        roadbuilder.RoadLength = roadbuilder.StdLength * (2 * curriculum);
        if (useRoadBuilder) targetPoints.Clear();
        if (useRoadBuilder) roadbuilder.CreateRoad();

        //transform.position = spawnPoint.position;
        //transform.rotation = Quaternion.identity;
        int rng = Random.Range(0, spawnPoints.Count - 1);
        //currTargetPointIndex = Random.Range(0, targetPoints.Count - 1);
        time = 0.0f;
        currTargetPointIndex = 0;
        transform.position = spawnPoints[rng].position;
        transform.rotation = spawnPoints[rng].rotation;
        rBody.velocity = Vector3.zero;

        //transform.position = new Vector3(Random.Range(-100, 100), 1f, Random.Range(-100, 100));
        //transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
    }

    private void Update()
    {
        time += Time.deltaTime;
    }

    public override void OnActionReceived(ActionBuffers actions)
  {
    // handle movement
    var discreteActionsOut = actions.DiscreteActions;

    MoveAgent(discreteActionsOut[0]);
    SteerAgent(discreteActionsOut[1]);

    // mete out rewards and punishment!!

    // make sure all 4 wheels are on the road!


    // The direction from the agent to the target
    Vector3 dirToTarget = (targetPoint.position - this.transform.position).normalized;
    // The alignment of the agent's velocity with this direction
    float velocityAlignment = Vector3.Dot(dirToTarget, rBody.velocity);
    AddReward((0.0001f * velocityAlignment)/curriculum);

    // Die if you fall off
    if (transform.localPosition.y < -.5f)
    {
        AddReward(-.5f);
        if (useRoadBuilder) roadbuilder.DestroyRoad();
        EndEpisode();
    }

    if (rBody.velocity.magnitude <= .4f)
    {
        if(!isSlow)
        {
            slowTime = time;
        }
        isSlow = true;
        if(time - slowTime > 20f)
        {
            AddReward(-1f);
            if (useRoadBuilder) roadbuilder.DestroyRoad();   
            EndEpisode();
        }
    }

    // Add minute remward that increases as speed increases
    if(rBody.velocity.magnitude > .4f)
    {
        isSlow = false;
        AddReward((.0001f * rBody.velocity.magnitude)/curriculum);
    }
    

    float distanceToTarget = Vector3.Distance(this.transform.position, targetPoint.position);

        //Debug.Log(distanceToTarget);

    // add rewards if a target point is reached
        if (distanceToTarget < 2f)
    {
      AddReward(1.5f);

        // increment target point, end episode if at end
        //if (incrementTargetPoint() == null)

        if (useRoadBuilder)
        {
            roadbuilder.DestroyRoad();
            //roadbuilder.RoadLength++;
        }
        EndEpisode();
    }
  }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GreenLight"))
        {
            AddReward(.5f);
        }
        if (other.gameObject.CompareTag("YellowLight"))
        {
            AddReward(.25f);
        }
        if (other.gameObject.CompareTag("RedLight"))
        {
            AddReward(-2f);
            if (useRoadBuilder) roadbuilder.DestroyRoad();
            EndEpisode();
        }
    }


        //Adds a goal point when a road is generated
        public void RegisterDrivePoint(Transform drivePoint) {
    targetPoints.Add(drivePoint);
  }

  //Removes all goal points when an episode ends using road generation
  public void RemoveDrivePoints() {
    targetPoints.Clear();
  }

  public void MoveAgent(int act)
  {
    var directionDrive = 0;

    switch (act)
    {
      case 0:
        directionDrive = 0;
        break;
      case 1:
        directionDrive = 1;
        break;
      case 2:
        directionDrive = -1;
        break;
    }

    wd.set_torque(directionDrive);
  }

  public void SteerAgent(int act)
  {
    var directionSteer = 0;

    switch (act)
    {
      case 0:
        directionSteer = 0;
        break;
      case 1:
        directionSteer = 1;
        break;
      case 2:
        directionSteer = -1;
        break;
    }

    wd.set_angle(directionSteer);
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var discreteActionsOut = actionsOut.DiscreteActions;
    // We probably will want to use actual key presses here (discrete), instead of axises since they're continuous
    if (Input.GetKey(KeyCode.UpArrow))
    {
        discreteActionsOut[0] = 1;
    }
    else if (Input.GetKey(KeyCode.DownArrow))
    {
        discreteActionsOut[0] = 2;
    }
    else
    {
        discreteActionsOut[0] = 0;
    }

    if (Input.GetKey(KeyCode.LeftArrow))
    {
        discreteActionsOut[1] = 2;
    }
    else if (Input.GetKey(KeyCode.RightArrow))
    {
        discreteActionsOut[1] = 1;
    }
    else
    {
        discreteActionsOut[1] = 0;
    }
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    // Target and Agent positions
    sensor.AddObservation(targetPoint.position - this.transform.position);
    sensor.AddObservation(targetPoint.position.x);
    sensor.AddObservation(targetPoint.position.z);

    // Agent velocity
    sensor.AddObservation(rBody.velocity.x);
    sensor.AddObservation(rBody.velocity.z);
  }

  private int axisToDiscrete(int axis)
  {
    switch (axis)
    {
      case -1:
        return 2;
      case 0:
        return 0;
      case 1:
        return 1;
      default:
        return 0;
    }
  }

  private int discreteToAxis(int discrete)
  {
    switch (discrete)
    {
      case 0:
        return 0;
      case 1:
        return 1;
      case 2:
        return -1;
      default:
        return 0;
    }
  }
}
