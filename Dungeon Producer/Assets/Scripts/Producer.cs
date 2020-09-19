using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Producer : MonoBehaviour
{
    public int Rows, Columns;
    public int minSize, maxSize;
    public GameObject floorTile;
    public GameObject corridorTile;
    private GameObject[,] positionsFloor;


    public class SubDungeon
    {
        public SubDungeon left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0); 
        public List<Rect> corridors = new List<Rect>();


        //public int debugId;
        //private static int debugCounter = 0;

        public SubDungeon(Rect xrect)
        {
            rect = xrect;
            //debugId = debugCounter;
            //debugCounter++;
        }

        public void CreateRoom()
        {
            if (left != null || right != null){
                if (left != null)
                {
                    left.CreateRoom();
                }
                if (right != null)
                {
                    right.CreateRoom();
                }
                if(left != null && right != null){
                    CreateCorridor(left, right);
                }
            }

            if (isLeaf())
            {
                int width = (int)Random.Range(rect.width / 2, rect.width - 2);
                int height = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - width - 1);
                int roomY = (int)Random.Range(1, rect.height - height - 1);
                room = new Rect(rect.x + roomX, rect.y + roomY, width, height);
                //Debug.Log("Created room " + room + " in sub-dungeon " + debugId + " " + rect);
            }
        }

        public bool isLeaf()
        {
            return left == null && right == null;
        }

        public Rect findRoom()
        {
            if (isLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect leftroom = left.findRoom();
                if (!leftroom.x.Equals(-1))
                {
                    return leftroom;
                }
            }
            if (right != null)
            {
                Rect rightroom = right.findRoom();
                if (!rightroom.x.Equals(-1))
                {
                    return rightroom;
                }
            }

            return new Rect(-1, -1, 0, 0);
        }


        public void CreateCorridor(SubDungeon left, SubDungeon right)
        {
            Rect leftroom = left.findRoom();
            Rect rightroom = right.findRoom();

            //Debug.Log("Creating corridor(s) between " + left.debugId + "(" + leftroom + ") and " + right.debugId + " (" + rightroom + ")");

            //create the corridor to a random point in each room
            Vector2 leftpoint = new Vector2((int)Random.Range(leftroom.x + 1, leftroom.xMax - 1), (int)Random.Range(leftroom.y + 1, leftroom.yMax - 1));
            Vector2 rightpoint = new Vector2((int)Random.Range(rightroom.x + 1, rightroom.xMax - 1), (int)Random.Range(rightroom.y + 1, rightroom.yMax - 1));

            if (leftpoint.x > rightpoint.x)
            {
                Vector2 temp = leftpoint;
                leftpoint = rightpoint;
                rightpoint = temp;
            }

            int w = (int)(leftpoint.x - rightpoint.x);
            int h = (int)(leftpoint.y - rightpoint.y);

            //Debug.Log("lpoint: " + lpoint + ", rpoint: " + rpoint + ", w: " + w + ", h: " + h);

            // if the points are not horizontally
            if (w != 0)
            {
                // randomly choose to go horizontal then vertical or the opposite
                if (Random.Range(0, 1) > 2)
                {
                    // add a corridor to the right
                    corridors.Add(new Rect(leftpoint.x, leftpoint.y, Mathf.Abs(w) + 1, 1));

                    // if left point is below right point go up otherwise go down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(rightpoint.x, leftpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(rightpoint.x, leftpoint.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    // go up or down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(leftpoint.x, leftpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(leftpoint.x, rightpoint.y, 1, Mathf.Abs(h)));
                    }

                    // go right
                    corridors.Add(new Rect(leftpoint.x, rightpoint.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                // if the points are aligned horizontally go up or down 
                if (h < 0)
                {
                    corridors.Add(new Rect((int)leftpoint.x, (int)leftpoint.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)rightpoint.x, (int)rightpoint.y, 1, Mathf.Abs(h)));
                }
            }

            //Debug.Log("Corridors: ");
            //foreach (Rect corridor in corridors)
            //{
            //    Debug.Log("corridor: " + corridor);
            //}
        }


        public bool Split(int minSize, int maxSize)
        {
            if (!isLeaf())
            {
                return false;
            }

            // choose a vertical or horizontal split
            bool splitH;
            if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minSize)
            {
                //Debug.Log("Sub-dungeon " + debugId + " will be a leaf");
                return false;
            }

            if (splitH)
            {
                // split so that the resulting sub-dungeons widths are not too small (split horizontally)
                int split = Random.Range(minSize, (int)(rect.width - minSize));
                left = new SubDungeon(new Rect(rect.x, rect.y, rect.width, split));
                right = new SubDungeon(
                  new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                // split so that the resulting sub-dungeons widths are not too small (split vertically)
                int split = Random.Range(minSize, (int)(rect.height - minSize));
                left = new SubDungeon(new Rect(rect.x, rect.y, split, rect.height));
                right = new SubDungeon(
                  new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }

            return true;
        }
    }




    public void CreateBSP(SubDungeon subDungeon)
    {
        //Debug.Log("Splitting sub-dungeon " + subDungeon.debugId + ": " + subDungeon.rect);
        if (subDungeon.isLeaf())
        {
            // if the sub-dungeon is too large
            if (subDungeon.rect.width > maxSize || subDungeon.rect.height > maxSize || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (subDungeon.Split(minSize, maxSize))
                {
                    //Debug.Log("Splitted sub-dungeon " + subDungeon.debugId + " in "
                      //+ subDungeon.left.debugId + ": " + subDungeon.left.rect + ", "
                      //+ subDungeon.right.debugId + ": " + subDungeon.right.rect);
                    CreateBSP(subDungeon.left);
                    CreateBSP(subDungeon.right);
                }
            }
        }
    }


    public void DrawRooms(SubDungeon subDungeon)
    {
        if (subDungeon == null)
        {
            return;
        }
        if (subDungeon.isLeaf())
        {
            for (int i = (int)subDungeon.room.x; i < subDungeon.room.xMax; i++)
            {
                for (int j = (int)subDungeon.room.y; j < subDungeon.room.yMax; j++)
                {
                    GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                    positionsFloor[i, j] = instance;
                }
            }
        }
        else
        {
            DrawRooms(subDungeon.left);
            DrawRooms(subDungeon.right);
        }
    }

    void DrawCorridors(SubDungeon subDungeon)
    {
        if (subDungeon == null)
        {
            return;
        }
        DrawCorridors(subDungeon.left);
        DrawCorridors(subDungeon.right);
        foreach (Rect corridor in subDungeon.corridors)
        {
            for (int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for (int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                    if (positionsFloor[i, j] == null)
                    {
                        GameObject instance = Instantiate(corridorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(transform);
                        positionsFloor[i, j] = instance;
                    }
                }
            }
        }
    }

    void Start()
    {
        SubDungeon rootSubDungeon = new SubDungeon(new Rect(0, 0, Rows, Columns));
        CreateBSP(rootSubDungeon);
        rootSubDungeon.CreateRoom();
        positionsFloor = new GameObject[Rows, Columns];
        DrawRooms(rootSubDungeon);
        DrawCorridors(rootSubDungeon);


    }
}
