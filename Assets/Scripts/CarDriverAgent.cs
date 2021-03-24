using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarDriverAgent : Agent
{
    Rigidbody rBody;

    // Current spawnpoint is manually created
    public Transform spawnPoint;
    public Transform targetPoint;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;
        transform.position = spawnPoint.position;
    }

    public void Update()
    {
        // Simple "Die if you fall off"
        if(transform.position.y < 0.0f)
        {
            EndEpisode();
        }

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, targetPoint.localPosition);
        // Simple "Win if reach point"
        if (distanceToTarget < 3f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Current actions/inputs are in Scripts/WheelDrive, will need to replace here with heuristic
        // discreteActionsOut[0]
        // discreteActionsOut[1]
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // We probably will want to use actual key presses here (discrete), instead of axises since they're continuous 
        //discreteActionsOut[0] = (int) Input.GetAxis("Horizontal");
        //discreteActionsOut[1] = (int) Input.GetAxis("Vertical");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Simple starting observations

        // Target and Agent positions
        sensor.AddObservation(targetPoint.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }
}
