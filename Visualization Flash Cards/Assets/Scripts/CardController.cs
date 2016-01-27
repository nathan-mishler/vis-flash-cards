using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CardController : MonoBehaviour {

	public List<GameObject> cards;
	public List<Vector3> cardPositions;
	public List<Sprite> visualizationList;

	public List<string> titles;
	public List<string> descriptions;
	public List<int> types;
	public List<string> pages;
	public List<string> typeNames;

	public RectTransform panel; 
	public Text titleField;
	public Text typeField;
	public Text textField;
	public Text pageField;
	public Text pageTitle; 
	public Text positionField;
	public Image positionImage;

	public GameObject blackBG;
	public GameObject whiteBG;

	bool textShowing = false;
	bool inVisualization = true;

	// The order of the visible cards
	public List<int> cardOrder;
	public List<int> visOrder;

	int visPosition = 0;

	// The camera's initial position
	float cameraSize;
	Vector3 cameraPosition; 
	Vector3 hidePosition = new Vector3 (0f, -30f);
	Vector2 panelShow = new Vector2 (0f, 0f);
	Vector2 panelHide = new Vector2 (0f, 1536f);

	// The minimum orthographic size value we want to zoom to
	public float MinSize = 10.0f;
	
	// The maximum orthographic size value we want to zoom to
	public float MaxSize = 60.0f;

	// The time each swipe takes
	public float swipeTime = 1.0f;
	public bool isSwiping = false;


	// The return button and its image
	public GameObject returnButtonPanel;

	// We're going to allow for a little panning when we're zoomed in
	private Lean.LeanFinger draggingFinger;
	private Vector2 lastFingerPos;

	Vector2 minCamera = new Vector2 (-8.0f, -4f);
	Vector2 maxCamera = new Vector2 (8f, 4f);

	// instruction scenes
	public List<GameObject> titleCards; 

	public GameObject startArrow;

	public List<Vector2> encPositions;

	bool adjustSource = true;


	// Internal positioning - where are we in the app?
	// 0 = title
	// 1 = credits
	// 2 = instructions
	// 3 = cards
	public int appPosition = 0;

	// Can we tap to show card back? Used to ignore a second of a tap when coming from the title screens
	public bool canFlipCard = false;


	public void reloadPlay() {
		textShowing = false;
		Reset (false);
	}

	// Use this for initialization
	void Start () {



		Lean.LeanTouch.OnFingerSwipe += FingerSwipe;
		Lean.LeanTouch.OnFingerDown += FingerDown;
		Lean.LeanTouch.OnFingerUp += FingerUp;
		Lean.LeanTouch.OnFingerTap += FingerTap;

		cameraPosition = Camera.main.transform.position; 
		cameraSize = Camera.main.orthographicSize;

		Reset ();


	}

	public void Reset (bool _reloadCards = true)
	{

		foreach (GameObject g in titleCards) {
			g.SetActive(false);
		}

		titleCards [0].SetActive (true);
		startArrow.SetActive (true);

		if (cardPositions.Count != 0) {
			panel.anchoredPosition = panelHide;
		}
	
		if (cardPositions.Count == 0) {

			foreach (GameObject c in cards) {
				cardPositions.Add (c.transform.position);
			}
		} else {
			if(_reloadCards) {
				cards [0].transform.position = cardPositions[0];
				cards [1].transform.position = cardPositions[1];
				cards [2].transform.position = cardPositions[2];
			} else {
				cards [cardOrder[0]].transform.position = cardPositions[0];
				cards [cardOrder[1]].transform.position = cardPositions[1];
				cards [cardOrder[2]].transform.position = cardPositions[2];
			}
		}
		Camera.main.transform.position = cameraPosition; 
		Camera.main.orthographicSize = cameraSize;

		inVisualization = false;
		appPosition = 0;


		returnButtonPanel.SetActive (false);
		blackBG.SetActive (true);
		whiteBG.SetActive (true);
		positionImage.gameObject.SetActive (false);
		if (_reloadCards) {
			visPosition = 0;
			ResetCards ();
		}
	}

	IEnumerator waitToFlip() {
		canFlipCard = false;
		yield return new WaitForSeconds (0.25f);
		canFlipCard = true;
	}

	IEnumerator waitLongerToFlip() {
		canFlipCard = false;
		yield return new WaitForSeconds (1);
		canFlipCard = true;
	}

	public void MoveGuiElement(Vector2 position){
		panel.anchoredPosition = position;
	}
	
	void FingerTap (Lean.LeanFinger obj)
	{
		if (appPosition != 3) {
			startArrow.SetActive(false);
			titleCards [appPosition].SetActive (false);
			appPosition++;


			blackBG.SetActive(false);
			if (appPosition == 3) {
				whiteBG.SetActive(false);
				returnButtonPanel.SetActive(true);
				inVisualization = true;
				StartCoroutine(waitToFlip());
				positionImage.gameObject.SetActive (true);
				positionImage.fillAmount = (float)(visPosition+1f)/visualizationList.Count;
			}else{
				titleCards[appPosition].SetActive(true);
			}
		} 

		if( inVisualization && canFlipCard && !isSwiping ) {

			Debug.Log(cameraSize);
			Debug.Log(Camera.main.orthographicSize);

			if (cameraSize == Camera.main.orthographicSize) {
				

				
				if (!textShowing) {

					textShowing = true;

					iTween.MoveTo (cards [cardOrder [1]], hidePosition, 0);
					iTween.ValueTo(panel.gameObject, iTween.Hash(
						"from", panel.anchoredPosition,
						"to", panelShow,
						"time", 0,
						"onupdatetarget", this.gameObject, 
						"onupdate", "MoveGuiElement"));

				
					titleField.text = titles [visOrder [visPosition]];
					textField.text = descriptions [visOrder [visPosition]].Replace("; ", ";\n");
					typeField.text = typeNames[ types[visOrder[visPosition]] ];

					string _atlas = "<i>Atlas of Knowlege</i>, ";
					string _page_or_pages = "";

					if ( pages [ visOrder [visPosition]].IndexOf("and") != -1 ) {
						_page_or_pages = "pages ";
					}else{
						_page_or_pages = "page ";
					}

					pageField.text = _atlas + _page_or_pages + pages [ visOrder [visPosition]];



					if( adjustSource ) { 
						int count = descriptions [visOrder [visPosition]].Split (';').Length-1;
						(pageTitle.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageTitle.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
						(pageField.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageField.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
					}
					
				}

			

				else {
					textShowing = false;
					iTween.MoveTo (cards [cardOrder [1]], cardPositions[1], 0);
					iTween.ValueTo(panel.gameObject, iTween.Hash(
						"from", panel.anchoredPosition,
						"to", panelHide,
						"time", 0,
						"onupdatetarget", this.gameObject, 
						"onupdate", "MoveGuiElement"));
				}
			}	

		}
	}
	
	// Only track this if we are zoomed in
	void FingerDown (Lean.LeanFinger obj)
	{

		if (cameraSize != Camera.main.orthographicSize) {

			if (Lean.LeanTouch.Fingers.Count == 1) {
				draggingFinger = obj;
				lastFingerPos = draggingFinger.DeltaScreenPosition;
			}
		}

	}

	void FingerUp (Lean.LeanFinger obj)
	{
		draggingFinger = null; 
	}
	
	protected virtual void LateUpdate()
	{

		if (inVisualization) {
			// Does the main camera exist?
			if (Camera.main != null && !textShowing) {
				// Make sure the pinch scale is valid
				if (Lean.LeanTouch.PinchScale > 0.0f) {
					float orthSize = Camera.main.orthographicSize;


					// Scale the Orthographic based on the pinch scale
					Camera.main.orthographicSize /= Lean.LeanTouch.PinchScale;
				
					// Make sure the new Orthographic is within our min/max
					Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, MinSize, MaxSize);

					// If we actually changed the scale, center camera
					if( orthSize != Camera.main.orthographicSize ) {
						Camera.main.transform.position = cameraPosition;

					}
				}
			}

			if (draggingFinger != null)
			{
				Lean.LeanTouch.MoveCamera(Camera.main.transform, draggingFinger.DeltaScreenPosition, draggingFinger.ScaledDeltaScreenPosition, minCamera, maxCamera);
			}
		}
	}

	void ResetCards ()
	{
		// Set the order that the list will show in
		visOrder = new List<int> ();

		List<int> availableVis = new List<int>();

		for (int h = 0; h < visualizationList.Count; h++) {
			availableVis.Add(h);
		}

		for (int i = 0; i < visualizationList.Count; i++)
		{
			int itemNum = Random.Range(0, availableVis.Count);
			
			visOrder.Add (availableVis[itemNum]);

			availableVis.RemoveAt(itemNum);
			//visOrder.Add (availableVis[i]);

		}

		cardOrder = new List<int> {0, 1, 2};


		cards [0].transform.position = cardPositions [0];
		if (textShowing) {
			cards [1].transform.position = hidePosition;
		} else {
			cards [1].transform.position = cardPositions [1];
		}
		cards [2].transform.position = cardPositions [2];

		cards [1].GetComponent<SpriteRenderer>().sprite = visualizationList[ visOrder[0] ];
		cards [2].GetComponent<SpriteRenderer>().sprite = visualizationList[ visOrder[1] ];

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void FingerSwipe (Lean.LeanFinger finger) {

		if (inVisualization) {
			if (Lean.LeanTouch.Fingers.Count == 1) {

				// Store the swipe delta in a temp variable
				var swipe = finger.SwipeDelta;
				// Swipe left, which moves all cards to the left
				if (swipe.x < -Mathf.Abs (swipe.y)) {
					// Only activate swipe if we aren't zoomed in
					if (cameraSize == Camera.main.orthographicSize) {

						if (!textShowing && !isSwiping) {

							if (visPosition + 1 < visualizationList.Count) {
								iTween.Stop();
								isSwiping = true;
								Camera.main.orthographicSize = cameraSize;
								visPosition++;

								//int new_right = cardOrder [0];
								//cardOrder.RemoveAt (0);
								//cardOrder.Add (new_right);
								for (int i = 0; i < 3; i++) {
									cardOrder[i]++;
									if (cardOrder[i] == 3){
										cardOrder[i] = 0;
									}
								}


								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], swipeTime);
								iTween.MoveTo (cards [cardOrder [1]], cardPositions [1], swipeTime);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], 0);
								//iTween.MoveTo ( Camera.main.gameObject, cameraPosition, swipeTime * .75f);
								

								//cards [cardOrder [2]].transform.position = cardPositions [2];
								//positionField.text = (visPosition + 1).ToString() + "/" + visualizationList.Count.ToString ();
								positionImage.fillAmount = (float)(visPosition+1) / (float)visualizationList.Count;
								// Redo the cards
								if( visPosition + 1 <= visualizationList.Count -1 ) {
									cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition + 1]];
								}
								swipeGo ();

							}
							else{

								ResetCards();
								visPosition = 0;
								cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition]];

								iTween.Stop();
								isSwiping = true;
								Camera.main.orthographicSize = cameraSize;
								visPosition = 0;
								

								for (int i = 0; i < 3; i++) {
									cardOrder[i]++;
									if (cardOrder[i] == 3){
										cardOrder[i] = 0;
									}
								}
								
																
								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], swipeTime);
								iTween.MoveTo (cards [cardOrder [1]], cardPositions [1], swipeTime);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], 0);

								positionField.text = (visPosition + 1).ToString() + "/" + visualizationList.Count.ToString ();
								positionImage.fillAmount = (float)(visPosition+1)/(float)visualizationList.Count;
								// Redo the cards

								cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition + 1]];
								swipeGo ();

							}
						}
						if(textShowing){
							if (visPosition + 1 < visualizationList.Count) {

								visPosition++;
								
								for (int i = 0; i < 3; i++) {
									cardOrder[i]++;
									if (cardOrder[i] == 3){
										cardOrder[i] = 0;
									}
								}
								
								
								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], 0);
								iTween.MoveTo (cards [cardOrder [1]], hidePosition, 0);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], 0);

								positionImage.fillAmount = (float)(visPosition+1) / (float)visualizationList.Count;
								// Redo the cards
								if( visPosition + 1 <= visualizationList.Count -1 ) {
									cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition + 1]];
								}
								titleField.text = titles [visOrder [visPosition]];
								textField.text = descriptions [visOrder [visPosition]].Replace("; ", ";\n");
								typeField.text = typeNames[ types[visOrder[visPosition]] ];
								string _atlas = "<i>Atlas of Knowlege</i>, ";
								string _page_or_pages = "";
								
								if ( pages [ visOrder [visPosition]].IndexOf("and") != -1 ) {
									_page_or_pages = "pages ";
								}else{
									_page_or_pages = "page ";
								}
								
								pageField.text = _atlas + _page_or_pages + pages [ visOrder [visPosition]];
								if( adjustSource ) { 
									int count = descriptions [visOrder [visPosition]].Split (';').Length-1;
									(pageTitle.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageTitle.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
									(pageField.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageField.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
								}
								
							}
							else{

								ResetCards();
								visPosition = 0;
								cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition]];

								
								for (int i = 0; i < 3; i++) {
									cardOrder[i]++;
									if (cardOrder[i] == 3){
										cardOrder[i] = 0;
									}
								}
								
								
								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], 0);
								iTween.MoveTo (cards [cardOrder [1]], hidePosition, 0);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], 0);
								
								positionImage.fillAmount = (float)(visPosition+1) / (float)visualizationList.Count;
								// Redo the cards
								if( visPosition + 1 <= visualizationList.Count -1 ) {
									cards [cardOrder [2]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition + 1]];
								}
								titleField.text = titles [visOrder [visPosition]];
								textField.text = descriptions [visOrder [visPosition]].Replace("; ", ";\n");
								typeField.text = typeNames[ types[visOrder[visPosition]] ];
								string _atlas = "<i>Atlas of Knowlege</i>, ";
								string _page_or_pages = "";
								
								if ( pages [ visOrder [visPosition]].IndexOf("and") != -1 ) {
									_page_or_pages = "pages ";
								}else{
									_page_or_pages = "page ";
								}
								
								pageField.text = _atlas + _page_or_pages + pages [ visOrder [visPosition]];
								if( adjustSource ) { 
									int count = descriptions [visOrder [visPosition]].Split (';').Length-1;
									(pageTitle.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageTitle.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
									(pageField.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageField.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
								}

							}
						}
					}

				}

				// Swipe right
				if (swipe.x > Mathf.Abs (swipe.y)) {
					// Only activate swipe if we aren't zoomed in
					if (cameraSize == Camera.main.orthographicSize) {
						if (!textShowing && !isSwiping) {

							if (visPosition != 0) {
								iTween.Stop();
								isSwiping = true;
								Camera.main.orthographicSize = cameraSize;
								visPosition--;
								for (int i = 0; i < 3; i++) {
									cardOrder[i]--;
									if (cardOrder[i] == -1){
										cardOrder[i] = 2;
									}
								}
						

								iTween.MoveTo (cards [cardOrder [1]], cardPositions [1], swipeTime);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], swipeTime);
								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], 0);
								//iTween.MoveTo ( Camera.main.gameObject, cameraPosition, swipeTime * .75f );
								//positionField.text = (visPosition + 1).ToString() + "/" + visualizationList.Count.ToString ();
								positionImage.fillAmount = (float)(visPosition+1) / (float)visualizationList.Count;
								if(visPosition != 0) {
									cards [cardOrder [0]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition - 1]];
								}
								swipeGo ();
							}
						}
						else if (textShowing) {
							if (visPosition != 0) {
								iTween.Stop();
								//isSwiping = true;
								Camera.main.orthographicSize = cameraSize;
								visPosition--;
								for (int i = 0; i < 3; i++) {
									cardOrder[i]--;
									if (cardOrder[i] == -1){
										cardOrder[i] = 2;
									}
								}
								
								
								iTween.MoveTo (cards [cardOrder [1]], hidePosition, 0);
								iTween.MoveTo (cards [cardOrder [2]], cardPositions [2], 0);
								iTween.MoveTo (cards [cardOrder [0]], cardPositions [0], 0);

								positionImage.fillAmount = (float)(visPosition+1) / (float)visualizationList.Count;
								if(visPosition != 0) {
									cards [cardOrder [0]].GetComponent<SpriteRenderer> ().sprite = visualizationList [visOrder [visPosition - 1]];
								}
								titleField.text = titles [visOrder [visPosition]];
								textField.text = descriptions [visOrder [visPosition]].Replace("; ", ";\n");
								typeField.text = typeNames[ types[visOrder[visPosition]] ];
								string _atlas = "<i>Atlas of Knowlege</i>, ";
								string _page_or_pages = "";
								
								if ( pages [ visOrder [visPosition]].IndexOf("and") != -1 ) {
									_page_or_pages = "pages ";
								}else{
									_page_or_pages = "page ";
								}
								
								pageField.text = _atlas + _page_or_pages + pages [ visOrder [visPosition]];
								if( adjustSource ) { 
									int count = descriptions [visOrder [visPosition]].Split (';').Length-1;
									(pageTitle.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageTitle.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
									(pageField.gameObject.transform as RectTransform).anchoredPosition = new Vector2((pageField.gameObject.transform as RectTransform).anchoredPosition.x, encPositions[count].y);
								}
							}
						}
					}
				}
		

			}

		}
	}

	void swipeGo () {

		//cameraReturnSize ();
		Camera.main.orthographicSize = cameraSize;
		Camera.main.transform.position = cameraPosition;

		StartCoroutine(pauseForSwipe());
	}

	IEnumerator pauseForSwipe () {
		yield return new WaitForSeconds(swipeTime);
		isSwiping = false;
	}

	void cameraReturnSize () {
		iTween.ValueTo(Camera.main.gameObject,iTween.Hash("from",Camera.main.orthographicSize,"to", cameraSize, "time",swipeTime,"onupdate","onCameraSizeUpdate"));			
	}

	void onCameraSizeUpdate(float orthSize) {
		Camera.main.orthographicSize = orthSize;
	}
	
}
