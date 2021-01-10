using UnityEngine;

public class WaveFloat : MonoBehaviour
{
    public float AirDrag = 1.0f;
    public float WaterDrag = 10f;

    public Transform[] FloatPoints;

    protected Rigidbody m_rigidbody;
    protected Wave m_wave;

    protected Vector3 m_centerOffset;

    public Vector3 Center{get{return transform.position + m_centerOffset;}}

    void Awake()
    {

    }

    void Update()
    {

    }
}