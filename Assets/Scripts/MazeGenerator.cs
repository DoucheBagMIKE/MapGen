using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    // Blockwise growing tree algorithom.
    public static MapPos[] dirs = new MapPos[4] { new MapPos(-1, 0), new MapPos(0,1), new MapPos(1, 0), new MapPos(0, -1) };

    [HideInInspector]
    public MapGenerator mapGen;
    [HideInInspector]
    public MapData mapData;

    public bool[,] Visited;
    List<MapPos> Fringe;
    public List<MapPos> DeadEnds;
    public int RemoveDeadEndsIterations;
    public int DeadCells;

    [Range(0f,1f)]
    public float loopPercent;
    [Range(0f,1f)]
    public float windyOrRandomPercent;

    // Use this for initialization
    void Awake()
    {
        mapData = gameObject.GetComponent<MapData>();
        mapGen = gameObject.GetComponent<MapGenerator>();

        Visited = new bool[mapData.width, mapData.height];
        Fringe = new List<MapPos>();
    }

    public void Generate(int sx, int sy)
    {
        if (DeadEnds == null)
        {
            DeadEnds = new List<MapPos>();
        }
        else
        {
            DeadEnds.Clear();
        }
        
        List<MapPos> RoomPositions = new List<MapPos>();

        if (sx < 0 || sx > mapData.width - 1 || sy < 0 || sy > mapData.height - 1)
        {
            return;
        }
        if (mapData.Map[sx, sy] == 1)
        {
            return;
        }

        for (int i = 0; i < DeadCells; i++)
        {
            Visited[mapData.RandomEven(0, mapData.width - 1), mapData.RandomEven(0, mapData.height - 1)] = true;
        }

        MapPos start = new MapPos(sx, sy);
        Fringe.Add(start);

        while (Fringe.Count > 0)
        {
            MapPos selected = pickNextFringe();
            mapData.Map[selected.x, selected.y] = 1;
            Visited[selected.x, selected.y] = true;

            List<MapPos> adjacent = FindFringeNeighbors(selected, 0);
            if (adjacent.Count > 0)
            {
                MapPos adjPos = adjacent[mapData.Rng.Next(0, adjacent.Count - 1)];
                if (mapData.Rng.NextDouble() >= loopPercent)
                {
                    mapData.Map[adjPos.x, adjPos.y] = 1;
                }
                setPassage(selected, adjPos);
                Fringe.Add(adjPos);
            }
            else
            {
                Fringe.Remove(selected);
            }
        }

        for (int i = 0; i < RemoveDeadEndsIterations; i++)
        {
            Sparsify();
        }

        
        for (int x = 0; x < mapData.width; x = x + 2)
        {
            for (int y = 0; y < mapData.height; y = y + 2)
            {
                MapPos cPos = new MapPos(x, y);
                List<MapPos> roomdata = mapGen.getLargeRoomPositions(x, y);

                if (roomdata.Count == 4)
                {
                    bool isNotInMaze = true;

                    foreach (MapPos pos in roomdata)
                    {
                        if (RoomPositions.Contains(pos))
                        {
                            isNotInMaze = false;
                        }
                    }

                    if (isNotInMaze)
                    {
                        RoomPositions.AddRange(roomdata);
                        mapData.LargeRoomPositions.Add(cPos);
                    }
                }

                if (mapData.Map[x, y] == 1 && IsDeadEnd(x, y))
                {
                    if (RoomPositions.Contains(cPos))
                    {
                        continue;
                    }
                    DeadEnds.Add(cPos);
                }
            }
            
        }
        print("DeadEndCount : " + DeadEnds.Count.ToString());
    }

    MapPos pickNextFringe()
    {
        if (mapData.Rng.NextDouble() <= windyOrRandomPercent)
        {
            return Fringe[mapData.Rng.Next(0, Fringe.Count - 1)];
        }
        else
        {
            return Fringe[Fringe.Count - 1];
        }
        
    }

    public List<MapPos> FindFringeNeighbors(MapPos Pos, int TileState)
    {
        List<MapPos> ret = new List<MapPos>();

        foreach (MapPos index in dirs)
        {
            int x = Pos.x + (index.x * 2);
            int y = Pos.y + (index.y * 2);

            if (x < 0 || x > mapData.width - 1 || y < 0 || y > mapData.height - 1)
            {
                continue;
            }

            MapPos rPos = new MapPos(x, y);

            if (mapData.Map[x, y] == TileState && Visited[x,y] == false)
            {
                ret.Add(rPos);
            }
        }
        return ret;
    }

    void setPassage(MapPos a, MapPos b)
    {
        int x = (a.x + b.x) / 2;
        int y = (a.y + b.y) / 2;

        mapData.Map[x, y] = 1;
    }

    public void Sparsify()
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                if (mapData.Map[x, y] == 1)
                {
                    if (IsDeadEnd(x, y))
                    {
                        mapData.Map[x, y] = 0;
                        Visited[x, y] = false;
                    }

                }

            }
        }
    }
    public bool IsDeadEnd (int x, int y)
    {
        int count = 0;
        foreach (MapPos i in dirs)
        {
            int nx = i.x + x;
            int ny = i.y + y;

            if (nx < 0 || nx > mapData.width - 1 || ny < 0 || ny > mapData.height - 1)
            {
                count++;
                continue;
            }

            if (mapData.Map[nx, ny] == 0)
            {
                count++;
            }
        }
        if (count > 2)
        {
            return true;
        }
        return false;
    }

}