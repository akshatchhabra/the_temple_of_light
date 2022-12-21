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

    public Angle(Quaternion rotation)
    {
        this.angle = Mod((int)(rotation.eulerAngles.y / 45f), 8);
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

    public static int Distance(Angle angle1, Angle angle2) // positive distance from angle2 to angle1
    {
        return (angle1 - angle2).angle;
    }

    public void SetAngle(int angle)
    {
        this.angle = new Angle(angle).angle;
    }

    public float ToDegrees()
    {
        return this.angle * 45f;
    }

    public double ToRadians()
    {
        return Math.PI * ToDegrees() / 180D;
    }

    public override string ToString()
    {
        return this.angle.ToString();
    }
}

public class LightBehavior : MonoBehaviour
{
    public float maxLength = 60f;
    public Angle direction = new Angle(0);
    public GameObject source;
    public LightBehavior parent = null;
    public int luminosity = 10;
    public float lightHeight = 3.3f;

    private Vector3 unit;
    private Vector3 endpoint;
    private GameObject obstr;
    private List<LightBehavior> children;
    private Vector3 origin;
    private Vector3 lightEnd;
    private Color lightColor;

    // Start is called before the first frame update
    void Start()
    {
        origin = this.transform.position;
        unit = GetUnitVector(direction);
        origin = source.transform.position;
        origin.y = lightHeight;
        endpoint = origin + unit * maxLength;
        Debug.Log(endpoint);
        obstr = null;
        children = new List<LightBehavior>();
        lightColor = GetComponent<Renderer>().material.GetColor("_Color");
        transform.position = origin;
        transform.Rotate(-direction.ToDegrees(), 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        obstr = null;
        LayerMask layers =~ LayerMask.GetMask("Light");
        Collider[] colliders = Physics.OverlapCapsule(origin, endpoint, 0.25f, layers);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject currObj = colliders[i].gameObject;
            if (Vector3.Distance(origin, currObj.transform.position) > 1 &&
                (GameObject.ReferenceEquals(obstr, null) || 
                    Vector3.Distance(origin, obstr.transform.position) > Vector3.Distance(origin, currObj.transform.position)))
            {
                obstr = currObj;
            }
        }
        if (!GameObject.ReferenceEquals(obstr, null))
        {
            lightEnd = LinePoint(obstr.transform.position);
        }
        else
        {
            lightEnd = endpoint;
        }
        transform.position = 0.5f * (origin + lightEnd);
        transform.localScale = 0.5f * new Vector3(1, Vector3.Distance(origin, lightEnd), 1);
    }

    public void ManualEnter(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameObject.Equals(other.gameObject, source) || other.gameObject.name == "light")
        {
            return;
        }
        if (other.tag == "Column")
        {
            Debug.Log("Hit a column!");
            GameObject column = other.gameObject;

            Angle colAngle = column.GetComponent<Column>().facing_angle;
            column.GetComponent<Column>().is_lit = true;
            column.GetComponent<Column>().lit_color = lightColor;

            ColType type = column.GetComponent<Column>().type;
            int distance = Angle.Distance(direction, colAngle);
            switch (type)
            {
                case ColType.MIRROR:
                    switch (distance)
                    {
                        case 1:
                        case 5:
                            CreateLight(column, direction + 2);
                            break;
                        case 3:
                        case 7:
                            CreateLight(column, direction - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case ColType.CONCAVE:
                    switch (distance)
                    {
                        case 2:
                        case 6:
                            break;
                        default:
                            CreateSplitLight(column, direction);
                            break;
                    }
                    break;
                case ColType.CONVEX:
                    switch (distance)
                    {
                        case 0:
                        case 1:
                        case 7:
                            CreateLight(column, colAngle);
                            break;
                        case 3:
                        case 4:
                        case 5:
                            CreateLight(column, -colAngle);
                            break;
                        default:
                            break;
                    }
                    break;
                case ColType.ONEWAY:
                    switch (distance)
                    {
                        case 0:
                        case 1:
                        case 7:
                            CreateLight(column, direction);
                            break;
                        case 3:
                            CreateLight(column, direction - 2);
                            break;
                        case 5:
                            CreateLight(column, direction + 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case ColType.COLOR:
                    switch (distance)
                    {
                        case 2:
                        case 6:
                            break;
                        default:
                            Color colColor = column.GetComponent<Column>().color;
                            if (HasColor(colColor))
                            {
                                CreateColorLight(column, colColor, direction);
                            }
                            break;
                    }
                    break;
                case ColType.PRISM:
                    switch (distance)
                    {
                        case 2:
                        case 6:
                            break;
                        default:
                            CreatePrismLight(column, direction);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        if (other.tag == "Monster")
        {
            Bat bat = other.GetComponent<Bat>();
            bat.is_dead = true;
            bat.time_of_death = Time.time;
            Animator bat_animator = other.GetComponent<Animator>();
            bat_animator.SetBool("is_dead", true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Column")
        {
            KillChildren();
        }
    }

    private GameObject CreateColorLight(GameObject column, Color color, Angle angle)
    {
        if (luminosity <= 0)
        {
            return null;
        }
        GameObject childLight = Instantiate(column.GetComponent<Column>().lightObject);
        Material childMaterial = childLight.GetComponent<Renderer>().material;
        Color colorA = new Color(color.r, color.g, color.b, lightColor.a);
        childMaterial.SetColor("_Color", colorA);
        childMaterial.SetColor("_EmissionColor", colorA);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.direction = angle;
        childBehavior.source = column;
        childBehavior.parent = this;
        childBehavior.luminosity = luminosity - 1;
        childLight.SetActive(true);
        children.Add(childBehavior);
        return childLight;
    }

    private GameObject CreateLight(GameObject column, Angle angle)
    {
        GameObject childLight = CreateColorLight(column, lightColor, angle);
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

    private bool HasColor(Color color)
    {
        return lightColor.r >= color.r && lightColor.g >= color.g && lightColor.b >= color.b;
    }

    private Vector3 GetUnitVector(Angle angle)
    {
        return new Vector3((float)Math.Cos(angle.ToRadians()), 0f, (float)Math.Sin(angle.ToRadians()));
    }

    public void KillChildren()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            children[i].Kill();
        }
        children = new List<LightBehavior>();
    }

    public void Kill()
    {
        KillChildren();
        if (!GameObject.ReferenceEquals(parent, null))
        {
            parent.RemoveChild(this);
        }
        Destroy(gameObject);
    }

    public void RemoveChild(LightBehavior child)
    {
        children.Remove(child);
    }

    private Vector3 LinePoint(Vector3 target)
    {
        switch (direction.angle)
        {
            case 0:
            case 4:
                return new Vector3(target.x, lightHeight, origin.z);
            case 2:
            case 6:
                return new Vector3(origin.x, lightHeight, target.z);
            case 1: // +, +
            {
                float t = 0.5f * (target.x - origin.x + target.z - origin.z);
                return new Vector3(origin.x + t, lightHeight, origin.z + t);
            }
            case 3: // -, +
            {
                float t = 0.5f * (-target.x + origin.x + target.z - origin.z);
                return new Vector3(origin.x - t, lightHeight, origin.z + t);
            }
            case 5: // -, -
            {
                float t = 0.5f * (-target.x + origin.x - target.z + origin.z);
                return new Vector3(origin.x - t, lightHeight, origin.z - t);
            }
            case 7: // +, -
            {
                float t = 0.5f * (target.x - origin.x - target.z + origin.z);
                return new Vector3(origin.x + t, lightHeight, origin.z - t);
            }
            default:
                return Vector3.zero;
        }
    }
}
