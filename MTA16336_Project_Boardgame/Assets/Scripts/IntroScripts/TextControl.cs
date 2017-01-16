using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextControl : MonoBehaviour {

    Color zeroAlpha;

    //Initialize global variables
    public Text welcomeText;
    public Text caliText;
    public Text caliText2;
    public Text hoveringText;

	// Use this for initialization
	void Start () {
     
	}

    bool once = true;
	// Update is called once per frame
	void Update () {
        if (once == true)
        {
            StartCoroutine(textFades());
            once = false;
        }
        
    }

    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    IEnumerator textFades()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeTextToFullAlpha(2f, welcomeText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(FadeTextToZeroAlpha(2f, welcomeText));
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeTextToFullAlpha(2f, hoveringText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(FadeTextToZeroAlpha(2f, hoveringText));
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeTextToFullAlpha(2f, caliText));
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeTextToFullAlpha(2f, caliText2));
        yield return new WaitForSeconds(5f);
        StartCoroutine(FadeTextToZeroAlpha(2f, caliText));
        StartCoroutine(FadeTextToZeroAlpha(2f, caliText2));
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
        SceneManager.UnloadScene(0);
        
    }
}

