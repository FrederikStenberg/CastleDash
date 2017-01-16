using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerTurnControl : MonoBehaviour {

    gridInit gInit;

    public Text playerText, amountSelectedText, gamePiecesText;
    public GameObject centerPiece, playerSelection, placeGamePieces, redTurn, blueTurn, greenTurn, yellowTurn, continueButton;

    bool doOnce = false, turnEnd = true;
    List<string> players = null;

    FadeTextManager fadeText;

	// Use this for initialization
	void Start () {
        gInit = GameObject.Find("gridInit").GetComponent<gridInit>();
        fadeText = GameObject.Find("ScriptHolder").GetComponent<FadeTextManager>();
	}

    public Sprite grayContinue, colorContinue;
    string gameState = "Select players";
	// Update is called once per frame
	void Update () {
        if (gInit.calibrationComplete == true)
        {
            gameCycleSwitchControl(gameState);
        }

        if (selectionActive == true)
        {
            gInit.SelectObject();
        }

        if (selectedPlayerAmountInt() == 0)
        {
            if (continueButton.GetComponent<SpriteRenderer>().sprite != grayContinue)
            {
                continueButton.GetComponent<SpriteRenderer>().sprite = grayContinue;
            }
            noPlayersSelected = true;
        }
        else
        {
            if (continueButton.GetComponent<SpriteRenderer>().sprite != colorContinue)
            {
                continueButton.GetComponent<SpriteRenderer>().sprite = colorContinue;
            }
            noPlayersSelected = false;
        }
	}

    /// <summary>
    /// Controls the state of the game using
    /// switch cases.
    /// </summary>
    void gameCycleSwitchControl(string state)
    {
        switch (state)
        {
            case "Select players":
                if (doOnce == false)
                {
                    StartCoroutine(selectPlayerAmountText());
                    doOnce = true;
                }
                amountSelectedText.text = selectedPlayerAmountInt() + " players selected";
                raycastButtonControl();
                break;  

            case "Place game pieces":
                if (doOnce == false)
                {
                    players = playerBoolToPlayerString();
                    GameObject center = Instantiate(centerPiece, new Vector3(5.333333f, -4.333333f, 0), Quaternion.identity) as GameObject;
                    StartCoroutine(placeGamePiecesText());
                    playerText.text = "";
                    amountSelectedText.text = "";
                    doOnce = true;
                }
                raycastButtonControl();
                break;

            case "Ready to start":
                if (doOnce == false)
                {
                    activateButtonsForPlayer();
                    disableAllPlayerButtons();
                    gamePiecesText.text = "";        
                    doOnce = true;
                }
                playerTurnSwitchControl(playerTurnControl());
                raycastButtonControl();
                break;
        }
    }

    /// <summary>
    /// Activates buttons for selected players.
    /// </summary>
    void activateButtonsForPlayer()
    {
        if (players.Contains("Green"))
        {
            GameObject greenButtons = GameObject.Find("LiveGame").transform.Find("GreenPlayer").gameObject;
            greenButtons.SetActive(true);
        }

        if (players.Contains("Yellow"))
        {
            GameObject yellowButtons = GameObject.Find("LiveGame").transform.Find("YellowPlayer").gameObject;
            yellowButtons.SetActive(true);
        }

        if (players.Contains("Red"))
        {
            GameObject redButtons = GameObject.Find("LiveGame").transform.Find("RedPlayer").gameObject;
            redButtons.SetActive(true);
        }

        if (players.Contains("Blue"))
        {
            GameObject blueButtons = GameObject.Find("LiveGame").transform.Find("BluePlayer").gameObject;
            blueButtons.SetActive(true);
        }
    }

    int currentTurnIndex = 0;
    string playerTurn = null;
    public string playerTurnControl()
    {
        if (turnEnd == true)
        {
            disableAllPlayerButtons();
            currentTurnIndex++;
            doOnceTheSecond = false;

            if (currentTurnIndex > players.Count)
            {
                currentTurnIndex = 0;
                playerTurn = players[currentTurnIndex];
            }
            else
            {
                playerTurn = players[currentTurnIndex];
            }
            turnEnd = false;
        }
        return playerTurn;
    }

    /// <summary>
    /// Switch function for each player.
    /// </summary>
    bool doOnceTheSecond = false;
    void playerTurnSwitchControl(string turn)
    {
        switch (turn)
        {
            case "Green":
                if (doOnceTheSecond == false)
                {
                    StartCoroutine(greenTurnSpriteText());
                    activateCurrentPlayerButtons(greenPlayerRotate, greenPlayerEnd);
                    doOnceTheSecond = true;
                }
                break;

            case "Yellow":
                if (doOnceTheSecond == false)
                {
                    StartCoroutine(yellowTurnSpriteText());
                    activateCurrentPlayerButtons(yellowPlayerRotate, yellowPlayerEnd);
                    doOnceTheSecond = true;
                }
                break;

            case "Red":
                if (doOnceTheSecond == false)
                {
                    StartCoroutine(redTurnSpriteText());
                    activateCurrentPlayerButtons(redPlayerRotate, redPlayerEnd);
                    doOnceTheSecond = true;
                }
                break;

            case "Blue":
                if (doOnceTheSecond == false)
                {
                    StartCoroutine(blueTurnSpriteText());
                    activateCurrentPlayerButtons(bluePlayerRotate, bluePlayerEnd);
                    doOnceTheSecond = true;
                }
                break;
        }
    }

    public GameObject bluePlayerRotate, bluePlayerEnd, greenPlayerRotate, greenPlayerEnd, redPlayerRotate, redPlayerEnd, yellowPlayerRotate, yellowPlayerEnd;
    public Sprite grayEnd, grayRotate, colorEnd, colorRotate;
    void disableAllPlayerButtons()
    {
        bluePlayerRotate.GetComponent<BoxCollider2D>().enabled = false;
        bluePlayerEnd.GetComponent<BoxCollider2D>().enabled = false;
        greenPlayerRotate.GetComponent<BoxCollider2D>().enabled = false;
        greenPlayerEnd.GetComponent<BoxCollider2D>().enabled = false;
        redPlayerRotate.GetComponent<BoxCollider2D>().enabled = false;
        redPlayerEnd.GetComponent<BoxCollider2D>().enabled = false;
        yellowPlayerRotate.GetComponent<BoxCollider2D>().enabled = false;
        yellowPlayerEnd.GetComponent<BoxCollider2D>().enabled = false;

        bluePlayerRotate.GetComponent<SpriteRenderer>().sprite = grayRotate;
        bluePlayerEnd.GetComponent<SpriteRenderer>().sprite = grayEnd;
        greenPlayerRotate.GetComponent<SpriteRenderer>().sprite = grayRotate;
        greenPlayerEnd.GetComponent<SpriteRenderer>().sprite = grayEnd;
        redPlayerRotate.GetComponent<SpriteRenderer>().sprite = grayRotate;
        redPlayerEnd.GetComponent<SpriteRenderer>().sprite = grayEnd;
        yellowPlayerRotate.GetComponent<SpriteRenderer>().sprite = grayRotate;
        yellowPlayerEnd.GetComponent<SpriteRenderer>().sprite = grayEnd;
    }

    void activateCurrentPlayerButtons(GameObject currentRotate, GameObject currentEnd)
    {
        currentRotate.GetComponent<BoxCollider2D>().enabled = true;
        currentEnd.GetComponent<BoxCollider2D>().enabled = true;
        currentRotate.GetComponent<SpriteRenderer>().sprite = colorRotate;
        currentEnd.GetComponent<SpriteRenderer>().sprite = colorEnd;
    }

    void disableCurrentRotate(GameObject currentRotate)
    {
        currentRotate.GetComponent<BoxCollider2D>().enabled = false;
        currentRotate.GetComponent<SpriteRenderer>().sprite = grayRotate;
    }

    /// <summary>
    /// Controls the selection and deselection of players.
    /// </summary>
    bool[] selectedColors = new bool[4]{false, false, false, false}; //[0]green, [1]yellow, [2]red, [3]blue
    
    bool selectionActive = false, noPlayersSelected = true;
    public GameObject greenCross, blueCross, redCross, yellowCross;
    void raycastButtonControl()
    {
        if (gInit.disableRaycast == false)
        {
            RaycastHit2D hit;
            Vector3 fwd = gInit.sphere.transform.TransformDirection(Vector3.forward);
            hit = Physics2D.Raycast(gInit.sphere.transform.position, fwd, 200);
            if (hit && hit.transform.tag == "greenSelect")
            {
                if (selectedColors[0] == false)
                {
                    greenCross.GetComponent<SpriteRenderer>().enabled = true;
                    selectedColors[0] = true;
                }
            }
            else if (hit && hit.transform.tag == "yellowSelect")
            {
                if (selectedColors[1] == false)
                {
                    yellowCross.GetComponent<SpriteRenderer>().enabled = true;
                    selectedColors[1] = true;
                }
            }
            else if (hit && hit.transform.tag == "redSelect")
            {
                if (selectedColors[2] == false)
                {
                    redCross.GetComponent<SpriteRenderer>().enabled = true;
                    selectedColors[2] = true;
                }
            }
            else if (hit && hit.transform.tag == "blueSelect")
            {
                if (selectedColors[3] == false)
                {
                    blueCross.GetComponent<SpriteRenderer>().enabled = true;
                    selectedColors[3] = true;
                }
            }
            else if (hit && hit.transform.tag == "greenDeselect")
            {
                if (selectedColors[0] == true)
                {
                    greenCross.GetComponent<SpriteRenderer>().enabled = false;
                    selectedColors[0] = false;
                }
            }
            else if (hit && hit.transform.tag == "yellowDeselect")
            {
                if (selectedColors[1] == true)
                {
                    yellowCross.GetComponent<SpriteRenderer>().enabled = false;
                    selectedColors[1] = false;
                }
            }
            else if (hit && hit.transform.tag == "redDeselect")
            {
                if (selectedColors[2] == true)
                {
                    redCross.GetComponent<SpriteRenderer>().enabled = false;
                    selectedColors[2] = false;
                }
            }
            else if (hit && hit.transform.tag == "blueDeselect")
            {
                if (selectedColors[3] == true)
                {
                    blueCross.GetComponent<SpriteRenderer>().enabled = false;
                    selectedColors[3] = false;
                }
            }
            else if (hit && hit.transform.tag == "continue")
            {
                if (noPlayersSelected == false)
                {
                    doOnce = false;
                    gameState = "Place game pieces";
                    playerSelection.SetActive(false);
                    placeGamePieces.SetActive(true);
                }
            }
            else if (hit && hit.transform.tag == "startGame")
            {    
                doOnce = false;
                gameState = "Ready to start";
                placeGamePieces.SetActive(false);
                gInit.readyToStart = true;   
            }
            else if (hit && hit.transform.tag == "endTurn")
            {
                turnEnd = true;
                selectionActive = false;
                gInit.selectionActive = true;
                gInit.selectionHasBeenCanceled = true;
                gInit.resetObjectRender(gInit.childRenders);
                if (gInit.goSelect != null)
                {
                    Destroy(gInit.goSelect);
                }
            }
            else if (hit && hit.transform.tag == "rotate")
            {
                selectionActive = true;
                gInit.selectionActive = false;
                if (playerTurnControl() == "Green")
                {
                    disableCurrentRotate(greenPlayerRotate);
                }
                else if (playerTurnControl() == "Blue")
                {
                    disableCurrentRotate(bluePlayerRotate);
                }
                else if (playerTurnControl() == "Red")
                {
                    disableCurrentRotate(redPlayerRotate);
                }
                else if (playerTurnControl() == "Yellow")
                {
                    disableCurrentRotate(yellowPlayerRotate);
                }
            }
        }
       
    }

    /// <summary>
    /// Uses the boolean array selectedColors and translates
    /// the selections to playingColors which then can be used
    /// output to text.
    /// </summary>
    /// <returns></returns>
    List<string> playerBoolToPlayerString()
    {
        string[] playingColors = new string[4] { "Green", "Yellow", "Red", "Blue" };
        List<string> playingColorsList = new List<string>();
        for (int i = 0; i < selectedColors.Length; i++)
        {   
            if (selectedColors[i] == false)
            {
                playingColors[i] = null;
            }
        }

        for (int i = 0; i < playingColors.Length; i++)
        {
            if (playingColors[i] != null)
            {
                Debug.Log(playingColors[i]);
                playingColorsList.Add(playingColors[i]);
            }
        }  
        return playingColorsList;
    }

    /// <summary>
    /// Managing an int used to display the amount of 
    /// players(colors) selected.
    /// </summary>
    /// <returns></returns>
    int selectedPlayerAmountInt()
    {
        int amountSelected = 0, loopAmount = 0;
        for (int i = 0; i < selectedColors.Length; i++)
        {
            if (selectedColors[i] == true)
            {
                loopAmount++;
            }
        }
        amountSelected = loopAmount;
        return amountSelected;
    }

    IEnumerator selectPlayerAmountText() {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(fadeText.FadeTextToFullAlpha(1f, playerText));
        StartCoroutine(fadeText.FadeTextToFullAlpha(1f, amountSelectedText));
    }

    IEnumerator placeGamePiecesText()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(fadeText.FadeTextToFullAlpha(1f, gamePiecesText));

    }

    IEnumerator redTurnSpriteText()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(fadeText.FadeSpriteToFullAlpha(1f, redTurn.GetComponent<SpriteRenderer>()));
        yield return new WaitForSeconds(5f);
        StartCoroutine(fadeText.FadeSpriteToZeroAlpha(1f, redTurn.GetComponent<SpriteRenderer>()));
    }

    IEnumerator greenTurnSpriteText()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(fadeText.FadeSpriteToFullAlpha(1f, greenTurn.GetComponent<SpriteRenderer>()));
        yield return new WaitForSeconds(5f);
        StartCoroutine(fadeText.FadeSpriteToZeroAlpha(1f, greenTurn.GetComponent<SpriteRenderer>()));
    }

    IEnumerator blueTurnSpriteText()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(fadeText.FadeSpriteToFullAlpha(1f, blueTurn.GetComponent<SpriteRenderer>()));
        yield return new WaitForSeconds(5f);
        StartCoroutine(fadeText.FadeSpriteToZeroAlpha(1f, blueTurn.GetComponent<SpriteRenderer>()));
    }

    IEnumerator yellowTurnSpriteText()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(fadeText.FadeSpriteToFullAlpha(1f, yellowTurn.GetComponent<SpriteRenderer>()));
        yield return new WaitForSeconds(5f);
        StartCoroutine(fadeText.FadeSpriteToZeroAlpha(1f, yellowTurn.GetComponent<SpriteRenderer>()));
    }
}
