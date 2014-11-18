using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class TouchReceiver : MonoBehaviour
{
	//alt
	TuioCursor[] cursorArray;
	Vector3[] latestPosition;
	int arraySize = 10;
	
	//aktuell
	bool[] pressedArray;
	Vector3[] vectorPositionOld;
	Vector3[] vectorPositionUpdated;
	int numberSupportedCursors = 2;


	// Use this for initialization
	void Start()
	{
		cursorArray = new TuioCursor[arraySize];
		latestPosition = new Vector3[arraySize];
		
		pressedArray = new bool[arraySize];
		vectorPositionOld = new Vector3[numberSupportedCursors];
		vectorPositionUpdated = new Vector3[numberSupportedCursors];
	}
	
	// Update is called once per frame
	void Update()
	{
		for (int i = 0; i < numberSupportedCursors; i++){
			if (pressedArray[i]){
				Vector3 pos1 = vectorPositionOld[i];
				Vector3 pos2 = vectorPositionUpdated[i];
				Debug.Log("Positionen: (" + pos1.x + "|" + pos1.y + ") (" + pos2.x + "|" + pos2.y + ")");
			}
		}
//		for (int i = 0; i <arraySize;i++)
//		{
////			if (pressedArray[i])
////			{
////				//Debug.Log("TE: " + cursorArray[i].getCursorID() + ": pressed - (" + cursorArray[i].getX() + " ; " + cursorArray[i].getY() + ")");
////				if (!latestPosition[i].Equals(Vector3.zero)){
////					//Falls verschiebung erkennbar
////					Debug.Log("Zwei Werte, theoretisch verschiebbar");
////					Vector3 position = new Vector3(cursorArray[i].getX(), cursorArray[i].getY(), 1);
////					//aktion durchführen
////
////
////					latestPosition[i] = position;
////				} else {
////					//noch keine vorherige Position bekannt
////					Debug.Log("Nur ein Eintrag.");
////					latestPosition[i] = new Vector3(cursorArray[i].getX(), cursorArray[i].getY(), 1);
////				}
////			} else {
////				//Entferne latest Eintrag für ungepresste Cursor
////				latestPosition[i] = Vector3.zero;
////			}
//		}
	}

	void processEvents(ArrayList events)
	{
		int i = 0;
		foreach (BBCursorEvent cursorEvent in events)
		{
			TuioCursor mycursor = cursorEvent.cursor;
			i = mycursor.getCursorID();
			//mycursor.getCursorID get the ID
			//cursorEvent.state get the state

			cursorArray [i] = mycursor;



			if (cursorEvent.state.Equals(BBCursorState.Add)){
				pressedArray [i] = true;
				vectorPositionOld[i] = new Vector3(mycursor.getX(), mycursor.getY(), 1);
				vectorPositionUpdated[i] = new Vector3(mycursor.getX(), mycursor.getY(), 1);
			} else if (cursorEvent.state.Equals(BBCursorState.Remove)){
				pressedArray [i] = false;
			} else {
				vectorPositionUpdated[i] = new Vector3(mycursor.getX(), mycursor.getY(), 1);
				//Berechnung wohl erst im Update loop
				//Calculate new TransformationMatrix
				//nächste ausgangsposition auf neue position setzen
				//vectorPositionOld[i] = vectorPositionUpdated[i];

			}
		}
	}
	
	
}
