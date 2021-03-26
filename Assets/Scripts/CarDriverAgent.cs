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
    float time = 0.0f;
    float slowTime = 0.0f;
    bool isSlow = false;

  // Current spawnpoint is manually created
  public Transform spawnPoint;
  public List<Transform> targetPoints;

  private int currTargetPointIndex;
  public Transform targetPoint
  {
    get
    {
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
  }

  public override void OnEpisodeBegin()
  {
    rBody.velocity = Vector3.zero;
    transform.position = spawnPoint.position;
    transform.rotation = Quaternion.identity;
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

    // Die if you fall off
    if (transform.position.y < -1.0f)
    {
        AddReward(-.33f);
        EndEpisode();
    }

    if (rBody.velocity.magnitude <= .3f)
    {
        if(!isSlow)
        {
            slowTime = time;
        }
        isSlow = true;
        if(time - slowTime > 5f)
        {
            EndEpisode();
        }
    }

    // Add minute remward that increases as speed increases
    if(rBody.velocity.magnitude > 1f)
    {
        isSlow = false;
        AddReward(.0001f * rBody.velocity.magnitude);
    }
    

    float distanceToTarget = Vector3.Distance(this.transform.localPosition, targetPoint.localPosition);

    // add rewards if a target point is reached
    if (distanceToTarget < 3f)
    {
      AddReward(1.0f);

      // increment target point, end episode if at end
      if (incrementTargetPoint() == null)
      {
        EndEpisode();
      }

    }
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

    //discreteActionsOut[1] = axisToDiscrete((int)Input.GetAxis("Horizontal"));
    //discreteActionsOut[0] = axisToDiscrete((int)Input.GetAxis("Vertical"));
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    // Simple starting observations

    // Target and Agent positions
    sensor.AddObservation(targetPoint.localPosition - this.transform.localPosition);
    sensor.AddObservation(rBody.velocity.magnitude);

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
