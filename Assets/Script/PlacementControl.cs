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

    [SerializeField, Range(0.01f, 10)]
    float stepForGrab, smoothDampDiceCarrier;

    DiceComponent diceSelected;

    Vector3 previousPos;

    Vector3 refVelo;

    Ray ray;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ControlPosDiceCarrier();
        //ControlRotDiceCarrier();
        ControlDiceGet();

        previousPos = diceCarrier.transform.position;
    }

    void ControlPosDiceCarrier()
    {
        float point;
        Plane plane = new Plane(Vector3.up, Vector3.up*2);

        if (plane.Raycast(ray, out point))
        {
            if (diceSelected != null) diceCarrier.transform.position = Vector3.SmoothDamp(diceCarrier.transform.position, ray.GetPoint(point), ref refVelo, smoothDampDiceCarrier);
            else diceCarrier.transform.position = ray.GetPoint(point);
        }
    }

    void ControlRotDiceCarrier()
    {
        if (diceSelected == null)
        {
            diceCarrier.transform.localEulerAngles = Vector3.zero;
            return;
        }

        Vector3 direction = diceCarrier.transform.position - previousPos;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Directly set the rotation to the target rotation
            diceCarrier.transform.rotation = targetRotation;
        }
    }

    void ControlDiceGet()
    {
        if (Input.GetMouseButtonDown(0) && diceSelected == null)
        {
            GrabDice();
        }

        if (Input.GetMouseButtonUp(0) && diceSelected != null)
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
                StartCoroutine(DicePosMover());
                diceSelected.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    void ReleaseDice()
    {
        diceSelected.transform.parent = null;

        StopCoroutine(DicePosMover());

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

    IEnumerator DicePosMover()
    {
        Vector3 basePos = diceSelected.transform.localPosition;
        Vector3 baseRot = diceSelected.transform.localEulerAngles;
        float t = 0;

        while (t < 1)
        {
            t += stepForGrab * Time.fixedDeltaTime;

            diceSelected.transform.localPosition = Vector3.Lerp(basePos, Vector3.zero, t);
            diceSelected.transform.localEulerAngles = Vector3.Lerp(baseRot, Vector3.zero, t);

            yield return new WaitForFixedUpdate();
        }

        diceSelected.transform.localPosition = Vector3.zero;
        diceSelected.transform.localEulerAngles = Vector3.zero;
    }
}
