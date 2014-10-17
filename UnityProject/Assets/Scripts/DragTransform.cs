using UnityEngine;
using System.Collections;

class DragTransform : MonoBehaviour
{
	public Collider chipCollider = null;
	public Collider tableCollider = null;

	private Color mouseOverColor = Color.blue;
	private Color originalColor = Color.yellow;
	private bool _isDragging = false;
	private float _distance = 0;


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
				
				//Debug.Log("Raycast, _initialDraggingMousePosition = " + _initialDraggingMousePosition);
			}
			else
				Debug.Log("No Raycast");
		}
	}
	
	private void UpdateDragging()
	{
		if(_isDragging)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 rayPoint = ray.GetPoint(_distance);
			transform.position = rayPoint;
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
}