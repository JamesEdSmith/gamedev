using Meta.XR.MRUtilityKit;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public string hitName;
    public Transform holder;
    public Transform emitter;
    public ParticleSystem particles;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialHolderPosition = holder.position;
        laserScale = transform.localScale.x;
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(player.position, transform.position) > 2f)
        {
            particles.Stop();
        }
        else if (particles.isStopped)
        {
            particles.Play();
        }
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit, 100, ~ignoreMe))
        {
            if (hit.collider != null && hit.collider != lastHit.collider)
            {
                holder.position = Vector3.Lerp(emitter.position, hit.point, 0.5f);
                Vector3 scale = new Vector3(laserScale, laserScale, laserScale);
                scale.x = -(emitter.position - hit.point).magnitude;
                holder.localScale = new Vector3(scale.x / laserScale * 0.5f, holder.localScale.y, holder.localScale.z);

                particles.transform.position = Vector3.Lerp(spotLight.position, hit.point, 0.5f);

                particles.transform.Translate(new Vector3(0, 0, -0.3f * transform.localScale.y), Space.Self);
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.scale = new Vector3((scale / laserScale).x, shape.scale.y, shape.scale.z);

                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = 20f * Vector3.Distance(spotLight.position, hit.point);

                if (!hit.collider.name.Contains("EffectMesh") && hit.collider.tag != "laser")
                {
                    if (hit.collider.GetComponent<Picture>() == null || hit.collider.GetComponent<Picture>().grabbed)
                    {
                        spotLight.gameObject.GetComponent<Light>().color = Color.red;
                        emitterMesh.material.SetColor("_EmissionColor", Color.red);
                        beamRenderer.material.SetColor("_Color", Color.red);
                        dropZone.fail();
                        hitName = hit.collider.gameObject.name;
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
                    }
                }
                else if (hit.collider.tag == "laser")
                {
                    //do nothing
                }
                else
                {
                    reciever.position = hit.point;
                    particles.Simulate(5);
                }
                if (hit.collider != null && hit.collider.name.Contains("Picture"))
                    Debug.Log("No");
                lastHit = hit;
            }
        }
        else
        {
            hit = new RaycastHit();
        }
    }

    public void reset()
    {
        spotLight.gameObject.GetComponent<Light>().color = Color.green;
        emitterMesh.material.SetColor("_EmissionColor", Color.green);
        beamRenderer.material.SetColor("_Color", Color.green);
        particles.Simulate(5);
    }
}
