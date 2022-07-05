using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuHandler : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField widthInput;
    [SerializeField] private TMPro.TMP_InputField heightInput;
    [SerializeField] private Button startButton;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
    public void StartRendering(){
        if (widthInput.text == "" || widthInput.text == "-"){
            ScreenData.width = 1920;
        }
        else{
            ScreenData.width = (uint)Mathf.Abs(int.Parse(widthInput.text));
        }

        if (heightInput.text == "" || heightInput.text == "-"){
            ScreenData.height = 1080;
        }
        else{
            ScreenData.height = (uint)Mathf.Abs(int.Parse(heightInput.text));
        }
        SceneManager.LoadScene("CornellBox");
    }
}
