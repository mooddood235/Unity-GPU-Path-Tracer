using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float transformSensitivity;
    [SerializeField] private float rotateSensitivity;
    [SerializeField] private float zoomSensitivity;
    private float horizontal;
    private float vertical;
    private void Update() {
        horizontal = Input.GetAxis("Mouse X");
        vertical = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(0)) Transform();
        if (Input.GetMouseButton(1)) Rotate();
        Zoom();
    }
    private void Transform(){
        transform.position += (transform.right * -horizontal + transform.up * -vertical) * transformSensitivity;
    }
    private void Rotate(){
        transform.Rotate(-vertical * rotateSensitivity, 0, 0, Space.Self);
        transform.Rotate(0, horizontal * rotateSensitivity, 0, Space.World);
    }
    private void Zoom(){
        transform.position += transform.forward * Input.mouseScrollDelta.y * zoomSensitivity;
    }
}
