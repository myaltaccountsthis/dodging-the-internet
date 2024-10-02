using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PixelPerfectUI : MonoBehaviour
{
    private Camera MainCamera;
    void Start() {
        MainCamera = Camera.main;
        AdjustScalingFactor();
    }

    void LateUpdate() {
        AdjustScalingFactor();
    }

    void AdjustScalingFactor() {
        GetComponent<CanvasScaler>().scaleFactor = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
    }
}