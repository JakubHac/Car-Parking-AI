using UnityEngine;

namespace DefaultNamespace
{
    public class Observer : MonoBehaviour
    {
        [SerializeField] private float _speedObservation;
        [SerializeField] private float _distanceObservation;
        [SerializeField] private float _desiredAngleObservation;
        [SerializeField] private float _angleToTargetObservation;
        [SerializeField] private Transform _carTarget;
        [SerializeField] private Rigidbody _carRigidbody;
        [SerializeField] private Vector3 _direction;


        private void Update()
        {
            _speedObservation = _carRigidbody.linearVelocity.magnitude / 25f;
            _distanceObservation = Vector3.Distance(_carTarget.position, transform.position) / 90f;
            _desiredAngleObservation = GetDiffToDesiredAngle();
            _direction = _carTarget.position - transform.position;
            _angleToTargetObservation = Vector3.SignedAngle(_direction, transform.forward, Vector3.up) / 180f;
        }

        private float GetDiffToDesiredAngle()
        {
            return Mathf.Repeat(_carTarget.eulerAngles.y - transform.eulerAngles.y + 180f, 360f) / 360f * 2f - 1f;
        }
    }
}
