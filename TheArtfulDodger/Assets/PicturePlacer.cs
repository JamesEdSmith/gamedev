using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicturePlacer : MonoBehaviour
{
    public int numbPictures;
    public int numbLasers;
    public GameObject picture;
    List<GameObject> pictures;
    public GameObject laser;
    List<GameObject> lasers;
    public GameObject bin;
    public OVRInput.Button placerButton;

    public MRUKRoom placeRoom;
    public List<MRUKAnchor> realPaintings;
    public GameObject cube;
    public List<Material> mats;

    public Transform player;

    private void Start()
    {
        pictures = new List<GameObject>();

        for (int i = 0; i < numbPictures; i++)
        {
            pictures.Add(Instantiate(picture));
        }

        lasers = new List<GameObject>();
        for (int i = 0; i < numbLasers; i++)
        {
            GameObject newLaser = Instantiate(laser);
            newLaser.GetComponent<Laser>().dropZone = bin.GetComponentInChildren<DropZone>();
            lasers.Add(newLaser);
            newLaser.GetComponent<Laser>().placer = this;
            newLaser.GetComponent<Laser>().player = player;
        }
    }


    private void Update()
    {
        if (OVRInput.GetDown(placerButton))
        {
            //cube.SetActive(!cube.activeSelf);
            if (placeRoom != null)
            {
                movePaintings();
                moveBin();
                moveLasers();
            }
        }
    }

    public void placePicture()
    {
        placeRoom = MRUK.Instance.GetCurrentRoom();
        List<MRUKAnchor> allAnchors = placeRoom.Anchors;
        realPaintings = allAnchors.FindAll(x => x.Label == MRUKAnchor.SceneLabels.WALL_ART);

        movePaintings();
        moveBin();
        moveLasers();
    }

    public void moveBin()
    {
        bin.transform.position = new Vector3(player.position.x - 0.5f, placeRoom.FloorAnchor.transform.position.y + 0.25f, player.position.z + 1f);
        Vector3 targetPosition = new Vector3(player.position.x, bin.transform.position.y, player.transform.position.z);
        bin.transform.rotation = Quaternion.LookRotation(targetPosition - bin.transform.position);
        //Vector3 position = new Vector3();
        //Vector3 normal = new Vector3();
        //placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 0.75f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE), out position, out normal);
        //bin.transform.position = new Vector3(position.x, placeRoom.FloorAnchor.transform.position.y + 0.5f, position.z - 0.2f);
        //bin.transform.rotation = Quaternion.LookRotation(normal);
        //int maxTries = 40;
        //while (maxTries > 0 && pictureClose(picture, 1f))
        //{
        //    maxTries--;
        //    placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 1f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE), out position, out normal);
        //    bin.transform.position = new Vector3(position.x, placeRoom.FloorAnchor.transform.position.y + 0.5f, position.z - 0.2f);
        //    bin.transform.rotation = Quaternion.LookRotation(normal);
        //}
        bin.GetComponentInChildren<DropZone>(true).reset();
    }

    public void movePaintings()
    {
        List<Material> pictureMats = new List<Material>(mats);

        foreach (GameObject picture in pictures)
        {
            picture.GetComponent<Grabbable>().enabled = false;
            picture.transform.GetComponent<Rigidbody>().isKinematic = true;
            Vector3 position = new Vector3();
            Vector3 normal = new Vector3();

            placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 1f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE), out position, out normal);

            picture.transform.position = position;
            picture.transform.rotation = Quaternion.LookRotation(normal);
            picture.transform.Translate(0, 0, 0.1f);
            int maxTries = 40;

            while (maxTries > 0 && pictureClose(picture, 2f))
            {
                maxTries--;
                placeRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 1f, new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE), out position, out normal);
                picture.transform.position = position;
                picture.transform.rotation = Quaternion.LookRotation(normal);
                picture.transform.Translate(0, 0, 0.1f);
            }

            Material mat = pictureMats[UnityEngine.Random.Range(0, pictureMats.Count)];
            pictureMats.Remove(mat);
            List<Material> paintingMat = new List<Material> { mat };
            picture.GetComponent<MeshRenderer>().SetMaterials(paintingMat);

            float width = mat.GetTexture("_MainTex").width;
            float height = mat.GetTexture("_MainTex").height;

            //picture.transform.localScale = new Vector3(width / (width + height), height / (width + height), picture.transform.localScale.z);
            picture.GetComponent<Grabbable>().enabled = true;
            picture.GetComponent<Picture>().reset();
        }
    }

    private bool pictureClose(GameObject picture, float dist)
    {
        int index = pictures.IndexOf(picture);

        if (index == 0)
            return false;

        for (int i = 0; i < index; i++)
        {
            if (Vector3.Distance(picture.transform.position, pictures[i].transform.position) < dist)
            {
                return true;
            }
        }

        foreach(MRUKAnchor anchor in realPaintings)
        {
            if (Vector3.Distance(picture.transform.position, anchor.transform.position) < dist)
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
            int maxTries = 40;

            if (hit.collider != null && (hit.collider.name.Contains("Cube") || hit.collider.name.Contains("Picture")))
                Debug.Log("Huh?");

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
            if (hit.collider != null && (hit.collider.name.Contains("Cube") || hit.collider.name.Contains("Picture")))
                Debug.Log("Huh?");

            laser.transform.position = position;
            laser.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0, 90, 0);
            laser.GetComponent<Laser>().reset();
            laser.SetActive(true);
        }
    }

}
