using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStoplightGeneration : MonoBehaviour
{
    // Start is called before the first frame update
    public float ChanceOfStoplight = 0.2f;
    public Transform spawnPoint;
    public GameObject stoplightRef;
    public bool enabled = false;
    void Start()
    {
        if(enabled)
        {
            // randomly generate a stop light at the signpoint
            float result = Random.Range(0, 1f);
            print(result);
            if (result < ChanceOfStoplight)
            {
                print("making stoplight");
                if (stoplightRef && spawnPoint)
                {
                    GameObject stoplightInstance = Object.Instantiate(stoplightRef, spawnPoint.position, spawnPoint.rotation, this.transform);
                    stoplightInstance.transform.localScale += new Vector3(0.0f, 50.0f, 0.0f);

                }
                else
                {
                    Debug.LogError("No stoplight prefab or spawn point specified");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
