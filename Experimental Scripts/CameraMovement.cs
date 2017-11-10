using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CameraMovement : MonoBehaviour {

    [SerializeField] GameObject player;
    [Range(0f, 10f)] [SerializeField] private float turnSpeed = 1.5f;   // How fast the rig will rotate from user input.
    [SerializeField] private float maxVerticalTilt = 75f;                       // The maximum value of the x axis rotation of the pivot.
    [SerializeField] private float minVerticalTilt = 45f;                       // The minimum value of the x axis rotation of the pivot.

    private float horizontalLook;                    // The rig's y axis rotation.
    private float verticalTilt;                    // The pivot's x axis rotation.

    private Vector3 cameraArmEulers;
    private Quaternion m_PivotTargetRot;
    private Quaternion m_TransformTargetRot;

    private Transform cameraTransform;
    private Transform pivotTransform;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // find the camera in the object hierarchy
        cameraTransform = GetComponentInChildren<Camera>().transform;
        pivotTransform = cameraTransform.parent;

        cameraArmEulers = transform.rotation.eulerAngles;

        m_PivotTargetRot = pivotTransform.transform.localRotation;
        m_TransformTargetRot = transform.localRotation;
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void UpdateCameraPosition()
    {
        transform.position = player.transform.position;
    }

    private void UpdateCameraRotation()
    {
        RotateCameraVertical();
        RotateCameraHorizontal();
    }

    private void RotateCameraVertical()
    {
        var y = CrossPlatformInputManager.GetAxis("Mouse Y");

        // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
        verticalTilt -= y * turnSpeed;
        // and make sure the new value is within the tilt range
        verticalTilt = Mathf.Clamp(verticalTilt, -minVerticalTilt, maxVerticalTilt);

        // Tilt input around X is applied to the pivot (the child of this object)
        m_PivotTargetRot = Quaternion.Euler(verticalTilt, cameraArmEulers.y, cameraArmEulers.z);

        pivotTransform.localRotation = m_PivotTargetRot;

    }

    private void RotateCameraHorizontal()
    {
        var x = CrossPlatformInputManager.GetAxis("Mouse X");

        // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
        horizontalLook += x * turnSpeed;

        m_TransformTargetRot = Quaternion.Euler(0f, horizontalLook, 0f);

        // Rotate the rig (the root object) around Y axis only:
        transform.localRotation = m_TransformTargetRot;


    }
}
