using UnityEngine;
using UnityEngine.UI;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

public class cam : MonoBehaviour {
   
    gridInit gInit;

    //Initialize variables
    public Image<Bgr, byte> currentFrame;
    public Grayscale grayFilter;
    public Text amountOfBlobs;

    Texture2D originalCamera, blueCamera, redCamera, greenCamera, yellowCamera, thCamera;
    bool bufferingComplete = false;
    Capture capture;
    HSLFiltering redHslFilter;
    BlobCounter bcRed, caliBlobs;
    Threshold threshold;
    BrightnessCorrection brightCorr;  

    //Template Matching
    float similarity;
    ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(0.70f);
    const Int32 divisor = 4;
    int match;

    // Use this for initialization
    void Start()
    {
        redHslFilter = new HSLFiltering();
        originalCamera = new Texture2D(200, 200);
        blueCamera = new Texture2D(200, 200);
        redCamera = new Texture2D(200, 200);
        greenCamera = new Texture2D(200, 200);
        yellowCamera = new Texture2D(200, 200);
        thCamera = new Texture2D(200, 200);
        gInit = GameObject.Find("gridInit").GetComponent<gridInit>();

        caliBlobs = new BlobCounter();
        bcRed = new BlobCounter();
        capture = new Capture();
        grayFilter = new Grayscale(0.2125, 0.7154, 0.0721);
        threshold = new Threshold(150);
        brightCorr = new BrightnessCorrection(-50);
    }

    public Rectangle[] redBlobs, caliRect;
    Bitmap grayCalMap;
    bool doOnce = false;
    // Update is called once per frame
    void Update()
    {
            //Original camera            
            currentFrame = new Image<Bgr, byte>(capture.QueryFrame().Bitmap);

            //RedFilter
            redHslFilter.Hue = new IntRange(350, 10);
            redHslFilter.Saturation = new Range(0.6f, 1.0f);
            redHslFilter.Luminance = new Range(0.05f, 0.8f);
            Bitmap bitmapCurrentframeRed = currentFrame.ToBitmap();
            redHslFilter.ApplyInPlace(bitmapCurrentframeRed);
            
            //Red blobcounter
            bcRed.MinHeight = 5;
            bcRed.MinWidth = 5;
            bcRed.MaxHeight = 15;
            bcRed.MaxWidth = 15;
            bcRed.FilterBlobs = true;
            redBlobs = bcRed.GetObjectsRectangles();
            Bitmap calMap = currentFrame.ToBitmap();
            grayCalMap = grayFilter.Apply(calMap);
            brightCorr.ApplyInPlace(grayCalMap);
            threshold.ApplyInPlace(grayCalMap);

            //CalibrationFilter
            if (gInit.calibrationComplete == false)
            { 
                caliRect = caliBlobs.GetObjectsRectangles();
            }
        
        Resources.UnloadUnusedAssets();
    }
    public Bitmap[] convertTemplateToBitmapAndBinary(Texture2D[] array)
    {
        Bitmap[] newArray = new Bitmap[array.Length];
        Bitmap tempBit;
        Byte[] tempByte;

        for (int i = 0; i < array.Length; i++)
        {
            tempByte = array[i].EncodeToJPG();

            using (var ms = new MemoryStream(tempByte))
            {
                tempBit = new Bitmap(ms);
                tempBit = grayFilter.Apply(tempBit);

                new Threshold().ApplyInPlace(tempBit);

                newArray[i] = tempBit;
            }
        }

        return newArray;
    }

    public int findGesture(Bitmap inputVideo, Bitmap[] arrayOfTemplates)
    {
        //gesture = gesture.GetComponent<Text>();

        similarity = 0;

        for (int i = 0; i < arrayOfTemplates.Length; i++)
        {
            TemplateMatch[] tm = etm.ProcessImage(new ResizeNearestNeighbor(inputVideo.Width / divisor, inputVideo.Height / divisor).Apply(inputVideo), new ResizeNearestNeighbor(arrayOfTemplates[i].Width / divisor, arrayOfTemplates[i].Height / divisor).Apply(arrayOfTemplates[i]));

            if (tm.Length == 1 && tm[0].Similarity > similarity)
            {
                similarity = tm[0].Similarity;
                match = i;
            }
        }

        if (match <= 3)
        {
            //gesture = gesture.GetComponent<Text>();
            //gesture.text = "one finger is shown";
            match = 1;
        }
        else if (match > 3 && match <= 7)
        {
            //gesture = gesture.GetComponent<Text>();
            //gesture.text = "Two fingers is shown";
            match = 2;

        }
        else if (match >= 8)
        {
            //gesture = gesture.GetComponent<Text>();
            //gesture.text = "Three fingers is shown";
            match = 3;
        }

        return match;

    }

    public int foundGesture(Bitmap[] playerTemplates) {
        int tempInt = findGesture(grayCalMap, playerTemplates);
        return tempInt;
    }
}
