using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ParkingAgent : Agent
    {
        [SerializeField] private Rigidbody _carRigidbody;
        [SerializeField] private SceneReset _sceneReset;
        [SerializeField] private Transform _carTarget;
        [SerializeField] private PrometeoCarController _carController;
        [SerializeField] private float desiredDistance = 0.1f;
        [SerializeField] private float desiredSpeed = 0f;
        [SerializeField] private float desiredAngle = 3f;
        [SerializeField] private int _customMaxStep = 15000;
        [SerializeField] private int _distanceRewardSteps = 100;
        [SerializeField] private int _distanceRewardPerStep = 1;
        [SerializeField] private float _backwardsPenalty = 0.005f;
        [SerializeField] private Text _rewardText;
        [SerializeField] private float _speedObservation;
        [SerializeField] private float _distanceObservation;
        [SerializeField] private float _desiredAngleObservation;
        [SerializeField] private float _angleToTargetObservation;

        private Vector3 _episodeStartPos;
        private bool _useRewardText = false;
        private int _defaultLayer = 0;
        [FormerlySerializedAs("_currentReward")] public float CurrentReward = 0;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == _defaultLayer)
            {
                int remainingSteps = _customMaxStep - StepCount;
                float remainingStepsNormalized = remainingSteps / (float) _customMaxStep;
                AddReward(-10f * remainingStepsNormalized);
                EndEpisode();
            }
        }

        public override void OnEpisodeBegin()
        {
            _sceneReset.ResetScene();
            _episodeStartPos = transform.position;
            _useRewardText = _rewardText != null;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            base.CollectObservations(sensor);

            _speedObservation = _carRigidbody.linearVelocity.magnitude / 25f;
            _distanceObservation = Vector3.Distance(_carTarget.position, transform.position) / 90f;
            _desiredAngleObservation = GetDiffToDesiredAngle();
            var direction = _carTarget.position - transform.position;
            _angleToTargetObservation = Vector3.SignedAngle(direction, transform.forward, Vector3.up) / 180f;

            sensor.AddObservation(_speedObservation); //1
            sensor.AddObservation(_distanceObservation); //1
            sensor.AddObservation(_desiredAngleObservation); //1
            sensor.AddObservation(_angleToTargetObservation); //1
        }

        private float GetDiffToDesiredAngle()
        {
            return ((_carTarget.eulerAngles.y - transform.eulerAngles.y) / 360f) * 2f - 1f;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var aiMovement = actions.DiscreteActions[0];
            _carController.AiWantsToGoForward = aiMovement < 3;
            _carController.AiWantsToGoBackward = aiMovement > 3;
            _carController.AiWantsToTurnLeft = aiMovement is 0 or 2;
            _carController.AiWantsToTurnRight = aiMovement is 4 or 6;

            if (_carController.AiWantsToGoBackward)
            {
                AddReward(-_backwardsPenalty);
            }

            var distance = Vector3.Distance(transform.position, _carTarget.position);
            var angle = Quaternion.Angle(transform.rotation, _carTarget.rotation);
            var speed = _carRigidbody.linearVelocity.magnitude;
            bool isCloseEnough = distance < desiredDistance;
            bool isAligned = angle < desiredAngle;
            bool isNotMoving = speed < desiredSpeed;
            bool isDone = isCloseEnough && isAligned && isNotMoving;

            int remainingSteps = _customMaxStep - StepCount;
            float remainingStepsNormalized = remainingSteps / (float) _customMaxStep;

            if (isDone)
            {
                AddReward(100f * remainingStepsNormalized + 10f);
                EndEpisode();
            }

            DistanceReward(distance, remainingStepsNormalized);
            AlignmentReward(remainingStepsNormalized);
            AddReward(-1f / _customMaxStep);

            CurrentReward = GetCumulativeReward();
            if (_useRewardText)
            {
                _rewardText.text = CurrentReward.ToString("F2");
            }

            if (StepCount >= _customMaxStep)
            {
                EpisodeInterrupted();
            }
        }

        private void AlignmentReward(float remainingStepsNormalized)
        {
            var angle = GetDiffToDesiredAngle();
            AddReward((1f - Mathf.Abs(angle)) * remainingStepsNormalized * 0.01f);
        }

        private void DistanceReward(float distance, float remainingStepsNormalized)
        {
            float startingDistance = Vector3.Distance(_episodeStartPos, _carTarget.position);
            float currentNomalizedDistance = Mathf.Clamp01(distance / startingDistance);
            int distanceSteps = Mathf.FloorToInt((1f - currentNomalizedDistance) * _distanceRewardSteps);

            AddReward(distanceSteps * _distanceRewardPerStep * remainingStepsNormalized * 0.05f);
        }
    }
}
