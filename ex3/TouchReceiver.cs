using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class TouchReceiver : MonoBehaviour
{	
	bool[] pressedArray;
	Vector4[] vectorPositionOld;
	Vector4[] vectorPositionUpdated;
	int numberSupportedCursors = 2;
	public Camera cam;
	
	//Transformationsmatrizen:
	Matrix4x4 transformationMatrix = Matrix4x4.identity;
	
	Matrix4x4 translationMatrix = Matrix4x4.identity;
	Matrix4x4 rotationMatrix = Matrix4x4.identity;
	Matrix4x4 scalingMatrix = Matrix4x4.identity;
	
	public float rotWinkel = 12.0f;
	
	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] verticesStart;
	
	
	// Use this for initialization
	void Start()
	{
		pressedArray = new bool[numberSupportedCursors];
		vectorPositionOld = new Vector4[numberSupportedCursors];
		vectorPositionUpdated = new Vector4[numberSupportedCursors];
	
		mesh = GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		//Erstellen einer Festen Kopie der Ausgangspositionen, damit es
		//bei mehrmaliger Anwendung nicht zu weiteren Verschiebungen kommt
		verticesStart = (Vector3[]) vertices.Clone();
	}
	
	// Update is called once per frame
	//Hier nur Transformations-Matrix anwenden.
	void Update()
	{

		Vector3 worldCoords = cam.ScreenToWorldPoint(vectorPositionOld[0]);
		
		//		Debug.Log("Transformations:Matrix: " + translationMatrix.ToString());
		//		Debug.Log("Transformations:Position: " + gameObject.rigidbody.position);
		//		Debug.Log("Koordinaten: " + translationMatrix.MultiplyVector(new Vector3()));

		//Transformationen immer auf Basis der Ursprungsposition (in diesem Fall 0,0,0) durchführen
		int j = 0;
		while (j < vertices.Length) {
			vertices[j] = transformationMatrix.MultiplyPoint(verticesStart[j]);
			j++;
		}
		
		Debug.Log("Transformationsmatrix: " + transformationMatrix);
		
		//Debug.Log("Positionen: (" + pos1.x + "|" + pos1.y + ") (" + pos2.x + "|" + pos2.y + ")");
		
		mesh.vertices = vertices;
		Debug.Log("vertices: ");
		foreach (var item in vertices)
		{
			Debug.Log("----: " + item);
		}
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
	
	/**
	 * Event-Routine für jedes TUIO-Event.
	 * Hier nur Transformations-Matrix berechnen.
	 */
	void processEvents(ArrayList events)
	{
		int i = 0;
		foreach (BBCursorEvent cursorEvent in events)
		{
			TuioCursor mycursor = cursorEvent.cursor;
			//i ist ID des Finger
			i = mycursor.getCursorID();
			
			//TUIO-Event für neuen Touch:
			if (cursorEvent.state.Equals(BBCursorState.Add)){
				pressedArray [i] = true;
				vectorPositionOld[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);
				vectorPositionUpdated[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);
				//TUIO-Event für Touch-Auflösung:
			} else if (cursorEvent.state.Equals(BBCursorState.Remove)){
				pressedArray [i] = false;
				//TUIO-Event für Position-Update:
			} else {
				vectorPositionUpdated[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);
				//Berechnung wohl erst im Update loop
				//Calculate new TransformationMatrix
				//nächste ausgangsposition auf neue position setzen
				//vectorPositionOld[i] = vectorPositionUpdated[i];
				
				//Translationsmatrix berechnen:
				if (i == 0) {
					Vector3 worldCoords = cam.ScreenToWorldPoint(vectorPositionUpdated[i]);
					//				Debug.Log("TUIOCoords: " + vectorPositionUpdated[i]);
					translationMatrix = new Matrix4x4();
					translationMatrix.SetRow(0,new Vector4(1,0,0,worldCoords.x));
					translationMatrix.SetRow(1,new Vector4(0,1,0,worldCoords.y));
					translationMatrix.SetRow(2,new Vector4(0,0,1,0));
					translationMatrix.SetRow(3,new Vector4(0,0,0,1));
					//Debug.Log("Transformations:Matrix: " + translationMatrix.ToString());
				}
				
				//Rotationsmatrix berechnen:
				if (pressedArray[0] && pressedArray[1]) {
					
					//Winkel berchnen: ??
					
					//				Debug.Log("TUIOCoords: " + vectorPositionUpdated[i]);
					rotationMatrix = new Matrix4x4();
					rotationMatrix.SetRow(0,new Vector4(Mathf.Cos(rotWinkel),-Mathf.Sin(rotWinkel),0,0));
					rotationMatrix.SetRow(1,new Vector4(Mathf.Sin(rotWinkel), Mathf.Cos(rotWinkel),0,0));
					rotationMatrix.SetRow(2,new Vector4(0,0,1,0));
					rotationMatrix.SetRow(3,new Vector4(0,0,0,1));
				}
			}
		} //end for
		
		//TransformationsMatrix aufbauen
		transformationMatrix = translationMatrix; //rotationMatrix;//* translationMatrix;
		
		
	}
}
