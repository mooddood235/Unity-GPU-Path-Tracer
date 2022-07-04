using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TMPro.TMP_Dropdown))]
public class SceneHandler : MonoBehaviour
{
    private TMPro.TMP_Dropdown dropDown;
    private void Awake() {
        dropDown = this.GetComponent<TMPro.TMP_Dropdown>();
    }
    public void LoadScene(){
        SceneManager.LoadScene(dropDown.value);
    }
}
