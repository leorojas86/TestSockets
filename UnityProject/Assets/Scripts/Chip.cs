using UnityEngine;
using System.Collections;

public class Chip : MonoBehaviour 
{

	#region Variables

	public Collider chipCollider = null;
	public Collider tableCollider = null;

	private Vector3 _initialChipDraggingPosition  = Vector3.zero;
	private Vector3 _initialDraggingMousePosition = Vector3.zero;

	private bool _isDragging = false;

	private float _distance = 0;

	#endregion

	#region Methods

	// Use this for initialization
	void Start() 
	{
	
	}

	
	// Update is called once per frame
	void Update() 
	{
		CheckForStartDragging();
		UpdateDragging();
		CheckForStopDragging();
	}

	private void CheckForStartDragging()
	{
		if(!_isDragging && Input.GetMouseButtonDown(0))
		{
			RaycastHit hit  = new RaycastHit();
			Ray ray 		= Camera.main.ScreenPointToRay(Input.mousePosition);

			if(chipCollider.Raycast(ray, out hit, 100.0f))
			{
				_distance = Vector3.Distance(transform.position, Camera.main.transform.position);
				_isDragging = true;
			}
			else
				Debug.Log("No Raycast");
		}
	}

	private void UpdateDragging()
	{
		if(_isDragging)
		{
			RaycastHit hit2  = new RaycastHit();
			Ray ray2 		= Camera.main.ScreenPointToRay(Input.mousePosition);
			
			if(tableCollider.Raycast(ray2, out hit2, 100.0f))
			{
				/*Ray ray 					= Camera.main.ScreenPointToRay(Input.mousePosition);
				Vector3 rayPoint 			= ray.GetPoint(_distance);
				Vector3 position 			= transform.parent.InverseTransformPoint(rayPoint);
				position.z 					= transform.localPosition.z;*/

				Vector3 hitPosition 		= transform.parent.InverseTransformPoint(hit2.point);
				hitPosition.z				= transform.localPosition.z;
				transform.localPosition 	= hitPosition;
			}
		}
	}

	private void CheckForStopDragging()
	{
		if(_isDragging && !Input.GetMouseButton(0))
		{
			Debug.Log("Stop Dragging");
			_isDragging	= false;
		}
	}

	#endregion
}
