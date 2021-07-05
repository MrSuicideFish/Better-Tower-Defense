using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    // Constants
    private const int GROUND_CHECK_LAYER = 30;
    
    private Camera _camera;
    public Camera Camera
    {
        get
        {
            if (_camera == null)
            {
                _camera = this.GetComponent<Camera>();
            }

            return _camera;
        }
    }
    
    public float minZoom = -10;
    public float maxZoom = 10;
    
    public float zoomSpeed;
    public float moveSpeed;
    public float rotationSpeed;
    public float height;
    public float moveSmoothTime;
    public float rotationSmoothTime;

    private Vector3 _pointPosition;
    private Vector3 _toPosition;
    private Vector3 _toRotation;
    private Vector3 _groundPosition;
    private Vector3 _rotationDragStart;

    private float _rotationAngle;
    private float _zoom;

    public void Move(float x, float z)
    {
        var vec = new Vector3(x, 0, z) * moveSpeed;
        _pointPosition += Camera.transform.TransformDirection(vec);
        _pointPosition.y = 0;
    }

    public void Rotate(float rotation)
    {
        _rotationAngle += rotation * rotationSpeed * Time.deltaTime;
    }

    public void Zoom(float amount)
    {
        _zoom += amount * zoomSpeed;
    }
    
    private Vector3 GetGroundedPosition(Vector3 pos)
    {
        RaycastHit groundHit;
        Ray groundRay = new Ray(new Vector3()
        {
            x = pos.x,
            y = 100,
            z = pos.z
        }, Vector3.down);
        
        if (Physics.Raycast(groundRay, out groundHit, 100, 1 << GROUND_CHECK_LAYER))
        {
            return groundHit.point;
        }

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float enter = 0;
        if (plane.Raycast(groundRay, out enter))
        {
            return groundRay.GetPoint(enter);
        }

        return pos;
    }
    
    private void Start()
    {
        _pointPosition = transform.position;
        _pointPosition.y = 0;
        _zoom = (maxZoom + minZoom) / 2;
    }

    private void FixedUpdate()
    {
        // limit zoom
        _zoom = Mathf.Clamp(_zoom, minZoom, maxZoom);
        
        // calc final pos/rot
        Vector3 groundedPos = GetGroundedPosition(_pointPosition);
        Vector3 targetPosition = new Vector3()
        {
            x = groundedPos.x + (Mathf.Cos(_rotationAngle) * _zoom),
            y = groundedPos.y + (height * _zoom),
            z = groundedPos.z + (Mathf.Sin(_rotationAngle) * _zoom)
        };
        
        // apply calculations
        _toRotation = Vector3.Slerp(_toRotation, groundedPos - targetPosition,
            rotationSmoothTime * Time.deltaTime);
        _toPosition = Vector3.Lerp(_toPosition, targetPosition,
            moveSmoothTime * Time.deltaTime);
    }

    private void Update()
    {
        Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Zoom(-Input.mouseScrollDelta.y);
        Rotate(Input.GetMouseButton(1) ? -Input.GetAxis("Mouse X") : 0);
        
        // apply pos and rot
        transform.forward = _toRotation;
        transform.position = _toPosition;
    }
}
