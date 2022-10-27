using UnityEngine;
using UnityEngine.UI;

public class DinosaurWalk : MonoBehaviour
{
    [SerializeField] bool isWalking = false;        //The trick is that this variable is tracked in dinosaur animator


    [SerializeField] GameObject dinosaur;
    [SerializeField] Text rewindIndication;
    void FixedUpdate()
    {
        if(isWalking)       //Simple walking around
        {
            dinosaur.transform.position += transform.forward*0.04f;
            dinosaur.transform.Rotate(Vector3.up,0.5f);
        }

        if(Input.GetKey(KeyCode.Space))                 //Didnt want to clutter RewindByKeyPress.cs with this "unnecessaryness" and didnt want to add another script just for this. So it is here (i know, it is ugly)
        {
            rewindIndication.text = "Currently rewinding!!!";
            rewindIndication.color = Color.green;
        }
        else
        {
            rewindIndication.text = "Press Spacebar to rewind";
            rewindIndication.color = Color.black;
        }
    }
}
