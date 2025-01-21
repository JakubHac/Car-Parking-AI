using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace DefaultNamespace
{
    public class ParkingAgent : Agent
    {
        [SerializeField] private Rigidbody _carRigidbody;
        [SerializeField] private SceneReset _sceneReset;
        [SerializeField] private Transform _carTarget;
        [SerializeField] private PrometeoCarController _carController;
        [SerializeField] private float desiredDistance = 0.1f;
        [SerializeField] private float distancePenaltyMultiplier = 5f;
        [SerializeField] private float desiredSpeed = 0f;
        [SerializeField] private float desiredAngle = 3f;
        [SerializeField] private float anglePenaltyMultiplier = 0.1f;
        [SerializeField] private int _customMaxStep = 15000;


        public override void OnEpisodeBegin()
        {
            _sceneReset.ResetScene();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            base.CollectObservations(sensor);
            sensor.AddObservation(transform.InverseTransformDirection(_carRigidbody.linearVelocity)); //3
            sensor.AddObservation(transform.position); //3
            sensor.AddObservation(_carRigidbody.rotation); //4
            sensor.AddObservation(_carTarget.position); //3
            sensor.AddObservation(_carTarget.rotation); //4
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var acceleration = actions.DiscreteActions[0];
            var steering = actions.DiscreteActions[1];
            _carController.AiWantsToGoForward = acceleration == 0;
            _carController.AiWantsToGoBackward = acceleration == 2;
            _carController.AiWantsToTurnLeft = steering == 0;
            _carController.AiWantsToTurnRight = steering == 2;

            var distance = Vector3.Distance(transform.position, _carTarget.position);
            var angle = Quaternion.Angle(transform.rotation, _carTarget.rotation);
            var speed = _carRigidbody.linearVelocity.magnitude;
            bool isCloseEnough = distance < desiredDistance;
            bool isAligned = angle < desiredAngle;
            bool isNotMoving = speed < desiredSpeed;
            bool isDone = isCloseEnough && isAligned && isNotMoving;

            if (isDone)
            {
                AddReward(10f);
            }

            AddReward(-1f / _customMaxStep);

            if (StepCount > _customMaxStep)
            {
                if (!isCloseEnough)
                {
                    AddReward(-distancePenaltyMultiplier * distance);
                }

                if (!isAligned)
                {
                    AddReward(-anglePenaltyMultiplier * angle);
                }

                EpisodeInterrupted();
            }
        }
    }
}
