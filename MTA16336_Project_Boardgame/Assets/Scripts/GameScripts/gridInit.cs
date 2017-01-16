using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System;
using UnityEngine.UI;
using AForge.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using UnityEngine.SceneManagement;

public class gridInit : MonoBehaviour {

    //Call external classes (Initialize in Start())
    cam Cam;
    PlayerTurnControl playerTurnControl;
    FadeTextManager fadeText;

    //Initialize global variables
    public bool calibrationComplete = false, disableRaycast = false, selectionActive = false, readyToStart = false, bufferingComplete = false;
    public GameObject sphere, tile;
    public Texture2D[] yellowPlayerTemplates, bluePlayerTemplates, greenPlayerTemplates, redPlayerTemplates;

    const int mapWidth = 33, mapHeight = -27;
    const float tileSizeUnits = 1.0f / 3;

    GameObject game;
    bool noSelected = false, yesSelected = false, startTemplateMatching = false;

    //Template Matching
    Bitmap[] yellowBitmapTemplates, blueBitmapTemplates, greenBitmapTemplates, redBitmapTemplates;

    //Initialize class
    void Start()
    {
        Cam = GameObject.Find("mainCamera").GetComponent<cam>(); // Initialize Cam class (Cam.cs)      
        game = GameObject.Find("Game");
        playerTurnControl = GameObject.Find("ScriptHolder").GetComponent<PlayerTurnControl>();
        fadeText = GameObject.Find("ScriptHolder").GetComponent<FadeTextManager>();
        GenerateTileMap();
        StartCoroutine(buffer());

        for (int i = 0; i < goTile.Length - 1; i++)
        {
            goTile[i].GetComponent<Renderer>().enabled = false;
        }
    }

    
    Renderer[] renderer;
    bool doOnce1 = false;
    // Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Application.LoadLevel(0);
        }

        if (Cam.caliRect.Length == 4 && calibrationComplete == false && bufferingComplete == true){
            if (Input.GetKeyDown("space"))
            {
                Calibrate();
                yellowBitmapTemplates = Cam.convertTemplateToBitmapAndBinary(yellowPlayerTemplates);
                blueBitmapTemplates = Cam.convertTemplateToBitmapAndBinary(bluePlayerTemplates);
                greenBitmapTemplates = Cam.convertTemplateToBitmapAndBinary(greenPlayerTemplates);
                redBitmapTemplates = Cam.convertTemplateToBitmapAndBinary(redPlayerTemplates);
            }
        }

        if (calibrationComplete == false)
        {
            goTile[0].GetComponent<Renderer>().enabled = true;
            goTile[32].GetComponent<Renderer>().enabled = true;
            goTile[858].GetComponent<Renderer>().enabled = true;
            goTile[890].GetComponent<Renderer>().enabled = true;
            game.SetActive(false);
        }
        else
        {
            game.SetActive(true);
            goTile[0].GetComponent<Renderer>().enabled = false;
            goTile[32].GetComponent<Renderer>().enabled = false;
            goTile[858].GetComponent<Renderer>().enabled = false;
            goTile[890].GetComponent<Renderer>().enabled = false;

            if (readyToStart == true)
            {
                GenerateObjects();
                RaycastCheck();
                readyToStart = false;
            }

            if (Cam.redBlobs.Length > 0)
            {
                sphere.transform.position = new Vector3(translatedFingerPos().x, translatedFingerPos().y, -6);
            }
        }

        if (startTemplateMatching == true)
        {
            if (playerTurnControl.playerTurnControl() == "Yellow")
            {
                fingerCountInput.text = "Fingers received: " + Cam.foundGesture(yellowBitmapTemplates);
                tmTime -= Time.deltaTime;
                tmCountdown.text = Mathf.Round(tmTime).ToString();
            }
            else if (playerTurnControl.playerTurnControl() == "Blue")
            {
                fingerCountInput.text = "Fingers received: " + Cam.foundGesture(blueBitmapTemplates);
                tmTime -= Time.deltaTime;
                tmCountdown.text = Mathf.Round(tmTime).ToString();
            }
            else if (playerTurnControl.playerTurnControl() == "Green")
            {
                fingerCountInput1.text = "Fingers received: " + Cam.foundGesture(greenBitmapTemplates);
                tmTime -= Time.deltaTime;
                tmCountdown1.text = Mathf.Round(tmTime).ToString();
            }
            else if (playerTurnControl.playerTurnControl() == "Red")
            {
                fingerCountInput1.text = "Fingers received: " + Cam.foundGesture(redBitmapTemplates);
                tmTime -= Time.deltaTime;
                tmCountdown1.text = Mathf.Round(tmTime).ToString();
            }
        }
    }

    public GameObject[] goTile = new GameObject[892];
    bool walkable = false;
    /// <summary>
    /// Creates the complete Tile Map, will be invisible post-calibration
    /// </summary>
    void GenerateTileMap()
    {
        int tileNumber = 0, matrixX = 0, matrixY = 0, layerPos = -2;
        for (float y = 0; y > mapHeight; y--)    
        {      
            for (float x = 0; x < mapWidth; x++)
            {
                matrixY++;
                matrixX++;
                float tilePosX = x/3, tilePosY = y/3;
                goTile[tileNumber] = Instantiate(tile, new Vector3(tilePosX, tilePosY, layerPos), Quaternion.identity) as GameObject;
                goTile[tileNumber].transform.parent = GameObject.Find("TileParent").transform;
                goTile[tileNumber].name = "tile" + tileNumber;
                goTile[tileNumber].tag = "grid";

                tileNumber++;     
            }
        }
    }

    public GameObject center, stair, corner, straight, empty, stair90, stair180, stair270, corner90, corner180, corner270, straight90, mainCenter;
    /// <summary>
    /// Creates the map
    /// </summary>
    GameObject[] ObjectsArray = new GameObject[99];
    public void GenerateObjects()
    { 
        GameObject[,] map = new GameObject[9, 11] { { empty, center, corner180, stair180, stair180, stair180, stair180, stair180, corner270, center, empty }, { center, center, straight, center, stair270, stair180, stair90, center, straight, center, center }, { straight, corner270, center, corner180, center, center, center, corner270, center, corner180, straight }, { corner, center, straight, center, center, stair180, center, center, straight, center, corner90 }, { stair270, center, center, center, stair270, mainCenter, stair90, center, center, center, stair90 }, { corner270, center, straight, center, center, stair, center, center, straight, center, corner180 }, { straight, corner, center, corner90, center, center, center, corner, center, corner90, straight }, { center, center, straight, center, stair270, stair, stair90, center, straight, center, center }, { empty, center, corner90, stair, stair, stair, stair, stair, corner, center, empty } };
        int tileNumber = 0;

        for(int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 11; j++) 
            {
                GameObject go = Instantiate(map[i,j], new Vector3(j + tileSizeUnits, (i * -1) - tileSizeUnits, 0), map[i,j].transform.rotation) as GameObject;
                go.transform.parent = GameObject.Find("ObjectParent").transform;
                tileNumber++;
                go.name = "object" + tileNumber;
                go.tag = "turnableObject";      
            }   
        }

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                ObjectsArray[i * 11 + j] = map[i,j];
            }
        }
    }

    IEnumerator buffer()
    {
        yield return new WaitForSeconds(3f);
        bufferingComplete = true;
    }

    double topLeftX, topLeftY, topRightX, topRightY, bottomLeftX, bottomLeftY, bottomRightX, bottomRightY;
    bool findCaliPoints()
    {

        for (int i = 0; i < Cam.caliRect.Length; i++)
        {
            if (Cam.caliRect[i].X > 320 && Cam.caliRect[i].Y > 240)
            {
                topLeftX = Cam.caliRect[i].X;
                topLeftY = Cam.caliRect[i].Y;
            }
            else if (Cam.caliRect[i].X < 320 && Cam.caliRect[i].Y > 240)
            {
                topRightX = Cam.caliRect[i].X;
                topRightY = Cam.caliRect[i].Y;
            }
            else if (Cam.caliRect[i].X > 320 && Cam.caliRect[i].Y < 240)
            {
                bottomLeftX = Cam.caliRect[i].X;
                bottomLeftY = Cam.caliRect[i].Y;
            }
            else if (Cam.caliRect[i].X < 320 && Cam.caliRect[i].Y < 240)
            {
                bottomRightX = Cam.caliRect[i].X;
                bottomRightY = Cam.caliRect[i].Y;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks for calibration points, ensures corners to be in their right position.
    /// Counters fail calibration.
    /// </summary>
    /// <returns></returns>

    /// <summary>
    /// Calibration of the map. Using EmguCVs method FindHomography to screate a
    /// homography of the camere view. 
    /// </summary>
    double[,] sourcePoints, destPoints, homog = new double[3, 3];
    Emgu.CV.Matrix<double> homographyGlobal;
    void Calibrate()
    {
        if (findCaliPoints() == true)
        {
            sourcePoints = new double[,] { { topLeftX, topLeftY }, { topRightX, topRightY }, { bottomLeftX, bottomLeftY }, { bottomRightX, bottomRightY } };
            destPoints = new double[,] { { goTile[890].transform.position.x, goTile[890].transform.position.y }, { goTile[858].transform.position.x, goTile[858].transform.position.y }, 
        { goTile[32].transform.position.x, goTile[32].transform.position.y }, { goTile[0].transform.position.x, goTile[0].transform.position.y } };
            Emgu.CV.Matrix<double> sourceMat = new Matrix<double>(sourcePoints);
            Emgu.CV.Matrix<double> destMat = new Matrix<double>(destPoints);
            Emgu.CV.Matrix<double> homography = new Matrix<double>(homog);

            CvInvoke.FindHomography(sourceMat, destMat, homography, Emgu.CV.CvEnum.HomographyMethod.Default);
            homographyGlobal = homography;
            Camera.main.backgroundColor = UnityEngine.Color.blue;
            calibrationComplete = true;
        }
    }

    /// <summary>
    /// Uses the homography created in Calibrate() and makes a translation to Unity
    /// making the two maps syncronize.
    /// </summary>
    /// <param name="fingerPos">The position of the player finger(Red blob)</param>
    Vector2 coordToUnity(Vector2 fingerPos)
    {
        double x = (fingerPos.x * homographyGlobal.Data[0, 0]) + (fingerPos.y * homographyGlobal[0, 1]) + (1 * homographyGlobal[0, 2]);
        double y = (fingerPos.x * homographyGlobal.Data[1, 0]) + (fingerPos.y * homographyGlobal[1, 1]) + (1 * homographyGlobal[1, 2]);
        return new Vector3((float)x, (float)y);
    }

    public Vector2 translatedFingerPos()
    {
        Vector2 fingerVec = new Vector2(Cam.redBlobs[0].X, Cam.redBlobs[0].Y);
        Vector2 fingerPos = coordToUnity(fingerVec);
        return fingerPos;
    }

    /// <summary>
    /// Does a raycast of all full-grid tiles. Call everytime the map has
    /// been modified.
    /// </summary>
    void RaycastCheck()
    {

        for (int i = 0; i < goTile.Length - 1; i++)
        {
            goTile[i].tag = "Untagged";
        }

        for (int i = 0; i < goTile.Length - 1; i++)
        {
            Vector3 fwd = goTile[i].transform.TransformDirection(Vector3.forward);
            if (Physics2D.Raycast(goTile[i].transform.position, fwd, 200).collider != null)
            {
                goTile[i].tag = "walkableGrid";
            }
        }
    }

    /// <summary>
    /// Finds the closest tile to the position of the finger
    /// </summary>
    /// <param name="tiles">The array of tiles from the full-grid</param>
    /// <returns></returns>
    Transform FindClosestObject(GameObject[] tiles)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = translatedFingerPos();
        for (int i = 0; i < tiles.Length - 1; i++)
        {
            float dist = Vector3.Distance(tiles[i].transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = tiles[i].transform;
                minDist = dist;
            }
        }
        return tMin;
    }

    /// <summary>
    /// Used to reset the color fo the selected objects
    /// </summary>
    /// <param name="renderArray"></param>
    public void resetObjectRender(Renderer[] renderArray)
    {
        if (renderArray != null)
        {
            foreach (Renderer r in childRenders)
            {
                r.material.color = defaultColor;
            }
        }
    }

    GameObject closestTile, currentSelectedObject, parentObject;
    bool skipFirstPast = true;
    public Renderer[] childRenders;
    UnityEngine.Color defaultColor = UnityEngine.Color.white, highlightColor = UnityEngine.Color.yellow, lockedInColor = UnityEngine.Color.red;
    /// <summary>
    /// Used to select wanted object (3x3 tile)
    /// </summary>
    public void SelectObject()
    {
        if (selectionActive == false)
        {
            if (closestTile != FindClosestObject(goTile).gameObject)
            {
                resetObjectRender(childRenders);

                closestTile = FindClosestObject(goTile).gameObject;

                RaycastHit2D hit;
                Vector3 fwd = closestTile.transform.TransformDirection(Vector3.forward);
                hit = Physics2D.Raycast(closestTile.transform.position, fwd, 200);
                if (hit)
                {
                    if (hit.transform.parent.gameObject.tag == "turnableObject")
                    {
                        Debug.Log("here?");
                        GameObject hitObject = hit.collider.gameObject;
                        parentObject = hitObject.transform.parent.gameObject;
                        childRenders = parentObject.GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in childRenders)
                        {
                            r.material.color = highlightColor;
                            currentSelectedObject = r.gameObject;
                        }
                        StopAllCoroutines();
                        StartCoroutine(checkSelection());
                    }
                }
            }
        }
        else
        {
            RaycastHit2D hit;
            Vector3 fwd = sphere.transform.TransformDirection(Vector3.forward);
            hit = Physics2D.Raycast(sphere.transform.position, fwd, 200);
            if (hit && hit.transform.tag == "yesButton")
            {
                foreach (Renderer r in childRenders)
                {
                    r.material.color = lockedInColor;
                    currentSelectedObject = r.gameObject;
                }
                yesSelected = true;
            }
            else if (hit && hit.transform.tag == "noButton")
            {
                resetObjectRender(childRenders);
                noSelected = true;
            }
        }
    }

    public GameObject confirmSelect, TMPlane, goSelect = null;
    public Text TMInfo, fingerCountInput, TMInfo1, fingerCountInput1, tmCountdown, tmCountdown1;
    bool doOnce = false;
    public bool selectionHasBeenCanceled = false;
    float tmTime = 10;
    public IEnumerator checkSelection()
    {
        GameObject tempObject = currentSelectedObject.transform.parent.gameObject;
        selectionHasBeenCanceled = false;
        yield return new WaitForSeconds(2f);
        if (selectionHasBeenCanceled == true)
        {
            yield break;
        }
        if (tempObject == currentSelectedObject.transform.parent.gameObject && playerTurnControl.playerTurnControl() == "Blue" || tempObject == currentSelectedObject.transform.parent.gameObject && playerTurnControl.playerTurnControl() == "Yellow")
        {
            goSelect = (GameObject)Instantiate(confirmSelect, currentSelectedObject.transform.parent.position + new Vector3(0, 0,-2), Quaternion.identity);
            selectionActive = true;
        }
        else if (tempObject == currentSelectedObject.transform.parent.gameObject && playerTurnControl.playerTurnControl() == "Green" || tempObject == currentSelectedObject.transform.parent.gameObject && playerTurnControl.playerTurnControl() == "Red")
        {
            goSelect = (GameObject)Instantiate(confirmSelect, currentSelectedObject.transform.parent.position + new Vector3(0, 0,-2), Quaternion.Euler(0,0,180));
            selectionActive = true;
        }
        yield return new WaitUntil(() => noSelected == true || yesSelected == true);
        if (yesSelected == true)
        {
            Destroy(goSelect);
            TMPlane.SetActive(true);
            sphere.SetActive(false);
            disableRaycast = true;
            startTemplateMatching = true;
            if (doOnce == false)
            {
                if (playerTurnControl.playerTurnControl() == "Yellow" || playerTurnControl.playerTurnControl() == "Blue")
                {
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, TMInfo));
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, fingerCountInput));
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, tmCountdown));        

                }
                else if (playerTurnControl.playerTurnControl() == "Green" || playerTurnControl.playerTurnControl() == "Red")
                {
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, TMInfo1));
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, fingerCountInput1));
                    StartCoroutine(fadeText.FadeTextToFullAlpha(1f, tmCountdown1));
                }
                StartCoroutine(checkFingerSelection());
                doOnce = true;
            }
            
        }
        else if (noSelected == true)
        {
            selectionActive = false;
            Destroy(goSelect);
            noSelected = false;
        }
    }

    IEnumerator checkFingerSelection()
    {
        if (playerTurnControl.playerTurnControl() == "Yellow")
        {
            int tempInt = Cam.foundGesture(yellowBitmapTemplates);
            yield return new WaitForSeconds(10f);
            rotateObject(parentObject, Cam.foundGesture(yellowBitmapTemplates));
            resetObjectRender(childRenders);
            TMPlane.SetActive(false);
            sphere.SetActive(true);
            disableRaycast = false;
            startTemplateMatching = false;
            yesSelected = false;
            tmTime = 10;
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown1));
            doOnce = false;
        }
        else if (playerTurnControl.playerTurnControl() == "Blue")
        {
            int tempInt = Cam.foundGesture(blueBitmapTemplates);
            yield return new WaitForSeconds(10f);
            rotateObject(parentObject, Cam.foundGesture(blueBitmapTemplates));
            resetObjectRender(childRenders);
            TMPlane.SetActive(false);
            sphere.SetActive(true);
            disableRaycast = false;
            startTemplateMatching = false;
            yesSelected = false;
            tmTime = 10;
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown1));
            doOnce = false;
        }
        else if (playerTurnControl.playerTurnControl() == "Green")
        {
            int tempInt = Cam.foundGesture(greenBitmapTemplates);
            yield return new WaitForSeconds(10f);
            rotateObject(parentObject, Cam.foundGesture(greenBitmapTemplates));
            resetObjectRender(childRenders);
            TMPlane.SetActive(false);
            sphere.SetActive(true);
            disableRaycast = false;
            startTemplateMatching = false;
            yesSelected = false;
            tmTime = 10;
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown1));
            doOnce = false;
        }
        else if (playerTurnControl.playerTurnControl() == "Red")
        {
            int tempInt = Cam.foundGesture(redBitmapTemplates);
            yield return new WaitForSeconds(10f);
            rotateObject(parentObject, Cam.foundGesture(redBitmapTemplates));
            resetObjectRender(childRenders);
            TMPlane.SetActive(false);
            sphere.SetActive(true);
            disableRaycast = false;
            startTemplateMatching = false;
            yesSelected = false;
            tmTime = 10;
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, TMInfo1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, fingerCountInput1));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown));
            StartCoroutine(fadeText.FadeTextToZeroAlpha(1f, tmCountdown1));
            doOnce = false;
        }
        
    }

    void rotateObject(GameObject go, int amountOfRotations)
    {
        if (playerTurnControl.playerTurnControl() == "Yellow")
        {
            go.transform.Rotate(new Vector3(0, 0, -90 * Cam.foundGesture(yellowBitmapTemplates)));
        } 
        else if (playerTurnControl.playerTurnControl() == "Blue")
        {
            go.transform.Rotate(new Vector3(0, 0, -90 * Cam.foundGesture(blueBitmapTemplates)));
        }
        else if (playerTurnControl.playerTurnControl() == "Green")
        {
            go.transform.Rotate(new Vector3(0, 0, -90 * Cam.foundGesture(greenBitmapTemplates)));
        }
        else if (playerTurnControl.playerTurnControl() == "Red")
        {
            go.transform.Rotate(new Vector3(0, 0, -90 * Cam.foundGesture(redBitmapTemplates)));
        }
    }

    public void CancelSelectionCor()
    {
        StopCoroutine(checkSelection());
    }
}
