using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeamEmitterEvents : MonoBehaviour
{
    // first event should trigger when the player hands in tier 1 fuel for the first time
    // this will make a beam that is 2 wide and 2 tall that goes into the middle
    // also makes a beam from the middle that goes to the top and bottom of the station


    // second event should trigger when the player hands in tier 2 fuel for the first time
    // this will make a beam that is 4 wide and 4 tall
    // will make a beam that starts the warp gate

    // third event should trigger when the player hands in tier 3 fuel for the first time
    // not sure what it should do, maybe send a beam through the warp gate that blows up something on the other side?

    [SerializeField]
    GameObject Beam;

    [SerializeField]
    GameObject WarpGate;

    [SerializeField]
    Material BeamMaterial1;
    [SerializeField]
    Material BeamMaterial2;
    [SerializeField]
    Material BeamMaterial3;

    GameObject EnergyCenter;
    GameObject EnergyCore;
    GameObject StationCenter;

    float BeamZPosGoal = 110f;
    float BeamYScaleGoal = 140f;

    List<float> CenterScaleGoals = new List<float>{25f, 70f, 125f};
    List<float> CenterGrowSpeeds = new List<float>{.05f, .0625f, .075f};
    List<float> BeamThicknesses = new List<float> { 1f, 2f, 4f };
    List<Material> MaterialGoals;

    GameObject lastBeam;

    int timesDone = 0;
    float beamGrowSpeed = .5f;

    void Start()
    {
        EnergyCenter = GameObject.Find("EnergyCenter");
        EnergyCore = GameObject.Find("EnergyCore");
        StationCenter = GameObject.Find("StationCenter");
        MaterialGoals = new List<Material> { BeamMaterial1, BeamMaterial2, BeamMaterial3 };
    }

    bool GrowBeam = false;
    GameObject BeamToGrow = null;
    bool GrowCenter = false;
    bool needsToLerpColor = false;

    void Update()
    {
        if (timesDone == 3)
        {
            GrowBeam = false;
        }
        if (GrowBeam) {
            if (BeamToGrow == null)
            {
                BeamToGrow = Instantiate(Beam, new Vector3(0,0, -20), Quaternion.identity);
                BeamToGrow.transform.localScale = new Vector3(BeamThicknesses[timesDone], BeamToGrow.transform.localScale.y, BeamThicknesses[timesDone]);
                BeamToGrow.GetComponent<Renderer>().material = MaterialGoals[timesDone];
                BeamToGrow.transform.SetParent(Beam.transform.parent, false);
                BeamToGrow.transform.rotation = Beam.transform.rotation;
            }
            growBeam(BeamToGrow);
        }
        if (GrowCenter)
        {
            growCenter(EnergyCenter);
        }
        if (needsToLerpColor)
        {
            EnergyCenter.GetComponent<Renderer>().material.Lerp(EnergyCenter.GetComponent<Renderer>().material, MaterialGoals[timesDone], .01f);
            // if we are done lerping then stop lerping
            if (EnergyCenter.GetComponent<Renderer>().material.color == MaterialGoals[timesDone].color)
            {
                needsToLerpColor = false;
            }
        }
        // if the player presses g grow the beams
        if (Input.GetKeyDown(KeyCode.G))
        {
            GrowBeam = true;
        }
    }



    void growBeam(GameObject beam)
    {
        // if the z pos and y scale dont match the goal, then grow the beam
        if (beam.transform.localPosition.z < BeamZPosGoal)
        {
            beam.transform.localPosition = new Vector3(BeamToGrow.transform.localPosition.x, BeamToGrow.transform.localPosition.y, BeamToGrow.transform.localPosition.z + beamGrowSpeed);
            beam.transform.localScale = new Vector3(BeamToGrow.transform.localScale.x, BeamToGrow.transform.localScale.y + beamGrowSpeed, BeamToGrow.transform.localScale.z);
            // do the oposite to the oldBeam
            if (lastBeam != null)
            {
                lastBeam.transform.localPosition = new Vector3(lastBeam.transform.localPosition.x, lastBeam.transform.localPosition.y, lastBeam.transform.localPosition.z + beamGrowSpeed);
                lastBeam.transform.localScale = new Vector3(lastBeam.transform.localScale.x, lastBeam.transform.localScale.y - beamGrowSpeed, lastBeam.transform.localScale.z);
            }
        }
        // if the z pos and y scale match the goal, then stop growing the beam
        if (beam.transform.localPosition.z >= BeamZPosGoal && BeamToGrow.transform.localScale.y >= BeamYScaleGoal)
        {
            GrowBeam = false;
            GrowCenter = true;
            
        }
    }


    void growCenter(GameObject EnergyCenter)
    {
        // grow the center x y and z scale until it hits centerscalegoal[timesDone]
        if (EnergyCenter.transform.localScale.x < CenterScaleGoals[timesDone])
        {
            EnergyCenter.transform.localScale = new Vector3(EnergyCenter.transform.localScale.x + CenterGrowSpeeds[timesDone], EnergyCenter.transform.localScale.y + CenterGrowSpeeds[timesDone], EnergyCenter.transform.localScale.z + CenterGrowSpeeds[timesDone]);
            needsToLerpColor = true;
        }
        else if (EnergyCenter.transform.localScale.x > CenterScaleGoals[timesDone] + CenterGrowSpeeds[timesDone])
        {
            EnergyCenter.transform.localScale = new Vector3(EnergyCenter.transform.localScale.x - CenterGrowSpeeds[timesDone], EnergyCenter.transform.localScale.y - CenterGrowSpeeds[timesDone], EnergyCenter.transform.localScale.z - CenterGrowSpeeds[timesDone]);
            needsToLerpColor = true;
        }
        else
        {
            if (lastBeam != null)
            {
                Destroy(lastBeam);
            }
            lastBeam = BeamToGrow;
            BeamToGrow = null;
            GrowCenter = false;
            if (timesDone == 2)
                timesDone = 1;
            else
                timesDone++;
            StationCenter.GetComponent<EnergyCoreEvents>().StartGrowingBeams();

        }
    }
}
