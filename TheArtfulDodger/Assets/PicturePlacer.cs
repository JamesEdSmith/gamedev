using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PicturePlacer : MonoBehaviour
{
    public int numbPictures;
    public int numbLasers;
    public GameObject debugCube;
    public GameObject picture;
    public List<GameObject> pictures;
    public GameObject laser;
    List<GameObject> lasers;
    public GameObject bin;
    public OVRInput.Button placerButton;

    public MRUKRoom placeRoom;
    public List<MRUKAnchor> realPaintings;
    public GameObject cube;
    public List<Material> mats;
    public Material debugNegativeMat;

    public Transform player;
    public TextMeshPro laserCounter;
    public WatchMenu watch;

    private void Start()
    {
        pictures = new List<GameObject>();

        for (int i = 0; i < numbPictures; i++)
        {
            pictures.Add(Instantiate(picture));
            pictures[i].GetComponent<Picture>().index = i;
        }

        lasers = new List<GameObject>();
        for (int i = 0; i < numbLasers; i++)
        {
            GameObject newLaser = Instantiate(laser);
            newLaser.GetComponent<Laser>().dropZone = bin.GetComponentInChildren<DropZone>();
            lasers.Add(newLaser);
            newLaser.GetComponent<Laser>().placer = this;
            newLaser.GetComponent<Laser>().player = player;
            newLaser.GetComponentInChildren<LineHolder>().targetPoint = player;
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(placerButton))
        {
            //cube.SetActive(!cube.activeSelf);
            reset();
        }
    }

    public void increaseLasers()
    {
        numbLasers++;
        laserCounter.text = "Lasers: " + numbLasers;
    }

    public void decreaseLasers()
    {
        numbLasers--;
        if (numbLasers < 0)
        {
            numbLasers = 0;
        }
        laserCounter.text = "Lasers: " + numbLasers;
    }

    public void reset()
    {
        if (placeRoom != null)
        {
            movePaintings();
            moveBin();
            

            while (lasers.Count < numbLasers)
            {
                GameObject newLaser = Instantiate(laser);
                newLaser.GetComponent<Laser>().dropZone = bin.GetComponentInChildren<DropZone>();
                lasers.Add(newLaser);
                newLaser.GetComponent<Laser>().placer = this;
                newLaser.GetComponent<Laser>().player = player;
            }

            moveLasers();

            foreach (GameObject laser in lasers)
            {
                if (lasers.IndexOf(laser) >= numbLasers)
                {
                    laser.SetActive(false);
                }
                else
                {
                    laser.SetActive(true);
                }
            }
        }
    }

    public void placePicture()
    {
        placeRoom = MRUK.Instance.GetCurrentRoom();
        List<MRUKAnchor> allAnchors = placeRoom.Anchors;
        realPaintings = allAnchors.FindAll(x => x.Label == MRUKAnchor.SceneLabels.WALL_ART);

        //foreach (MRUKAnchor anchor in realPaintings)
        //{
        //    Instantiate(debugCube, anchor.transform.position, anchor.transform.rotation).transform.Translate(0.25f, 0.25f, 0);
        //    Instantiate(debugCube, anchor.transform.position, anchor.transform.rotation).transform.Translate(-0.25f, 0.25f, 0);
        //    Instantiate(debugCube, anchor.transform.position, anchor.transform.rotation).transform.Translate(0.25f, -0.25f, 0);
        //    Instantiate(debugCube, anchor.transform.position, anchor.transform.rotation).transform.Translate(-0.25f, -0.25f, 0);
        //}


        //int width = (int)placeRoom.FloorAnchor.PlaneRect.Value.width;
        //int height = (int)placeRoom.FloorAnchor.PlaneRect.Value.height;
        //int depth = (int)placeRoom.WallAnchors[0].PlaneRect.Value.height;

        //GameObject cube;
        //for (int i = -width/2; i <= width/2; i++)
        //{
        //    for (int j = -height / 2; j <= height / 2; j++)
        //    {
        //        for (int k = -depth / 2; k <= depth / 2; k++)
        //        {
        //            if ((i != 0 || width % 2 == 1) && (j != 0 || height % 2 == 1) && (k != 0 || depth % 2 == 1))
        //            {
        //                float iAdjust = i > 0 ? -0.5f : 0.5f;
        //                float jAdjust = j > 0 ? -0.5f : 0.5f;
        //                float kAdjust = k > 0 ? -0.5f : 0.5f;
        //                cube = Instantiate(debugCube, new Vector3(width % 2 == 1 ? i : i + iAdjust, (depth % 2 == 1 ? k : k + kAdjust) + depth/2f, height % 2 == 1 ? j : j + jAdjust), placeRoom.FloorAnchor.transform.rotation);
        //                cube.transform.position += placeRoom.FloorAnchor.transform.position;
        //            }
        //        }
        //    }
        //}

        movePaintings();
        moveBin();
        moveLasers();
    }

    public void moveBin()
    {
        Vector3 targetPosition;

        if (placeRoom.FloorAnchor != null)
        {
            MRUKAnchor floor = placeRoom.FloorAnchor;
            List<MRUKAnchor> furnature = placeRoom.Anchors.FindAll(x => x.Label != MRUKAnchor.SceneLabels.WALL_FACE &&
            x.Label != MRUKAnchor.SceneLabels.CEILING &&
            x.Label != MRUKAnchor.SceneLabels.DOOR_FRAME &&
            x.Label != MRUKAnchor.SceneLabels.GLOBAL_MESH &&
            x.Label != MRUKAnchor.SceneLabels.FLOOR &&
            x.Label != MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE &&
            x.Label != MRUKAnchor.SceneLabels.WALL_ART &&
            x.Label != MRUKAnchor.SceneLabels.WINDOW_FRAME);

            List<Vector3> spots = new List<Vector3>();


            for (float i = floor.PlaneRect.Value.xMin + 0.50f; i < floor.PlaneRect.Value.xMax - 0.50f; i += 0.25f)
            {
                for (float j = floor.PlaneRect.Value.yMin + 0.50f; j < floor.PlaneRect.Value.yMax - 0.5f; j += 0.25f)
                {

                    bool found = false;

                    foreach (MRUKAnchor anchor in furnature)
                    {
                        bin.transform.position = floor.transform.position + floor.transform.right * i + floor.transform.up * j;
                        targetPosition = new Vector3(player.position.x, bin.transform.position.y, player.transform.position.z);
                        bin.transform.rotation = Quaternion.LookRotation(targetPosition - bin.transform.position);

                        if (anchor.GetDistanceToSurface(bin.GetComponentInChildren<BoxCollider>().ClosestPoint(anchor.transform.position)) < 0.2f)
                        {
                            found = true;
                        }
                    }
                    float dist = Vector3.Distance(player.transform.position, floor.transform.position + floor.transform.right * i + floor.transform.up * j);

                    if (!isPositionInRoom(floor.transform.position + floor.transform.right * i + floor.transform.up * j))
                    {
                        found = true;
                    }

                    if (!found && dist < 2.5f)
                    {
                        spots.Add(floor.transform.position + floor.transform.right * i + floor.transform.up * j);
                    }
                    else if (found)
                    {
                        //GameObject cube = Instantiate(debugCube, floor.transform.position + floor.transform.right * i + floor.transform.up * j, floor.transform.rotation);
                        //cube.GetComponent<MeshRenderer>().material = debugNegativeMat;
                    }
                    else
                    {
                        //GameObject cube = Instantiate(debugCube, floor.transform.position + floor.transform.right * i + floor.transform.up * j, floor.transform.rotation);
                    }

                }
            }
            if (spots.Count == 0)
            {
                bin.transform.position = player.transform.position + player.transform.forward;
            }
            else
            {
                int pick = UnityEngine.Random.Range(0, spots.Count);
                bin.transform.position = spots[pick];
            }

        }
        else
        {
            bin.transform.position = new Vector3(player.position.x - 0.5f, placeRoom.FloorAnchor.transform.position.y + 0.25f, player.position.z + 1f);
        }

        targetPosition = new Vector3(player.position.x, bin.transform.position.y, player.transform.position.z);
        bin.transform.rotation = Quaternion.LookRotation(targetPosition - bin.transform.position);

        bin.GetComponentInChildren<DropZone>(true).reset();
        //firstPicture.position = bin.transform.position;
    }

    private bool isPositionInRoom(Vector3 vector3)
    {
        foreach (MRUKAnchor wall in placeRoom.WallAnchors)
        {
            Vector3 pos = new Vector3();
            wall.GetClosestSurfacePosition(vector3, out pos);
            if (Quaternion.Angle(wall.transform.rotation, Quaternion.LookRotation(vector3 - wall.transform.position)) > 90
                && Vector3.Distance(wall.transform.position, pos) < wall.PlaneRect.Value.width / 2.1f)
            {
                return false;
            }
        }
        return true;
    }

    Transform firstPicture;
    List<Material> targetMats;

    public void movePaintings()
    {
        List<Material> pictureMats = new List<Material>(mats);
        List<MRUKAnchor> walls = placeRoom.Anchors.FindAll(x => x.Label == MRUKAnchor.SceneLabels.WALL_FACE);
        List<Vector3> spots = new List<Vector3>();
        List<Quaternion> rots = new List<Quaternion>();

        foreach (MRUKAnchor wall in walls)
        {
            pictures[0].transform.rotation = wall.transform.rotation;
            for (float i = wall.PlaneRect.Value.xMin + 0.25f; i < wall.PlaneRect.Value.xMax - 0.25f; i += 0.25f)
            {
                for (float j = wall.PlaneRect.Value.yMin + 0.75f; j < wall.PlaneRect.Value.yMax - 1f; j += 0.25f)
                {
                    pictures[0].GetComponent<Rigidbody>().position = wall.transform.position + wall.transform.right * i + wall.transform.up * j;
                    Bounds colliderBounds = picture.GetComponent<BoxCollider>().bounds;
                    if (!pictureClose(pictures[0], 0.3f))
                    {
                        spots.Add(wall.transform.position + wall.transform.right * i + wall.transform.up * j + wall.transform.forward * 0.06f);
                        rots.Add(wall.transform.rotation);
                        //GameObject cube = Instantiate(debugCube, wall.transform.position + wall.transform.right * i + wall.transform.up * j, wall.transform.rotation);
                        //float dist = realPaintings[0].GetDistanceToSurface(colliderBounds.ClosestPoint(realPaintings[0].transform.position));
                        //cube.GetComponentInChildren<TextMeshPro>().text = dist.ToString("0.00");
                    }
                    else
                    {
                        //Instantiate(debugCube, wall.transform.position + wall.transform.right * i + wall.transform.up * j, wall.transform.rotation).GetComponent<MeshRenderer>().material = debugNegativeMat;
                    }
                }
            }
        }
        List<Material> usedPictureMats = new List<Material>();

        foreach (GameObject picture in pictures)
        {
            picture.GetComponentInChildren<Grabbable>().enabled = false;
            picture.transform.GetComponent<Rigidbody>().isKinematic = true;

            int pick = UnityEngine.Random.Range(0, spots.Count);
            picture.GetComponent<Rigidbody>().position = spots[pick];
            picture.GetComponent<Rigidbody>().rotation = rots[pick];

            List<Vector3> indices = new List<Vector3>();

            foreach (Vector3 spot in spots)
            {
                if (Vector3.Distance(spot, spots[pick]) <= 0.75f)
                {
                    indices.Add(spot);
                }
            }

            foreach (Vector3 i in indices)
            {
                rots.RemoveAt(spots.IndexOf(i));
                spots.Remove(i);
            }

            Material mat = pictureMats[UnityEngine.Random.Range(0, pictureMats.Count)];
            usedPictureMats.Add(mat);
            pictureMats.Remove(mat);

            float width = mat.GetTexture("_BaseMap").width;
            float height = mat.GetTexture("_BaseMap").height;

            picture.transform.localScale = new Vector3(width / (width + height), height / (width + height), picture.transform.localScale.z);
            picture.GetComponent<Grabbable>().enabled = true;
            List<Material> paintingMat = new List<Material> { mat };
            picture.GetComponent<Picture>().SetMaterials(paintingMat);
            picture.GetComponent<Picture>().reset();
        }
        targetMats = new List<Material>();
        for (int i = 0; i < 3; i++)
        {
            int picked = UnityEngine.Random.Range(0, usedPictureMats.Count);
            targetMats.Add(usedPictureMats[picked]);
            usedPictureMats.RemoveAt(picked);
        }

        watch.setImages(targetMats);

        //firstPicture = pictures.Find(predicate).transform;
        //Debug.Log("Cool");
        //foreach (Vector3 spot in spots)
        //{
        //    Instantiate(debugCube, spot, Quaternion.identity);
        //}
    }

    private bool predicate(GameObject obj)
    {
        Picture picture = obj.GetComponent<Picture>();
        Material mat = picture.paintingRenderer.material;
        return mat.GetTexture("_BaseMap") == targetMats[0].GetTexture("_BaseMap");
    }

    private bool pictureClose(GameObject picture, float dist)
    {
        int index = pictures.IndexOf(picture);

        Bounds colliderBounds = picture.GetComponent<BoxCollider>().bounds;

        foreach (MRUKAnchor anchor in realPaintings)
        {
            if (anchor.GetDistanceToSurface(colliderBounds.ClosestPoint(anchor.transform.position)) < dist)
            {
                return true;
            }
        }
        return false;
    }

    public void moveLasers()
    {
        foreach (GameObject laser in lasers)
        {
            RaycastHit hit;
            LayerMask ignoreMe = laser.GetComponent<Laser>().ignoreMe;
            Vector3 position = new Vector3();
            Vector3 normal = new Vector3();
            placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_DOWN, 0.75f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.CEILING), out position, out normal);
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

            //if (hit.collider != null && (hit.collider.name.Contains("Cube") || hit.collider.name.Contains("Picture")))
            //    Debug.Log("Huh?");

            while (maxTries > 0 && hit.collider != null && !hit.collider.name.Contains("EffectMesh"))
            {
                maxTries--;
                placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_DOWN, 0.75f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.CEILING), out position, out normal);
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
            }
            //if (hit.collider != null && (hit.collider.name.Contains("Cube") || hit.collider.name.Contains("Picture")))
            //    Debug.Log("Huh?");

            laser.transform.position = position;
            laser.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0, 90, 0);
            laser.GetComponent<Laser>().reset();
            laser.SetActive(true);
        }
    }

}
