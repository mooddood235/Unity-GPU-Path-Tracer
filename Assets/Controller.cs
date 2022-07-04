using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    [SerializeField] private float transformSensitivity;
    [SerializeField] private float rotateSensitivity;
    [SerializeField] private float zoomSensitivity;
    private float horizontal;
    private float vertical;
    private void Update() {
        if (MouseOverUI()) return;
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
    private bool MouseOverUI(){
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> rayCastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, rayCastResults);

        foreach (RaycastResult rayCastResult in rayCastResults){
            if (rayCastResult.gameObject.layer == LayerMask.NameToLayer("UI")){
                return true;
            }
        }
        return false;
    }
}
