using Meta.XR.MRUtilityKit;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public string hitName;
    public Transform holder;
    public Transform emitter;
    public ParticleSystem particles;
    public LineRenderer lineRenderer;
    public Transform spotLight;
    public Transform reciever;
    public MeshRenderer beamRenderer;
    public LayerMask ignoreMe;

    public DropZone dropZone;

    Vector3 initialHolderPosition;
    float laserScale;

    public MeshRenderer emitterMesh;

    RaycastHit lastHit;

    public PicturePlacer placer;
    public Transform player;

    public GameObject flash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialHolderPosition = holder.position;
        laserScale = transform.localScale.x;

    }

    bool lastColliderMoved;
    Vector3 lastColliderPosition;

    private void FixedUpdate()
    {
        //if (Vector3.Distance(player.position, transform.position) > 1f)
        //{
        //    //particles.Stop();
        //}
        //if (particles.isStopped)
        //{
        //    particles.Play();
        //}
        if (placer.placeRoom != null)
        {
            flash.transform.LookAt(player);

            Vector3 directionToPoint = player.position - transform.position;
            float angle = Vector3.Angle(-transform.right, directionToPoint);

            if (angle < 50)
            {
                flash.transform.localScale = new Vector3(3f, 3f, 3f) * (50f - angle) / 50f;
            }
            else
            {
                flash.transform.localScale = Vector3.zero;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.right, out hit, 100, ~ignoreMe))
            {
                if (lastHit.collider != null && lastColliderPosition != hit.point)
                {
                    lastColliderMoved = true;
                }
                else
                {
                    lastColliderMoved = false;
                }
                lineRenderer.positionCount = 7;
                if (hit.collider != null && (hit.collider != lastHit.collider || lastColliderMoved))
                {
                    holder.position = Vector3.Lerp(emitter.position, hit.point, 0.5f);
                    Vector3 scale = new Vector3(laserScale, laserScale, laserScale);
                    scale.x = -(emitter.position - hit.point).magnitude;
                    holder.localScale = new Vector3(scale.x / laserScale * 0.5f, holder.localScale.y, holder.localScale.z);

                    //ParticleSystem.ShapeModule shape = particles.shape;
                    //shape.scale = new Vector3(shape.scale.x, shape.scale.y, -(holder.localScale).x);

                    //ParticleSystem.EmissionModule emission = particles.emission;
                    //emission.rateOverTime = 100f * Vector3.Distance(spotLight.position, hit.point);


                    if (!hit.collider.name.Contains("EffectMesh") && hit.collider.tag != "laser")
                    {
                        if (hit.collider.GetComponent<Picture>() != null)
                        {
                            spotLight.gameObject.GetComponent<Light>().color = Color.red;
                            emitterMesh.material.SetColor("_EmissionColor", Color.red);
                            if (beamRenderer != null)
                                beamRenderer.material.SetColor("_BaseColor", Color.red);
                            //ParticleSystem.MainModule main = particles.main;
                            //main.startColor = Color.red;
                            lineRenderer.material.SetColor("_BaseColor", Color.red);
                            lineRenderer.material.SetColor("_EmissionColor", Color.red);
                            flash.GetComponent<SpriteRenderer>().material.SetColor("_BaseColor", Color.red);
                            flash.GetComponent<SpriteRenderer>().material.SetColor("_EmissionColor", Color.red);

                            dropZone.fail();
                            hitName = hit.collider.gameObject.name;
                            lineRenderer.SetPositions(new Vector3[]
                            { emitter.transform.position + emitter.transform.up * 0.038f,
                        emitter.transform.position + emitter.transform.up * 0.038f + (hit.point - emitter.transform.position).normalized * 0.1f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/4f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/2f,
                          emitter.transform.position + (hit.point - emitter.transform.position)*3f / 4f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0.1f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0.0f });
                        }
                        else
                        {
                            Vector3 position = new Vector3();
                            Vector3 normal = new Vector3();
                            placer.placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_DOWN, 0.75f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.CEILING), out position, out normal);
                            if (UnityEngine.Random.Range(0, 1) == 0)
                            {
                                Quaternion rotationx = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.right);
                                Quaternion rotationy = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up);
                                Quaternion rotationz = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.forward);
                                normal = rotationx * normal;
                                normal = rotationy * normal;
                                normal = rotationz * normal;
                            }
                            Physics.Raycast(position, normal, out hit, 100, ~ignoreMe);
                            int maxTries = 100;

                            while (maxTries > 0 && hit.collider != null && !hit.collider.name.Contains("EffectMesh"))
                            {
                                maxTries--;
                                placer.placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_DOWN, 0.75f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.CEILING), out position, out normal);
                                if (UnityEngine.Random.Range(0, 1) == 0)
                                {
                                    Quaternion rotationx = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.right);
                                    Quaternion rotationy = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up);
                                    Quaternion rotationz = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.forward);
                                    normal = rotationx * normal;
                                    normal = rotationy * normal;
                                    normal = rotationz * normal;
                                }
                                Physics.Raycast(position, normal, out hit, 100, ~ignoreMe);

                                if (hit.collider != null && (hit.collider.name.Contains("Cube") || hit.collider.name.Contains("Picture")))
                                    Debug.Log("Huh?");
                            }
                            transform.position = position;
                            transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0, 90, 0);
                            lineRenderer.SetPositions(new Vector3[]
                            { emitter.transform.position + emitter.transform.up * 0.038f,
                        emitter.transform.position + emitter.transform.up * 0.038f + (hit.point - emitter.transform.position).normalized * 0.1f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/4f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/2f,
                          emitter.transform.position + (hit.point - emitter.transform.position)*3f / 4f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0.1f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0 });
                        }
                    }
                    else if (hit.collider.tag == "laser")
                    {
                        //do nothing
                    }
                    else
                    {
                        lineRenderer.SetPositions(new Vector3[]
                            { emitter.transform.position + emitter.transform.up * 0.038f,
                        emitter.transform.position + emitter.transform.up * 0.038f + (hit.point - emitter.transform.position).normalized * 0.1f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/4f,
                          emitter.transform.position + (hit.point - emitter.transform.position)/2f,
                          emitter.transform.position + (hit.point - emitter.transform.position)*3f / 4f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0.1f,
                          hit.point + (emitter.transform.position - hit.point).normalized * 0.038f });
                        reciever.position = hit.point;
                    }

                    if (hit.collider != null && hit.collider.name.Contains("Picture"))
                        Debug.Log("No");
                    lastHit = hit;
                    lastColliderPosition = lastHit.point;
                }
            }
            else
            {
                hit = new RaycastHit();
            }
        }
    }

    public void reset()
    {
        //particles.Play();
        spotLight.gameObject.GetComponent<Light>().color = Color.green;
        emitterMesh.material.SetColor("_EmissionColor", Color.green);
        if (beamRenderer != null)
            beamRenderer.material.SetColor("_BaseColor", Color.green);

        //ParticleSystem.MainModule main = particles.main;
        //main.startColor = Color.green;
        lineRenderer.material.SetColor("_BaseColor", Color.green);
        lineRenderer.material.SetColor("_EmissionColor", Color.green);
        flash.GetComponent<SpriteRenderer>().material.SetColor("_BaseColor", Color.green);
        flash.GetComponent<SpriteRenderer>().material.SetColor("_EmissionColor", Color.green);
        //particles.Simulate(5);
    }
}
