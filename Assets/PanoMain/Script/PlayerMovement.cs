using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
//using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {

    private float rotationRate = 0.2f;
    private const int numberOfTouches=1;
   


    private void Start()
    {
        Input.gyro.enabled = true; 
    }
    void Update()
    {

        if (Input.touchCount == numberOfTouches)
        {


            // get the user touch inpun
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                   //Debug.Log("Touch phase began at: " + touch.position);
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    //Debug.Log("Touch phase Moved");
                    transform.Rotate(0 /* set No rotation in X axis */, - touch.deltaPosition.x * rotationRate, 0/* set No rotation in Z axis */, Space.World);
                }
            }
        }

        else
        {
            if (SystemInfo.supportsGyroscope)
            {
                //You should also be able do it in one line if you want:
                transform.Rotate(0/* set No rotation in X axis */, Input.gyro.rotationRateUnbiased.y * Time.deltaTime * Mathf.Rad2Deg, 0 /* set No rotation in Z axis */ , Space.World);
            }

        }
    }

   

}


