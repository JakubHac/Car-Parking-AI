using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] private bool _switchTargets = false;

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;
    List<ParkingAgent> agents = new List<ParkingAgent>();

	void Start(){
		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
        agents.AddRange(FindObjectsByType<ParkingAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None));
	}

    private void Update()
    {
        if (_switchTargets)
        {
            carTransform = agents.Select(x => (x.transform, currentReward: x.CurrentReward)).OrderByDescending(x => x.currentReward).FirstOrDefault().transform;
        }
    }

    void LateUpdate()
	{
		//Look at car
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

		//Move to car
		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
	}

}
