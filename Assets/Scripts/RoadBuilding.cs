using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoadBuilding : MonoBehaviour
{

    public GameObject StraightPiece;
    public GameObject CurvedPiece;
    public GameObject SPiece;
    public int RoadLength;
    public int StdLength = 3;

    public float RoadVerticalScale = 1.0f;

    CarDriverAgent cda;

    Vector3 previousEnd;
    int numLefts;

    // Start is called before the first frame update
    void Start()
    {
        numLefts = 0;

        //Set the start of the first track piece to be the origin of this training area
        previousEnd = transform.parent.position;

        cda = GetComponent<CarDriverAgent>();
    }


    // Build out the whole road
    public void CreateRoad()
    {
        GameObject currentPiece = null;
        System.Random r = new System.Random();

        // Make as many pieces are as specified in the component
        for (int i = 0; i < RoadLength; i++)
        {
            Vector3 piecePos;
            Vector3 endOnePos;
            Vector3 shiftBy;

            //Determine the rotation of the next piece based on which direction we are going
            Quaternion nextRot = Quaternion.identity;
            switch (numLefts)
            {
                case 0:
                    nextRot = Quaternion.Euler(0, 0, 0);
                    break;
                case 1:
                    nextRot = Quaternion.Euler(0, 270, 0);
                    break;
                case -1:
                    nextRot = Quaternion.Euler(0, 90, 0);
                    break;
            }


            // Choose a piece at random, biasing the straight pieces since they are shorter
            int randInt = r.Next(0, 6);
            switch (randInt)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    // Make the straight piece, scale it down vertically, match it to the previous end position, and set up for the next piece
                    currentPiece = Instantiate(StraightPiece, previousEnd, nextRot, gameObject.transform.parent.transform);
                    currentPiece.transform.localScale = new Vector3(1.0f, RoadVerticalScale, 1.0f);
                    piecePos = currentPiece.transform.position;
                    endOnePos = currentPiece.transform.Find("End1").transform.position;
                    shiftBy = piecePos - endOnePos;
                    currentPiece.transform.position = previousEnd + shiftBy;
                    previousEnd = currentPiece.transform.Find("End2").transform.position;
                    break;

                case 4:
                    Vector3 newScale = new Vector3(1.0f, RoadVerticalScale, 1.0f);
                    Quaternion newRotate = Quaternion.identity;

                    // Determine the direction of the curve based on which way we are already going
                    // Can only have one curve in one direction in a row
                    if (numLefts == 1)
                    {
                        newRotate = Quaternion.Euler(0.0f, 270.0f, 0.0f);
                        numLefts = 0;
                    }
                    else if (numLefts == -1)
                    {
                        newRotate = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                        newScale = new Vector3(-1.0f, RoadVerticalScale, 1.0f);
                        numLefts = 0;
                    }
                    else
                    {
                        int dirInt = r.Next(0, 2);
                        switch (dirInt)
                        {
                            case 0:
                                numLefts = -1;
                                break;
                            case 1:
                                newScale = new Vector3(-1.0f, RoadVerticalScale, 1.0f);
                                numLefts = 1;
                                break;
                        }
                    }

                    // Make the curve piece, scale it down vertically, match it to the previous end position, and set up for the next piece
                    currentPiece = Instantiate(CurvedPiece, gameObject.transform.parent.transform);
                    currentPiece.transform.localScale = newScale;
                    currentPiece.transform.rotation = newRotate;

                    piecePos = currentPiece.transform.position;
                    endOnePos = currentPiece.transform.Find("End1").position;
                    shiftBy = piecePos - endOnePos;
                    currentPiece.transform.position = previousEnd + shiftBy;
                    previousEnd = currentPiece.transform.Find("End2").transform.position;
                    break;
                case 5:
                    currentPiece = Instantiate(SPiece, previousEnd, nextRot, gameObject.transform.parent.transform);

                    // 50% chance to flip the direction of the S
                    int directionInt = r.Next(0, 2);
                    if (directionInt == 1)
                    {
                        currentPiece.transform.localScale = new Vector3(-1.0f, RoadVerticalScale, 1.0f);
                    }
                    else
                    {
                        currentPiece.transform.localScale = new Vector3(-1.0f, RoadVerticalScale, 1.0f);
                    }

                    // Make the S-Piece, scale it down vertically, match it to the previous end position, and set up for the next piece
                    piecePos = currentPiece.transform.position;
                    endOnePos = currentPiece.transform.Find("End1").position;
                    shiftBy = piecePos - endOnePos;
                    currentPiece.transform.position = previousEnd + shiftBy;
                    previousEnd = currentPiece.transform.Find("End2").transform.position;
                    break;
            }
        }

        // Tell the agent to drive to the last tile in the course
        cda.RegisterDrivePoint(currentPiece.transform.Find("DrivePoint"));
    }

    // Destroy the roads that are in the world and deregister the goal points
    public void DestroyRoad()
    {
        GameObject[] roads = GameObject.FindGameObjectsWithTag("Road");

        foreach (GameObject i in roads)
        {
            if (i.transform.parent.gameObject == transform.parent.gameObject)
            {
                Destroy(i);
            }
        }

        cda.RemoveDrivePoints();

        numLefts = 0;
        previousEnd = transform.parent.position;
    }
}
