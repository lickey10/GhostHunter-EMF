using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EMF : MonoBehaviour {
	float magneticHeading = 0;
	float trueHeading = 0;
	float currentTrueHeading = 0;
	Vector3 rawVector;
	Vector3 hitLocation;
	double hitTimeStamp;
	double currentHitTimeStamp;
	bool firstRun = true;
	int consecutiveHits = 0;//number of hits that happen within a certain timeframe - 5 seconds
    bool displayTrueHeadingInfo = false;
    bool displayMagneticHeadingInfo = false;
    bool displayRawVectorInfo = false;
	
	public Image BackgroundImage;
	public Text TrueHeadingValue;
	public Text MagneticHeadingValue;
	public Text RawVectorValueX;
	public Text RawVectorValueY;
	public Text RawVectorValueZ;
	public Text GeoLocationValue;
	public Text emfIntensityText;
	private float emfIntensity = 0;
	private float emfIntensity_previous = 0;
	double previousReadingTimestamp = 0;
	int readingDisplayDelay = 10;//seconds to delay the display of the reading.  calms the display down for the user
	public int EMF_MediumThreshold = 400; //the threshold for a medium emf hit
	public int EMF_LargeThreshold = 600; //the threshold for a large emf hit
	public int GhostDiscoveryThreshold = 15;//the threshold for changes in trueHeading readings before it's considered a hit
	public int ConsecutiveHitsThreshold = 5;//number of hits before it is considered a "big" hit
	public int ConsecutiveHitsTimeFrame = 5;//number of seconds between hits for hits to be "consecutive"
	public bool AutomaticPictureTaking = false; //whether or not the app will automatically take pictures
	public int AutomaticPictureNumber = 5; //the number of pictures to take if AutomaticPictureTaking is on and triggered
	public bool VibrateHandset = true;//whether the handset will vibrate on a big hit
    LookAtTargetController targetController;

	System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

	// Use this for initialization
	void Start () {
		Input.location.Start();
		Input.compass.enabled = true;
        targetController = transform.root.GetComponentInChildren<LookAtTargetController>();
	}
	
	// Update is called once per frame
	void Update () {
		magneticHeading = Input.compass.magneticHeading;
		trueHeading = Input.compass.trueHeading;
		rawVector = Input.compass.rawVector;
		hitTimeStamp = Input.compass.timestamp;

		//if (TrueHeadingValue != null)
		//{
		//    TrueHeadingValue.text = trueHeading.ToString("f1");
		//    MagneticHeadingValue.text = magneticHeading.ToString("f1");
		//    RawVectorValueX.text = rawVector.x.ToString("f1");
		//    RawVectorValueY.text = rawVector.y.ToString("f1");
		//    RawVectorValueZ.text = rawVector.z.ToString("f1");

		//    targetController.RawVector = rawVector;
		//}

		
		//hitTimeStamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;

		//if (previousReadingTimestamp == 0)
		//	previousReadingTimestamp = hitTimeStamp;

		//var tempTimeDifference = hitTimeStamp - previousReadingTimestamp;

		//if (tempTimeDifference > readingDisplayDelay)
		//{
		//	float temp = Mathf.Abs((float)previousReadingTimestamp - (float)hitTimeStamp);

		//	previousReadingTimestamp = hitTimeStamp;
		//}

		if (rawVector != null)//there are results from our sensors
		{
			emfIntensity = rawVector.magnitude;

			if (emfIntensity != emfIntensity_previous && (hitTimeStamp - previousReadingTimestamp > readingDisplayDelay))
			{
				previousReadingTimestamp = hitTimeStamp;
				emfIntensity_previous = emfIntensity;

				emfIntensityText.text = emfIntensity.ToString("f0");

				//emfIntensityText.text = tempTimeDifference.ToString("f0");
				//emfIntensityText.text = hitTimeStamp.ToString("f0");

				if (BackgroundImage != null)
				{
					if (emfIntensity > EMF_LargeThreshold)
						BackgroundImage.color = Color.red;
					else if (emfIntensity > EMF_MediumThreshold)
						BackgroundImage.color = Color.yellow;
					else
						BackgroundImage.color = Color.blue;
				}
			}
		}

		//checkForGhosts();
	}

    public void OnGUI()
    {
        //if(displayTrueHeadingInfo)
        //    GUILayout.Label("Popup Options Example 1");
        //else if(displayMagneticHeadingInfo)
        //    GUILayout.Label("Popup Options Example 2");
        //else if(displayRawVectorInfo)
        //    GUILayout.Label("Popup Options Example 3");
    }

    public void DisplayTrueHeadingInfo()
    {
        Application.LoadLevel("EMFDescriptionInfo");
        displayTrueHeadingInfo = true;
        displayRawVectorInfo = false;
        displayMagneticHeadingInfo = false;
    }

    public void DisplayMagneticHeadingInfo()
    {
        Application.LoadLevel("EMFDescriptionInfo");
        displayMagneticHeadingInfo = true;
        displayRawVectorInfo = false;
        displayTrueHeadingInfo = false;
    }

    public void DisplayRawVectorInfo()
    {
        Application.LoadLevel("EMFDescriptionInfo");
        displayRawVectorInfo = true;
        displayMagneticHeadingInfo = false;
        displayTrueHeadingInfo = false;
    }

    public void DisplayEMF()
    {
        Application.LoadLevel("main");
    }

    public void StartApp()
    {
        Application.LoadLevel("main");
    }

    bool checkForGhosts()
	{
		bool foundGhost = false;
		
		if(!firstRun)
		{
			if(Mathf.Abs(currentTrueHeading - trueHeading) > GhostDiscoveryThreshold)//magnetic field fluctuation detected
			{
				foundGhost = true;
				
				if(hitTimeStamp - Input.compass.timestamp < ConsecutiveHitsTimeFrame)//make sure we are within a given timeframe
					consecutiveHits++;
				else
				{
					//if(consecutiveHits >= ConsecutiveHitsThreshold)//this is a "big" hit
					//	saveHit();
					
					consecutiveHits = 0;
				}
			}
			//else//no field fluctuations detected so we will look for other signs
			//	detectEntities();
		}
		else
			firstRun = false;
		
		currentTrueHeading = trueHeading;
		currentHitTimeStamp = hitTimeStamp;

        if (BackgroundImage != null)
        {
            if (foundGhost)
                BackgroundImage.color = Color.red;
            else
                BackgroundImage.color = Color.blue;
        }

		return foundGhost;
	}
	
	void saveHit()
	{
		//save this one
		saveGhost();
		
		if(VibrateHandset)//vibrate the handset
			Handheld.Vibrate();
		
		if(AutomaticPictureTaking)//take pictures
		{
			webcamClass theCamera = new webcamClass();
			
			for(int x = 0; x < AutomaticPictureNumber; x++)
				theCamera.TakeSnapshot();
			
			theCamera = null;
		}
	}
	
	void saveGhost()
	{
		DetectedGhost detectedGhost = new DetectedGhost();
		detectedGhost.ConsecutiveHits = consecutiveHits;
		detectedGhost.Location = hitLocation;
		detectedGhost.MagneticHeading = magneticHeading;
		detectedGhost.RawVector = rawVector;
		detectedGhost.TrueHeading = trueHeading;
		detectedGhost.TimeStamp = hitTimeStamp;
		
		detectedGhost.Save();
	}

	/// <summary>
	/// Detect entities using other signs
	/// </summary>
	bool detectEntities()
	{
		bool entityDetected = false;
		
		
		return entityDetected;
	}
}
