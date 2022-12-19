using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightInstantiator : MonoBehaviour
{
    public GameObject lightObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject LightInstance()
    {
        return Instantiate(lightObject);
    }
}
