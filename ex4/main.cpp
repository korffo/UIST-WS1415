#include "opencv/cvaux.h"
#include "opencv/highgui.h"
#include "opencv/cxcore.h"
#include <stdio.h>
#include <math.h>

//--------------------------------------------------------------------------------------
//--------------------------------- Round Blob Detector --------------------------------
//--------------------------------------------------------------------------------------


int main(int argc, char* argv[])
{
	//Standardkalibrierung durchf�hren f�r �ffnugnsiwnkel
	//provisorisch festgelegter Winkel
	double apertureAngle = 60.0;

	//General Settings
	//Size of the tracked Object in mm (Coladeckel in rot, gesch�tzt)
	double trackedObjectSize = 31.4;
	


    // Create camera capture
    CvCapture* camera = cvCreateCameraCapture(0); // Use camera 0
    
	double h = cvGetCaptureProperty(camera, CV_CAP_PROP_FRAME_HEIGHT);
    double w = cvGetCaptureProperty(camera, CV_CAP_PROP_FRAME_WIDTH);
 
    CvMemStorage* storage = cvCreateMemStorage(0); // needed for Hough circles
    CvSize size = cvSize(w,h);
    IplImage* hsv_frame    = cvCreateImage(size, IPL_DEPTH_8U, 3);
    IplImage* thresholded  = cvCreateImage(size, IPL_DEPTH_8U, 1);
    IplImage* thresholded2 = cvCreateImage(size, IPL_DEPTH_8U, 1);
 
    // Define thresholds
    CvScalar hsv_min = cvScalar(0, 140, 95, 0);
    CvScalar hsv_max = cvScalar(10, 256, 256, 0);
    CvScalar hsv_min2 = cvScalar(169, 140, 95, 0);
    CvScalar hsv_max2 = cvScalar(179, 256, 256, 0);
     
    // Create windows
    cvNamedWindow("window", 1);
    cvNamedWindow("window2", 1);
    cvNamedWindow("window3", 1);
    

	while (true) {
        // Read new camera frame
        IplImage* frame = cvQueryFrame(camera);
        if (frame == NULL) {
            continue;
        }
 
        // color detection using HSV
        cvCvtColor(frame, hsv_frame, CV_BGR2HSV);
        
        // to handle color wrap-around, two halves are detected and combined
        cvInRangeS(hsv_frame, hsv_min, hsv_max, thresholded);
        cvInRangeS(hsv_frame, hsv_min2, hsv_max2, thresholded2);
        cvOr(thresholded, thresholded2, thresholded);
 
        // Display image in window
        cvShowImage("window", thresholded);
        cvWaitKey(1);

 
        // hough detector works better with some smoothing of the image
        cvSmooth( thresholded, thresholded, CV_GAUSSIAN, 9, 9 );
        CvSeq* circles = cvHoughCircles(thresholded, storage, CV_HOUGH_GRADIENT, 2, thresholded->height/2, 100, 50, 20, 200);
        cvShowImage("window3", thresholded);

        for (int i = 0; i < circles->total; i++)
        {
			float* p = (float*)cvGetSeqElem( circles, i );
            printf("Ball(%d)! x=%f y=%f r=%f\n\r",i,p[0],p[1],p[2]);
            cvCircle( frame, cvPoint(cvRound(p[0]),cvRound(p[1])),
                     3, CV_RGB(0,255,0), -1, 8, 0 );
            cvCircle( frame, cvPoint(cvRound(p[0]),cvRound(p[1])),
                     cvRound(p[2]), CV_RGB(255,0,0), 3, 8, 0 );
			
			
			
			//Berechnungen
			float erkannterDurchmesser = p[2]*2;
			float bildBreiteInPx = w;
			float bildHoeheInPx = h;
			float positionX = p[0];
			float positionY = p[1];

	
			//float angle = atan2(erkannterRadius,bildBreiteInPx/2);
			//verh�ltnis zwischen erkannter Objektgr��e und Brennweite herstellen durch "3-satz"
			float tmp2 = ((bildBreiteInPx/2) / erkannterDurchmesser);
			//Faktor auf den �ffnungswinkel anwenden um neuen winkel zu erhalten
			float angle = (apertureAngle/2) / tmp2;

			//Faktor der die Erkannte und Tats�chliche Gr��e ins Verh�ltnis setzt
			float pixelToMM = trackedObjectSize / erkannterDurchmesser;

			float distance = trackedObjectSize/tan(angle*(3.14159 /180));
			float widthFromCameraCenter = ((bildBreiteInPx/2)-positionX)*pixelToMM;
			float heightFromCameraCenter = ((bildHoeheInPx/2)-positionY)*pixelToMM;
//			printf("Bild %f, position %f",bildHoeheInPx, positionY);

			printf("(%d) X=%fmm - Y=%fmm - Entfernung=%fmm\n(Ursprung = Kamera)\n\r", i, widthFromCameraCenter, heightFromCameraCenter, distance);
		}

       // Display image in window
       cvShowImage("window2", frame);
       cvWaitKey(1);
    }
    
    cvReleaseCapture(&camera);
    return 0;
}