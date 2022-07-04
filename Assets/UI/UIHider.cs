using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIHider : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    void Awake(){
        canvasGroup = this.GetComponent<CanvasGroup>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)){
            if (canvasGroup.blocksRaycasts){
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
            else{
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
}
