using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyFuntionTest : MonoBehaviour
{
    public GameObject StartObj;
    public GameObject EndObj;
    public float LEARPDuration = 0;

    private Vector3 startPos;
    private Vector3 endPos;
    private float accumatedTimeSinceLERPStart;

    public EasingFunctions.easingFunctionList currentEasingFunc = EasingFunctions.easingFunctionList.Quadratic_InOut;
    // Start is called before the first frame update
    void Start()
    {
        if(StartObj != null)
        {
            startPos = StartObj.transform.position;
        }

        if (EndObj != null)
        {
            endPos = EndObj.transform.position;
        }
        accumatedTimeSinceLERPStart = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        accumatedTimeSinceLERPStart += Time.deltaTime;

        if (accumatedTimeSinceLERPStart < LEARPDuration)
        {
            // Step 1, pass the accumulated time to TimeFuntion to get the correct T value
            float t = EasingFunctions.TimeFunction(accumatedTimeSinceLERPStart, LEARPDuration);

            // Step 2, feet the t value to the chosen Easing Funtcion 
            switch(currentEasingFunc)
            {
                case EasingFunctions.easingFunctionList.Quadratic_In:
                    t = EasingFunctions.Quadratic.In(t);
                    break;

                case EasingFunctions.easingFunctionList.Quadratic_Out:
                    t = EasingFunctions.Quadratic.Out(t);
                    break;

                case EasingFunctions.easingFunctionList.Quadratic_InOut:
                    t = EasingFunctions.Quadratic.InOut(t);
                    break;

                case EasingFunctions.easingFunctionList.Bounce_In:
                    t = EasingFunctions.Quadratic.In(t);
                    break;

                case EasingFunctions.easingFunctionList.Bounce_Out:
                    t = EasingFunctions.Quadratic.Out(t);
                    break;

                case EasingFunctions.easingFunctionList.Bounce_InOut:
                    t = EasingFunctions.Quadratic.InOut(t);
                    break;

                case EasingFunctions.easingFunctionList.Elastic_In:
                    t = EasingFunctions.Quadratic.In(t);
                    break;

                case EasingFunctions.easingFunctionList.Elastic_Out:
                    t = EasingFunctions.Quadratic.Out(t);
                    break;

                case EasingFunctions.easingFunctionList.Elastic_InOut:
                    t = EasingFunctions.Quadratic.InOut(t);
                    break;
            }

            // Step 3, Use that T value to LERP and get the new postion
            Vector3 newPostion = Vector3.Lerp(startPos, endPos, t);
            // Step 4, override the existing position with the new position from the LERP
            transform.position = newPostion;
        }
    }
}
