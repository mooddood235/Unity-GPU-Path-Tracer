using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialUIHandler : MonoBehaviour
{
    [HideInInspector] public PathTracerHandler pathTracer;
    private Material mat;

    [SerializeField] private FlexibleColorPicker colorPicker;
    [SerializeField] private Slider specSlider;
    [SerializeField] private Slider metalSlider;
    [SerializeField] private Slider roughnessSlider;
    [SerializeField] private TMPro.TMP_InputField emissionR;
    [SerializeField] private TMPro.TMP_InputField emissionG;
    [SerializeField] private TMPro.TMP_InputField emissionB;


    public void SetMat(Material mat){
        this.mat = mat;
        colorPicker.SetColor(mat.albedo);
        specSlider.SetValueWithoutNotify(mat.specChance);
        metalSlider.SetValueWithoutNotify(mat.metalness);
        roughnessSlider.SetValueWithoutNotify(mat.roughness);
        emissionR.SetTextWithoutNotify(mat.emission.x.ToString());
        emissionG.SetTextWithoutNotify(mat.emission.y.ToString());
        emissionB.SetTextWithoutNotify(mat.emission.z.ToString());
    }

    public void SetColor(){
        if (!mat) return;
        mat.albedo = colorPicker.GetColor();
        pathTracer.ResetCurrSample();
    }
    public void SetSpec(){
        if (!mat) return;
        mat.specChance = specSlider.value;
        pathTracer.ResetCurrSample();
    }
    public void SetMetal(){
        if (!mat) return;
        mat.metalness = metalSlider.value;
        pathTracer.ResetCurrSample();
    }
    public void SetRoughness(){
        if (!mat) return;
        mat.roughness = roughnessSlider.value;
        pathTracer.ResetCurrSample();
    }
    public void SetEmission(){
        if (!mat) return;
        mat.emission = new Vector3(float.Parse(emissionR.text), float.Parse(emissionG.text), float.Parse(emissionB.text));
        pathTracer.ResetCurrSample();
    }
}
