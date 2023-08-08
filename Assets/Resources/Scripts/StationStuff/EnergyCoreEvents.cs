using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnergyCoreEvents : MonoBehaviour
{

    [SerializeField]
    GameObject Beam1;
    GameObject lastBeam1;
    [SerializeField]
    GameObject Beam2;
    GameObject lastBeam2;
    [SerializeField]
    GameObject Beam3;

    [SerializeField]
    GameObject WarpGate;

    [SerializeField]
    Material BeamMaterial1;
    [SerializeField]
    Material BeamMaterial2;
    [SerializeField]
    Material BeamMaterial3;

    GameObject EnergyCenter;

    float BeamYPosGoal = 135f;
    float Beam3ZPosGoal = 250f;
    //float BeamYScaleGoal = 165f; // should always be 30 more than the y pos goal

    List<float> CenterScaleGoals = new List<float> { .2f, 1.4f, 2.5f };
    List<float> BeamThicknesses = new List<float> { 1f, 2f, 4f };
    List<Material> MaterialGoals;

    int timesDone = 0;
    float beamGrowSpeed = .5f;
    float beam3GrowSpeed = 1f;

    void Start()
    {
        EnergyCenter = GameObject.Find("EnergyCenter");
        MaterialGoals = new List<Material> { BeamMaterial1, BeamMaterial2, BeamMaterial3 };

        for (int i = 0; i < CenterScaleGoals.Count; i++)
        {
            CenterScaleGoals[i] = CenterScaleGoals[i] / 2;
            BeamThicknesses[i] = BeamThicknesses[i] / 2;
        }

    }

    bool GrowBeams = false;
    bool GrowBeam3 = false;
    bool RemoveBeam3 = false;
    GameObject BeamToGrow1 = null;
    GameObject BeamToGrow2 = null;
    GameObject BeamToGrow3 = null;
    bool CreatedPortal = false;

    public void StartGrowingBeams()
    {
        GrowBeams = true;
    }

    void Update()
    {
        if (GrowBeams)
        {
            if (BeamToGrow1 == null)
            {
                BeamToGrow1 = Instantiate(Beam1, new Vector3(0, .02f, 0), Quaternion.identity);
                BeamToGrow1.transform.localScale = new Vector3(BeamThicknesses[timesDone], BeamToGrow1.transform.localScale.y, BeamThicknesses[timesDone]);
                BeamToGrow1.GetComponent<Renderer>().material = MaterialGoals[timesDone];
                BeamToGrow1.transform.SetParent(transform, false);
                BeamToGrow1.transform.rotation = transform.rotation;
                BeamToGrow1.SetActive(true);
            }
            if (BeamToGrow2 == null)
            {
                BeamToGrow2 = Instantiate(Beam2, new Vector3(0, -.02f, 0), Quaternion.identity);
                BeamToGrow2.transform.localScale = new Vector3(BeamThicknesses[timesDone], BeamToGrow2.transform.localScale.y, BeamThicknesses[timesDone]);
                BeamToGrow2.GetComponent<Renderer>().material = MaterialGoals[timesDone];
                BeamToGrow2.transform.SetParent(transform, false);
                BeamToGrow2.transform.rotation = transform.rotation;
                BeamToGrow2.SetActive(true);
            }
            if (BeamToGrow3 == null && timesDone > 0)
            {
                BeamToGrow3 = Instantiate(Beam3, new Vector3(0, 0, 0), Quaternion.identity);
                BeamToGrow3.transform.localScale = new Vector3(BeamThicknesses[timesDone]*4, BeamToGrow3.transform.localScale.y, BeamThicknesses[timesDone]*4);
                BeamToGrow3.GetComponent<Renderer>().material = MaterialGoals[timesDone];
                BeamToGrow3.transform.SetParent(transform, false);
                BeamToGrow3.transform.rotation = transform.rotation;
                BeamToGrow3.SetActive(true);
            }
            
            growBeams(BeamToGrow1, BeamToGrow2);
        }
        if (GrowBeam3)
        {
            if (timesDone == 2 && CreatedPortal)
            {
                GrowBeam3 = false;
                return;
            }
            if (BeamToGrow3.transform.localPosition.z < Beam3ZPosGoal)
            {
                BeamToGrow3.transform.localPosition = new Vector3(BeamToGrow3.transform.localPosition.x, BeamToGrow3.transform.localPosition.y, BeamToGrow3.transform.localPosition.z + beamGrowSpeed);
                BeamToGrow3.transform.localScale = new Vector3(BeamToGrow3.transform.localScale.x, BeamToGrow3.transform.localScale.y, BeamToGrow3.transform.localScale.z + beamGrowSpeed*2);
            }
            else
            {
                GrowBeam3 = false;
                // start the WarpGateEvent
                WarpGate.GetComponent<WarpGateEvents>().StartEvent(timesDone - 1);
                
                // call RemoveBeam3 in 3 seconds
                Invoke("Beam3Remove", 5);
            }
        }
        if (RemoveBeam3)
        {
            if (BeamToGrow3.transform.localScale.z > 0)
            {
                BeamToGrow3.transform.localPosition = new Vector3(BeamToGrow3.transform.localPosition.x, BeamToGrow3.transform.localPosition.y, BeamToGrow3.transform.localPosition.z + beam3GrowSpeed);
                BeamToGrow3.transform.localScale = new Vector3(BeamToGrow3.transform.localScale.x, BeamToGrow3.transform.localScale.y, BeamToGrow3.transform.localScale.z - beam3GrowSpeed*2);
            }
            else
            {
                RemoveBeam3 = false;
                WarpGate.GetComponent<WarpGateEvents>().StopEvent(timesDone - 1);
                CreatedPortal = true;
                Destroy(BeamToGrow3);
            }
        }
    }

    void Beam3Remove()
    {
        RemoveBeam3 = true;
    }

    void growBeams(GameObject beam1, GameObject beam2)
    {
        // if the z pos and y scale dont match the goal, then grow the beam
        if (beam1.transform.localPosition.y < BeamYPosGoal)
        {
            beam1.transform.localPosition = new Vector3(BeamToGrow1.transform.localPosition.x, BeamToGrow1.transform.localPosition.y + beamGrowSpeed, BeamToGrow1.transform.localPosition.z);
            beam1.transform.localScale = new Vector3(BeamToGrow1.transform.localScale.x, BeamToGrow1.transform.localScale.y + beamGrowSpeed, BeamToGrow1.transform.localScale.z);
            beam2.transform.localPosition = new Vector3(BeamToGrow2.transform.localPosition.x, BeamToGrow2.transform.localPosition.y - beamGrowSpeed, BeamToGrow2.transform.localPosition.z);
            beam2.transform.localScale = new Vector3(BeamToGrow2.transform.localScale.x, BeamToGrow2.transform.localScale.y + beamGrowSpeed, BeamToGrow2.transform.localScale.z);
            // if the lastbeams arent null , then shrink them
            if (lastBeam1 != null)
            {
                lastBeam1.transform.localPosition = new Vector3(lastBeam1.transform.localPosition.x, lastBeam1.transform.localPosition.y + beamGrowSpeed, lastBeam1.transform.localPosition.z);
                lastBeam1.transform.localScale = new Vector3(lastBeam1.transform.localScale.x, lastBeam1.transform.localScale.y - beamGrowSpeed, lastBeam1.transform.localScale.z);
                lastBeam2.transform.localPosition = new Vector3(lastBeam2.transform.localPosition.x, lastBeam2.transform.localPosition.y - beamGrowSpeed, lastBeam2.transform.localPosition.z);
                lastBeam2.transform.localScale = new Vector3(lastBeam2.transform.localScale.x, lastBeam2.transform.localScale.y - beamGrowSpeed, lastBeam2.transform.localScale.z);
            }
        }
        else
        {
            if (lastBeam1 != null)
            {
                Destroy(lastBeam1);
                Destroy(lastBeam2);
            }
            if (timesDone > 0)
                GrowBeam3 = true;
            GrowBeams = false;
            lastBeam1 = BeamToGrow1;
            lastBeam2 = BeamToGrow2;
            BeamToGrow1 = null;
            BeamToGrow2 = null;
            if (timesDone == 2)
                timesDone = 1;
            else
                timesDone++;
        }
    }
}
