using UnityEngine;

public class WaveFloat : MonoBehaviour
{
    public float AirDrag = 1.0f;
    public float WaterDrag = 6f;
    public float GravityCo = 1f;

    public float Y_AMP = 1;

    public Transform[] FloatPoints;

    protected Rigidbody m_rigidbody;
    protected Wave m_wave;

    protected float m_waterLine;
    protected Vector3[] m_waterLinePoints;

    protected Vector3 m_smoothVectorRotation;
    protected Vector3 m_targetUp;
    protected Vector3 m_centerOffset;
    public Vector3 Center { get { return transform.position + m_centerOffset; } }


    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.useGravity = false;
        m_wave = FindObjectOfType<Wave>();

        m_waterLinePoints = new Vector3[FloatPoints.Length];
        for(int i = 0; i < FloatPoints.Length; i++)
        {
            m_waterLinePoints[i] = FloatPoints[i].position;
        }
        m_centerOffset = PhysicsHelper.GetCenter(m_waterLinePoints) - transform.position;
    }

    void Update()
    {
        float newWaterLine = 0f;
        bool pointUnderWater = false;
        for(int i = 0; i <FloatPoints.Length; i++)
        {
            m_waterLinePoints[i] = FloatPoints[i].position;
            var waterPos = m_wave.GetWavePos(FloatPoints[i].position, Y_AMP);
            m_waterLinePoints[i].y = waterPos.y;
            newWaterLine += waterPos.y / FloatPoints.Length;
            if (waterPos.y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        float waterLineDelta = newWaterLine - m_waterLine;
        m_waterLine = newWaterLine;

        m_targetUp = PhysicsHelper.GetNormal(m_waterLinePoints);

        var gravity = Physics.gravity;
        m_rigidbody.drag = AirDrag;
        if(m_waterLine > Center.y)
        {
            m_rigidbody.drag = WaterDrag;
            gravity = -Physics.gravity;
            transform.Translate(Vector3.up * waterLineDelta);
        }
        m_rigidbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(m_waterLine - Center.y) * GravityCo, 0, 1));// Mathf.SmoothStep(0, 1, Mathf.Abs(m_waterLine - Center.y) * GravityCo));

        if (pointUnderWater)
        {
            Vector3 smoothVectorRotation = Vector3.zero;
            m_targetUp = Vector3.SmoothDamp(transform.up, m_targetUp, ref smoothVectorRotation, 0.02f);
            m_rigidbody.rotation = Quaternion.FromToRotation(transform.up, m_targetUp) * m_rigidbody.rotation;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;

            if (m_wave != null)
            {

                //draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(m_waterLinePoints[i], Vector3.one * 0.2f);
            }

            //draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);

        }

        //draw center
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, m_waterLine, Center.z), Vector3.one * 0.2f);
            Gizmos.DrawRay(new Vector3(Center.x, m_waterLine, Center.z), m_targetUp * 1f);
        }
    }
}