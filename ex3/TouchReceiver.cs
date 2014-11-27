using UnityEngine;
using System.Collections;
using TUIO;
using System.Collections.Generic;

public class TouchReceiver : MonoBehaviour
{	
	bool[] pressedArray;
	Vector4[] fingerPositionOld;
	Vector4[] fingerPositionUpdated;
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
		fingerPositionOld = new Vector4[numberSupportedCursors];
		fingerPositionUpdated = new Vector4[numberSupportedCursors];
	
		mesh = GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;

		//Erstellen einer festen Kopie der Ausgangspositionen, damit es
		//bei mehrmaliger Anwendung nicht zu weiteren Verschiebungen kommt
		verticesStart = (Vector3[]) vertices.Clone();
	    vertexCenter = Vector3.zero;
	}
	
	/**
	 * Update-Routine.
	 * Hier  Transformations-Matrix anwenden.
	 */
	void Update()
	{
		Vector3 worldCoords = cam.ScreenToWorldPoint(fingerPositionOld[0]);
	
		//Transformation anwenden:
		int j = 0;
		while (j < vertices.Length) {
			vertices[j] = transformationMatrix.MultiplyPoint(verticesStart[j]);
			j++;
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		//Matritzen Zurücksetzen, Neue Ausgangsposition Setzen
		if (!pressedArray [0] || !pressedArray [1])
		{
			scalingMatrix = Matrix4x4.identity;
			rotationMatrix = Matrix4x4.identity;
			translationMatrix = Matrix4x4.identity;
			transformationMatrix = Matrix4x4.identity;
			
			verticesStart = (Vector3[]) vertices.Clone();

			fingerPositionOld[0] = fingerPositionUpdated[0];

			//Center von Vertex neu setzen:
			vertexCenter = Vector3.zero;
			int i = 0;
			while (i < vertices.Length) {
				vertexCenter.x += vertices[i].x;
				vertexCenter.y += vertices[i].y;
				i++;
			}
			vertexCenter.x = vertexCenter.x / vertices.Length;
			vertexCenter.y = vertexCenter.y / vertices.Length;
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
			//Curser Anzahl max:
		 	if (i <= numberSupportedCursors-1) {
			
				//TUIO-Event für neuen Touch:
				if (cursorEvent.state.Equals(BBCursorState.Add)){
					pressedArray [i] = true;
					fingerPositionOld[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);
					fingerPositionUpdated[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);
					//TUIO-Event für Touch-Auflösung:
				} else if (cursorEvent.state.Equals(BBCursorState.Remove)){
					pressedArray [i] = false;
					//TUIO-Event für Position-Update:
				} else {
					fingerPositionUpdated[i] = new Vector4(mycursor.getX()*Screen.width, Screen.height-mycursor.getY()*Screen.height, 1,0);

					//Translationsmatrix berechnen:
					if (i == 0) {
						//Verschiebung bestimmen
						Vector3 worldCoordsOld = cam.ScreenToWorldPoint(fingerPositionOld[i]);
						Vector3 worldCoordsNew = cam.ScreenToWorldPoint(fingerPositionUpdated[i]);
						Vector3 worldCoords = worldCoordsNew - worldCoordsOld;
					
						translationMatrix.SetRow(0,new Vector4(1,0,0,worldCoords.x));
						translationMatrix.SetRow(1,new Vector4(0,1,0,worldCoords.y));
						translationMatrix.SetRow(2,new Vector4(0,0,1,0));
						translationMatrix.SetRow(3,new Vector4(0,0,0,1));

						translationMatrix = processTransformationInCenter(translationMatrix);
					}
					
					//Rotationsmatrix berechnen:
					if (pressedArray[0] && pressedArray[1]) {
						
						//Winkel berchnen:
						Vector3 richtungsVektor1 = fingerPositionOld[0]-fingerPositionOld[1];
						Vector3 richtungsVektor2 = fingerPositionUpdated[0]-fingerPositionUpdated[1];
						rotWinkel = AngleSigned(richtungsVektor1,richtungsVektor2);

						//Winkel in Rad benötigt
						float rotCalculated = rotWinkel * Mathf.PI / 180;
						//Debug.Log("Winkel: " + rotWinkel+ "- Berechnet: " + rotCalculated);

						rotationMatrix = new Matrix4x4();
						rotationMatrix.SetRow(0,new Vector4(Mathf.Cos(rotCalculated),-Mathf.Sin(rotCalculated),0,0));
						rotationMatrix.SetRow(1,new Vector4(Mathf.Sin(rotCalculated), Mathf.Cos(rotCalculated),0,0));
						rotationMatrix.SetRow(2,new Vector4(0,0,1,0));
						rotationMatrix.SetRow(3,new Vector4(0,0,0,1));

						rotationMatrix = processTransformationInCenter(rotationMatrix);

						//Abstände berechnen
						float d1 = Vector3.Distance(fingerPositionOld[0], fingerPositionOld[1]);
						float d2 = Vector3.Distance(fingerPositionUpdated[0], fingerPositionUpdated[1]);

						scalingMatrix = new Matrix4x4();
						scalingMatrix.SetRow(0,new Vector4(d2/d1,0,0,0));
						scalingMatrix.SetRow(1,new Vector4(0,d2/d1,0,0));
						scalingMatrix.SetRow(2,new Vector4(0,0,1,0));
						scalingMatrix.SetRow(3,new Vector4(0,0,0,1));

						scalingMatrix = processTransformationInCenter(scalingMatrix);
					}
				} 
			} //end if - Curser Anzahl
		} //end for - Event-Loop

		//TransformationsMatrix aufbauen
		transformationMatrix = scalingMatrix * rotationMatrix * translationMatrix;

	}


	/** 
	 * Transformationen müssen im Ursprung erfolgen. Deswegen zunächst Transation in den Ursprung.
	 * */
	private Matrix4x4 processTransformationInCenter(Matrix4x4 transformation ) {

		Matrix4x4 translationMatrixToCenter = new Matrix4x4();
		translationMatrixToCenter.SetRow(0,new Vector4(1,0,0,vertexCenter.x));
		translationMatrixToCenter.SetRow(1,new Vector4(0,1,0,vertexCenter.y));
		translationMatrixToCenter.SetRow(2,new Vector4(0,0,1,0));
		translationMatrixToCenter.SetRow(3,new Vector4(0,0,0,1));

		transformation = translationMatrixToCenter * transformation;

		//Translation umkehren:
		translationMatrixToCenter.SetRow(0,new Vector4(1,0,0,-vertexCenter.x));
		translationMatrixToCenter.SetRow(1,new Vector4(0,1,0,-vertexCenter.y));
	
		return (transformation * translationMatrixToCenter);
	}

	/**
	 * Winkelberechnung.
	 */
	private float AngleSigned(Vector3 v1, Vector3 v2) {

		Vector3 normale = new Vector3(0,0,1);
		return 
			Mathf.Atan2( Vector3.Dot(normale, Vector3.Cross(v1,v2)), Vector2.Dot(v1, v2)) * Mathf.Rad2Deg;
	}

}
