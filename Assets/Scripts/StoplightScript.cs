using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoplightScript : MonoBehaviour
{

    public float LightIntensity = 2;
    public Light redLight;
    public Light yellowLight;
    public Light greenLight;
    public (Color, float)[] phases = {
        (Color.Green, 10),
        (Color.Yellow, 2),
        (Color.Red, 5),
        (Color.Green, 8),
        (Color.Yellow, 2),
        (Color.Red, 7),
        (Color.Green, 12),
        (Color.Yellow, 2),
        (Color.Red, 3)
    };
    private int phaseIndex = 0;

    public enum Color
    {
        Red,
        Yellow,
        Green
    }

    public Color CurrentColor
    {
        get
        {
            return phases[phaseIndex].Item1;
        }
    }

    private void onColorChanged(Color newColor)
    {
        switch (newColor)
        {
            case Color.Red:
                redLight.intensity = 1;
                yellowLight.intensity = 0;
                greenLight.intensity = 0;
                this.tag = "RedLight";
                break;
            case Color.Yellow:
                redLight.intensity = 0;
                yellowLight.intensity = 1;
                greenLight.intensity = 0;
                this.tag = "YellowLight";
                break;
            case Color.Green:
                redLight.intensity = 0;
                yellowLight.intensity = 0;
                greenLight.intensity = 1;
                this.tag = "GreenLight";
                break;
        }
    }

    // Advances phase, and recursively invokes next phase change
    // according to current phase length
    private void nextColor()
    {
        phaseIndex = (phaseIndex + 1) % phases.Length;
        print("going to phase " + phaseIndex);
        onColorChanged(CurrentColor);
        Invoke("nextColor", phases[phaseIndex].Item2);
    }

    // Start is called before the first frame update
    void Start()
    {
        // start on random phase 
        // will always be "same", but random with respect to other stoplights in scene
        phaseIndex = Random.Range(0, phases.Length);
        nextColor();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
