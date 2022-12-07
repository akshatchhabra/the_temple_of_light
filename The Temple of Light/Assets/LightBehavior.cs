using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Angle
{

    public int angle;

    private int Mod(int x, int y)
    {
        int mod = x % y;
        return mod < 0 ? mod + y : mod;
    }

    public Angle(int angle)
    {
        this.angle = Mod(angle, 8);
    }

    public static Angle operator -(Angle angle)
    {
        return new Angle(-angle.angle);
    }

    public static Angle operator +(Angle angle, int change)
    {
        return new Angle(angle.angle + change);
    }

    public static Angle operator +(Angle angle1, Angle angle2)
    {
        return angle1 + angle2.angle;
    }

    public static Angle operator -(Angle angle, int change)
    {
        return new Angle(angle.angle - change);
    }

    public static Angle operator -(Angle angle1, Angle angle2)
    {
        return angle1 - angle2.angle;
    }

    public static bool operator ==(Angle angle1, Angle angle2)
    {
        return angle1.angle == angle2.angle;
    }

    public static bool operator !=(Angle angle1, Angle angle2)
    {
        return !(angle1 == angle2);
    }

    public override bool Equals(object other)
    {
        if (!(other is Angle))
        {
            return false;
        }
        return other as Angle == this;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static int Distance(Angle angle1, Angle angle2)
    {
        return (angle1 - angle2).angle;
    }

    public void SetAngle(int angle)
    {
        this.angle = new Angle(angle).angle;
    }

    public double ToDegrees()
    {
        return this.angle * 45D;
    }
}

public class LightBehavior : MonoBehaviour
{
    public GameObject lightObject;
    public float maxLength = 60f;
    public Angle direction = new Angle(3);
    public GameObject source;

    private Vector3 unit;
    private Vector3 endpoint;
    private GameObject obstr;
    private List<GameObject> children;
    private Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        unit = GetUnitVector(direction);
        origin = source.transform.position;
        endpoint = origin + unit * maxLength;
        obstr = null;
        children = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        LayerMask layers =~ LayerMask.GetMask("Light");
        Collider[] colliders = Physics.OverlapCapsule(origin, endpoint, 1f, layers);
        if (colliders.Length >= 1)
        {
            GameObject prevObstr = obstr;
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject currObj = colliders[i].gameObject;
                if (GameObject.ReferenceEquals(obstr, null) || Vector3.Distance(origin, obstr.transform.position) > Vector3.Distance(origin, currObj.transform.position))
                {
                    obstr = currObj;
                }
            }
            if (!GameObject.Equals(obstr, prevObstr))
            {
                transform.position = 0.5f * (origin + obstr.transform.position);
                transform.localScale = new Vector3(1, Vector3.Distance(origin, obstr.transform.position), 1);
                KillChildren();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameObject.Equals(other.gameObject, source))
        {
            return;
        }
        if (other.tag == "Column")
        {
            GameObject column = other.gameObject;

            Angle lightAngle = new Angle((int)(transform.rotation.eulerAngles.y % 360 / 45));
            Angle colAngle = column.GetComponent<Column>().facing_angle;

            ColType type = column.GetComponent<Column>().type;
            switch (type)
            {
                case ColType.MIRROR:
                    if (Angle.Distance(lightAngle, colAngle) == 1)
                    {
                        CreateLight(column, lightAngle + 2);
                    }
                    else if (Angle.Distance(lightAngle, colAngle) == 3)
                    {
                        CreateLight(column, lightAngle - 2);
                    }
                    break;
                case ColType.CONCAVE:
                    if (Angle.Distance(lightAngle, colAngle) <= 1)
                    {
                        CreateLight(column, colAngle);
                    }
                    else if (Angle.Distance(lightAngle, colAngle) >= 3)
                    {
                        CreateLight(column, -colAngle);
                    }
                    break;
                case ColType.CONVEX:
                    if (Angle.Distance(lightAngle, colAngle) <= 1)
                    {
                        CreateSplitLight(column, colAngle);
                    }
                    else if (Angle.Distance(lightAngle, colAngle) >= 3)
                    {
                        CreateSplitLight(column, -colAngle);
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

    private GameObject CreateLight(GameObject column, Angle angle)
    {
        GameObject childLight = Instantiate(lightObject);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.direction = angle;
        childBehavior.source = column;
        childLight.SetActive(true);
        children.Add(childLight);
        return childLight;
    }

    private GameObject CreateColorLight(GameObject column, Color color, Angle angle)
    {
        GameObject childLight = CreateLight(column, angle);
        Material childMaterial = childLight.GetComponent<Renderer>().material;
        childMaterial.SetColor("_Color", color);
        childMaterial.SetColor("_EmissionColor", color);
        return childLight;
    }

    private GameObject[] CreatePrismLight(GameObject column, Angle angle)
    {
        GameObject[] output = new GameObject[3];
        Color color = GetComponent<Renderer>().material.GetColor("_Color");
        if (color.r == 1)
        {
            output[0] = CreateColorLight(column, Color.red, angle - 1);
        }
        if (color.g == 1)
        {
            output[1] = CreateColorLight(column, Color.green, angle);
        }
        if (color.b == 1)
        {
            output[2] = CreateColorLight(column, Color.blue, angle + 1);
        }
        return output;
    }

    private GameObject[] CreateSplitLight(GameObject column, Angle angle)
    {
        GameObject[] output = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            output[i] = CreateLight(column, angle - 1 + i);
        }
        return output;
    }

    private bool NotSide(Angle colAngle, Angle lightAngle)
    {
        return Angle.Distance(lightAngle, colAngle) != 2;
    }

    private bool HasColor(Color color)
    {
        Color lightColor = GetComponent<Renderer>().material.GetColor("_Color");
        return lightColor.r >= color.r && lightColor.g >= color.g && lightColor.b >= color.b;
    }

    private Vector3 GetUnitVector(Angle angle)
    {
        return new Vector3 ((float)Math.Cos(angle.ToDegrees()), 0, (float)Math.Sin(angle.ToDegrees()));
    }

    private void KillChildren()
    {
        foreach (GameObject child in children)
        {
            child.GetComponent<LightBehavior>().Kill();
        }
        children = new List<GameObject>();
    }

    public void Kill()
    {
        KillChildren();
        Destroy(this);
    }
}
