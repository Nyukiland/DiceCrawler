using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementControl : MonoBehaviour
{
    [SerializeField]
    GameObject diceCarrier;

    [SerializeField]
    LayerMask layerToIgnore;

    [SerializeField]
    float throwStrength, torqueStrength;

    DiceComponent diceSelected;

    Vector3 previousPos;

    Ray ray;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ControlPosDiceCarrier();
        ControlRotDiceCarrier();
        ControlDiceGet();

        previousPos = diceCarrier.transform.position;
    }

    void ControlPosDiceCarrier()
    {
        float point;
        Plane plane = new Plane(Vector3.up, Vector3.up*2);

        if (plane.Raycast(ray, out point))
        {
            diceCarrier.transform.position = ray.GetPoint(point);
        }
    }

    void ControlRotDiceCarrier()
    {
        if (diceSelected != null) return;


    }

    void ControlDiceGet()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GrabDice();
        }

        if (diceSelected == null) return;

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseDice();
        }
    }

    void GrabDice()
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerToIgnore))
        {
            if (hit.collider.GetComponent<DiceComponent>() && hit.collider.GetComponent<DiceComponent>().IsItPickeable())
            {
                diceSelected = hit.collider.GetComponent<DiceComponent>();
                diceSelected.transform.parent = diceCarrier.transform;
                diceSelected.transform.localPosition = Vector3.zero;
                diceSelected.transform.localEulerAngles = Vector3.zero;
                diceSelected.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    void ReleaseDice()
    {
        diceSelected.transform.parent = null;

        if (diceSelected.IsItGridPlacable())
        {

        }
        else
        {
            diceSelected.GetComponent<Rigidbody>().isKinematic = false;
            diceSelected.GetComponent<Rigidbody>().AddForce((diceCarrier.transform.position - previousPos) * throwStrength, ForceMode.Impulse);
            diceSelected.GetComponent<Rigidbody>().AddTorque((diceCarrier.transform.position - previousPos) * torqueStrength, ForceMode.Impulse);
        }

        diceSelected = null;
    }
}
