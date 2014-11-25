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
	
	private float rotWinkel = 0;

	private Mesh mesh;
	private Vector3 vertexCenter;
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
//		Debug.Log("vertices alt: ");
//		foreach (var item in vertices)
//		{
//			Debug.Log("----: " + item);
//		}

		//rotWinkel++;
		//rotWinkel = rotWinkel%360;
		Vector3 worldCoords = cam.ScreenToWorldPoint(vectorPositionOld[0]);
		
		//		Debug.Log("Transformations:Matrix: " + translationMatrix.ToString());
		//		Debug.Log("Transformations:Position: " + gameObject.rigidbody.position);
		//		Debug.Log("Koordinaten: " + translationMatrix.MultiplyVector(new Vector3()));

		//TODO:Move to Center for Rotation, Mittelpunkt immer Aktuell in Vector3 vertexCenter
		// -> Verschiebung um vertexCenter, rotation, zurückSchieben?

		
		//Transformationen immer auf Basis der Ursprungsposition (in diesem Fall 0,0,0) durchführen
		vertexCenter = Vector3.zero;
		int j = 0;
		while (j < vertices.Length) {
			vertices[j] = transformationMatrix.MultiplyPoint(verticesStart[j]);
			vertexCenter.x += vertices[j].x;
			vertexCenter.y += vertices[j].y;
			j++;
		}
		vertexCenter.x = vertexCenter.x / vertices.Length;
		vertexCenter.y = vertexCenter.y / vertices.Length;
		Debug.Log("Middle: (" + vertexCenter.x + "|" + vertexCenter.y + ")");
		//	Debug.Log("Transformationsmatrix: " + transformationMatrix);
		
		//Debug.Log("Positionen: (" + pos1.x + "|" + pos1.y + ") (" + pos2.x + "|" + pos2.y + ")");
		
		mesh.vertices = vertices;
//		Debug.Log("vertices neu: ");
//		foreach (var item in vertices)
//		{
//			Debug.Log("----: " + item);
//		}
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		//Matritzen Zurücksetzen, Neue Ausgangsposition Setzen
		if (!pressedArray [0])
		{
			scalingMatrix = Matrix4x4.identity;
			rotationMatrix = Matrix4x4.identity;
			translationMatrix = Matrix4x4.identity;
			transformationMatrix = Matrix4x4.identity;
			//vectorPositionOld [0] = vectorPositionUpdated [0];
			//vectorPositionOld [1] = vectorPositionUpdated [1];
			
			verticesStart = (Vector3[]) vertices.Clone();
		}
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
				//vectorPositionUpdated[i] = new Vector4(mycursor.getX(), mycursor.getY(), 1,0);
				//Berechnung wohl erst im Update loop
				//Calculate new TransformationMatrix
				//nächste ausgangsposition auf neue position setzen
				//vectorPositionOld[i] = vectorPositionUpdated[i];
				
				//Translationsmatrix berechnen:
				if (i == 0) {
					//Verschiebung bestimmen
					Vector3 worldCoordsOld = cam.ScreenToWorldPoint(vectorPositionOld[i]);
					Vector3 worldCoordsNew = cam.ScreenToWorldPoint(vectorPositionUpdated[i]);
					Vector3 worldCoords = worldCoordsNew - worldCoordsOld;
				
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
					//Annahme: Drehung um Fixpunkt Old[0]
					Vector3 richtungsVektor1 = vectorPositionOld[0]-vectorPositionOld[1];
					Vector3 richtungsVektor2 = vectorPositionUpdated[0]-vectorPositionUpdated[1];
					//rotWinkel = Vector3.Angle(vectorPositionUpdated[0],vectorPositionUpdated[1]);
					rotWinkel = Vector3.Angle(richtungsVektor1,richtungsVektor2);

					//Winkel in Rad benötigt
					float rotCalculated = rotWinkel * Mathf.PI / 180;
					//Debug.Log("Winkel: " + rotWinkel+"- Offset: " +rotWinkelOffset + "- Berechnet: " + rotCalculated);


					rotationMatrix = new Matrix4x4();
					rotationMatrix.SetRow(0,new Vector4(Mathf.Cos(rotCalculated),-Mathf.Sin(rotCalculated),0,0));
					rotationMatrix.SetRow(1,new Vector4(Mathf.Sin(rotCalculated), Mathf.Cos(rotCalculated),0,0));
					rotationMatrix.SetRow(2,new Vector4(0,0,1,0));
					rotationMatrix.SetRow(3,new Vector4(0,0,0,1));

					//Abstände berechnen
					float d1 = Vector3.Distance(vectorPositionOld[0], vectorPositionOld[1]);
					float d2 = Vector3.Distance(vectorPositionUpdated[0], vectorPositionUpdated[1]);
					//Debug.Log("Abstände: - alt "+d1+" - neu "+d2);
					scalingMatrix = new Matrix4x4();
					scalingMatrix.SetRow(0,new Vector4(d2/d1,0,0,0));
					scalingMatrix.SetRow(1,new Vector4(0,d2/d1,0,0));
					scalingMatrix.SetRow(2,new Vector4(0,0,1,0));
					scalingMatrix.SetRow(3,new Vector4(0,0,0,1));
				}
			}
			//Debug.Log("Positionen: Old ("+vectorPositionOld[i].x+"|"+vectorPositionOld[i].y+") - Updated ("+vectorPositionUpdated[i].x+"|"+vectorPositionUpdated[i].y+")");
		} //end for
		
		//TransformationsMatrix aufbauen
		transformationMatrix = scalingMatrix * rotationMatrix * translationMatrix;


		//TODO: Rotationsmatrix außenvor lassen, da rotation auf der Stelle seperat erfolgt
		//Oder evtl. TranslationIn0 * Rotation * TranslationInUrsprungsPosition?
		//transformationMatrix = scalingMatrix * translationMatrix;
	}
}
