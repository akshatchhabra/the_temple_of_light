using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LightBehavior : MonoBehaviour
{
    public GameObject lightObject;
    public float maxLength = 60f;
    public Vector3 origin = Vector3.zero;
    public int direction = 0;
    public GameObject source;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 unit = GetUnitVector(direction);
        Vector3 endpoint = origin + unit * maxLength;
        Collider[] colliders = Physics.OverlapCapsule(origin, endpoint, 1f);
        if (colliders.Length >= 1)
        {
            GameObject obstr = colliders[0].gameObject;
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject currObj = colliders[i].gameObject;
                if (currObj != source && Vector3.Distance(origin, obstr.transform.position) > Vector3.Distance(origin, currObj.transform.position))
                {
                    obstr = currObj;
                }
            }
            transform.position = 0.5f * (origin + obstr.transform.position);
            transform.localScale = new Vector3(1, Vector3.Distance(origin, obstr.transform.position), 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == this.transform.parent)
        {
            return;
        }
        if (other.tag == "Column")
        {
            GameObject column = other.gameObject;

            int lightAngle = (int)(transform.rotation.eulerAngles.y % 360 / 45);
            int colAngle = column.GetComponent<Column>().facing_angle;

            ColType type = column.GetComponent<Column>().type;
            switch (type)
            {
                case ColType.MIRROR:
                    if (lightAngle == (colAngle + 1) % 8 || lightAngle == (colAngle - 1) % 8)
                    {
                        CreateLight(column, (lightAngle + 2) % 8);
                    }
                    else if (lightAngle == (colAngle + 3) % 8 || lightAngle == (colAngle - 3) % 8)
                    {
                        CreateLight(column, (lightAngle - 2) % 8);
                    }
                    break;
                case ColType.CONCAVE:
                    if (NotSide(colAngle, lightAngle))
                    {
                        CreateLight(column, colAngle);
                    }
                    break;
                case ColType.CONVEX:
                    if (NotSide(colAngle, lightAngle))
                    {
                        CreateSplitLight(column, colAngle);
                    }
                    break;
                case ColType.ONEWAY:
                    if (lightAngle == colAngle)
                    {
                        CreateLight(column, lightAngle);
                    }
                    break;
                case ColType.COLOR:
                    Color colColor = column.GetComponent<Renderer>().material.GetColor("_Color");
                    if (HasColor(colColor) && NotSide(colAngle, lightAngle))
                    {
                        CreateColorLight(column, colColor, lightAngle);
                    }
                    break;
                case ColType.PRISM:
                    if (NotSide(colAngle, lightAngle))
                    {
                        CreatePrismLight(column, lightAngle);
                    }
                    break;
                default:
                    break;
            }
        }

        if (other.tag == "Monster")
        {
            //TODO
        }
    }

    private GameObject CreateLight(GameObject column, int angle)
    {
        GameObject childLight = Instantiate(lightObject);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.origin = column.transform.position;
        childBehavior.direction = angle;
        childBehavior.source = column;
        return childLight;
    }

    private GameObject CreateColorLight(GameObject column, Color color, int angle)
    {
        GameObject childLight = CreateLight(column, angle);
        Material childMaterial = childLight.GetComponent<Renderer>().material;
        childMaterial.SetColor("_Color", color);
        childMaterial.SetColor("_EmissionColor", color);
        return childLight;
    }

    private GameObject[] CreatePrismLight(GameObject column, int angle)
    {
        GameObject[] output = new GameObject[3];
        Color color = GetComponent<Renderer>().material.GetColor("_Color");
        if (color.r == 1)
        {
            output[0] = CreateColorLight(column, Color.red, (angle - 1) % 8);
        }
        if (color.g == 1)
        {
            output[1] = CreateColorLight(column, Color.green, angle);
        }
        if (color.b == 1)
        {
            output[2] = CreateColorLight(column, Color.blue, (angle + 1) % 8);
        }
        return output;
    }

    private GameObject[] CreateSplitLight(GameObject column, int angle)
    {
        GameObject[] output = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            output[i] = CreateLight(column, (angle - 1 + i) % 8);
        }
        return output;
    }

    private bool NotSide(int colAngle, int lightAngle)
    {
        return lightAngle != (colAngle + 2) % 8 && lightAngle != (colAngle - 2) % 8;
    }

    private bool HasColor(Color color)
    {
        Color lightColor = GetComponent<Renderer>().material.GetColor("_Color");
        return lightColor.r >= color.r && lightColor.g >= color.g && lightColor.b >= color.b;
    }

    private Vector3 GetUnitVector(int angle)
    {
        return new Vector3 ((float)Math.Cos(angle * 45D), 0, (float)Math.Sin(angle * 45D));
    }
}
