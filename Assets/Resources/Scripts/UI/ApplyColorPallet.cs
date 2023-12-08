using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyColorPallet : MonoBehaviour
{

    Color Color1; // Dark BLue
    Color Color2; // Orange
    Color Color3; // Yellow
    Color Color4; // Light Grey
    Color Color5; // Black


    Color CreateColor(int r, int g, int b, int a)
    {
        return new Color((float)r/255f, g/255f, b/255f, a);
    }


    // Start is called before the first frame update
    void Start()
    {
        Color1 = CreateColor(8, 19, 63, 1);
        Color2 = CreateColor(225, 95, 1, 1);
        Color3 = CreateColor(244, 172, 0, 1);
        Color4 = CreateColor(224, 220, 215, 1);
        Color5 = CreateColor(0, 0, 0, 1);

        // make a list for the colors
        List<Color> colors = new List<Color>();
        colors.Add(Color1);
        colors.Add(Color2);
        colors.Add(Color3);
        colors.Add(Color4);
        colors.Add(Color5);
            
        for (int x = 1; x <= 5; x++)
        {
            string name = "Color" + x.ToString();
            // make every button with the tag Color1Button use the color Color1
            foreach (var button in GameObject.FindGameObjectsWithTag(name))
            {
                Debug.Log("Setting " + button.name + " color to color" + x.ToString());
                button.GetComponent<UnityEngine.UI.Image>().color = colors[x-1];
            }
        }



}

}
