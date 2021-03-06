﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;



public class MapGenerator : MonoBehaviour {
    public enum GenerateType {Maze_Structure, Prefab_Maze};

    public GenerateType GenType;
    [HideInInspector]
    public MapData mapData;
    [HideInInspector]
    MazeGenerator mazeGen;

    public int[,] Map;

    public List<string> _32x32;
    public List<string> _64x64;
    List<GameObject> Items;
    private Dictionary<string, MapPos> _DoorRoomLookUp;
    public string[] dirs;

    MapPos StartPos = new MapPos(-1, -1);

    [Range(0, 10)]
    public int MaxLargeRooms;

    bool GotFiles = false;
    int mapsGenerated;

    public TilesetData MapConfig;

    void Awake ()
    {
        mapData = gameObject.GetComponent<MapData>();
        mazeGen = gameObject.GetComponent<MazeGenerator>();

        _DoorRoomLookUp = new Dictionary<string, MapPos>();
        _DoorRoomLookUp["Door_Bottom_Left"] = new MapPos(0,  -1);
        _DoorRoomLookUp["Door_Left_Bottom"] = new MapPos( -1, 0);
        _DoorRoomLookUp["Door_Bottom_Right"] = new MapPos(2,  -1);
        _DoorRoomLookUp["Door_Right_Bottom"] = new MapPos(3, 0);
        _DoorRoomLookUp["Door_Top_Left"] = new MapPos(0, 3);
        _DoorRoomLookUp["Door_Left_Top"] = new MapPos(-1, 2);
        _DoorRoomLookUp["Door_Top_Right"] = new MapPos(2, 3);
        _DoorRoomLookUp["Door_Right_Top"] = new MapPos(3, 2);

        dirs = new string[4] { "Up", "Down", "Left", "Right" };

        _32x32 = new List<string>();
        _64x64 = new List<string>();
        Items = new List<GameObject>();

        mapsGenerated = 0;
        
        
    }

    void Start()
    {

        switch(GenType)
        {
            case GenerateType.Prefab_Maze:
                GenPrefabMaze();
                break;
        }

    }

    void Update()
    {
        if(Input.GetAxis("Fire1") != 0)
        {
            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount;i++)
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
            mapData.Map = new int[mapData.width, mapData.height];
            mazeGen.Visited = new bool[mapData.width, mapData.height];
            GenPrefabMaze();
        }
    }

    public List<MapPos> getLargeRoomPositions (int x, int y)
    {
        Queue<MapPos> fringe = new Queue<MapPos>();

        int roomCount = 0;
        fringe.Enqueue(new MapPos(x, y));
        List<MapPos> visited = new List<MapPos>();
        List<MapPos> roomPositions = new List<MapPos>();
        int nx;
        int ny;
        MapPos nPos;

        while (fringe.Count != 0)
        {

            MapPos cPos = fringe.Dequeue();
            visited.Add(cPos);

            if (cPos.x < x || cPos.x > x + 3 || cPos.y < y || cPos.y > y + 3)
            {
                continue;
            }
            if (cPos.x % 2 == 0 && cPos.y % 2 == 0)
            {
                roomCount++;
                roomPositions.Add(cPos);
            }
            foreach (MapPos dir in MazeGenerator.dirs)
            {
                nx = dir.x + cPos.x;
                ny = dir.y + cPos.y;
                nPos = new MapPos(nx, ny);
                if (nx < 0 | nx > mapData.width - 1 || ny < 0 || ny > mapData.height - 1)
                {
                    continue;
                }
                if (!visited.Contains(nPos) && mapData.Map[nx, ny] == 1 && !fringe.Contains(nPos))
                {
                    fringe.Enqueue(nPos);
                }
            }
        }
        return roomPositions;
        
    }
    void SetDoors(int x, int y, DoorManager doorManager, bool isLargeRoom)
    {
        
        if (isLargeRoom == true)
        {          

            foreach (Door door in doorManager.doors)
            {
                foreach(string key in _DoorRoomLookUp.Keys)
                {
                    if (door.gameObject.name.Contains(key) == true)
                    {
                        int dx = _DoorRoomLookUp[key].x + x;
                        int dy = _DoorRoomLookUp[key].y + y;
                        if ((dx < 0 || dx > mapData.width - 1 || dy < 0 || dy > mapData.height - 1 ) || mapData.Map[dx,dy] == 0)
                        {
                            door.gameObject.SetActive(true);
                        }
                    }
                }

            }
        }
        else
        {
            foreach (string dir in dirs)
            {
                int nx = x;
                int ny = y;

                switch (dir)
                {
                    case "Up":
                        ny = y + 1;
                        break;
                    case "Down":
                        ny = y - 1;
                        break;
                    case "Right":
                        nx = x + 1;
                        break;
                    case "Left":
                        nx = x - 1;
                        break;
                }

                if ((nx < 0 || nx > mapData.width - 1 || ny < 0 || ny > mapData.height - 1) || mapData.Map[nx, ny] == 0)
                {
                    //get doors with Facing dir
                    for (int i = 0; i < doorManager.doors.Count; i++)
                    {
                        if (doorManager.doors[i].facing == dir)
                        {
                            doorManager.doors[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
    GameObject GenRoom(int x, int y, string chunkName, bool isLargeRoom, MapPos mapPos)
    {
        GameObject go = (GameObject)Instantiate(Resources.Load(chunkName));
        DoorManager doorManager = go.GetComponent<DoorManager>();

        go.transform.position = new Vector3(x, y, 0);
        go.transform.parent = gameObject.transform;
        SetDoors(mapPos.x, mapPos.y, doorManager, isLargeRoom);

        Transform Spawners = go.transform.FindChild("Spawners");

        for (int i = 0; i < Spawners.childCount; i++)
        {
            Transform child = Spawners.GetChild(i);
            if (child.name == "ItemSpawner" && Items.Count != 0)
            {
                GenItem((int)child.position.x, (int)child.position.y, Items[mapData.Rng.Next(0, Items.Count)], child);
            }
            
        }
        return go;
    }
    GameObject GenItem(int x, int y, GameObject prefab, Transform spawner)
    {
        GameObject go = (GameObject)Instantiate(prefab);
        go.transform.position = new Vector3(x, y, 0);
        go.transform.parent = spawner.parent.parent;
        return go;
    }
    public List<MapPos> GetAllLargeRoomPos ()
    {
        List<MapPos> ret = new List<MapPos>();
        List<MapPos> visited = new List<MapPos>();
        for (int y = 0; y < mapData.height; y = y + 2)
        {
            for (int x = 0; x < mapData.width; x = x + 2)
            {
                List<MapPos> roomdata = getLargeRoomPositions(x, y);

                if (roomdata.Count == 4)
                {
                    bool isNotInMaze = true;

                    foreach (MapPos pos in roomdata)
                    {
                        if (visited.Contains(pos))
                        {
                            isNotInMaze = false;
                        }
                    }

                    if (isNotInMaze)
                    {
                        ret.Add(new MapPos(x, y));
                    }

                }

            }
        }

        return ret;
    }
    void GenPrefabMaze()
    {
        if (MapConfig != null)
        {
            mazeGen.RemoveDeadEndsIterations = MapConfig.RemoveDeadEndsIterations;
            mazeGen.DeadCells = MapConfig.DeadCells;
            mazeGen.loopPercent = MapConfig.loopPercent;
            mazeGen.windyOrRandomPercent = MapConfig.windyOrRandomPercent;
            mazeGen.Visited = new bool[MapConfig.width, MapConfig.height];

            mapData.width = MapConfig.width;
            mapData.height = MapConfig.height;
            mapData.Map = new int[MapConfig.width, MapConfig.height];

            MaxLargeRooms = MapConfig.MaxLargeRooms;
            Items = MapConfig.Items;
        }

        mapsGenerated++;

        if (mapData.LargeRoomPositions == null)
        {
            mapData.LargeRoomPositions = new List<MapPos>();
        }
        else
        {
            mapData.LargeRoomPositions.Clear();
        }

        getFiles();
        
        mazeGen.Generate(mapData.RandomEven(0, mapData.width - 1), mapData.RandomEven(0, mapData.height - 1));
        InitLargeRooms ();
        CutExcessLargeRooms();
        
        // dead ends are a problem in maps that can contain nothing but loops. (StackOverflow Exception)
        // they suck as a starting location. usefull for and end location.
        if (mazeGen.DeadEnds.Count != 0)
        {
            //start/end location wont allways spawn now because there isnt always dead ends.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            StartPos = mazeGen.DeadEnds[mapData.RandomEven(0, mazeGen.DeadEnds.Count - 1)];
            print("Start Pos: " + StartPos.ToString());
            findmapend(StartPos);
            player.transform.position = new Vector3((((StartPos.x*MapData.ROOMWIDTH)/2)+(MapData.ROOMWIDTH/2)), (((StartPos.y*MapData.ROOMHEIGHT)/2)-(MapData.ROOMHEIGHT/2)), 0);
            cam.transform.position = new Vector3((((StartPos.x * MapData.ROOMWIDTH) / 2) + (MapData.ROOMWIDTH / 2)), (((StartPos.y * MapData.ROOMHEIGHT) / 2) - (MapData.ROOMHEIGHT / 2)), -10);

        }
        else
        {
            if (mapsGenerated < 10)
            {
                GenPrefabMaze();
            }
        }
        GenPrefabs();
        
    }
    void GenPrefabs()
    {
        List<MapPos> Generated = new List<MapPos>();

        int prefabX;
        int prefabY;
        int mapX = 0;
        int mapY = 0;
        for (int y = 0; y < mapData.height; y = y + 2)
        {

            for (int x = 0; x < mapData.width; x = x + 2)
            {
                MapPos cPos = new MapPos(x, y);
                if (mapX == (mapData.width / 2) + 1)
                {
                    mapX = 0;
                    mapY++;
                }

                prefabX = mapX * MapData.ROOMWIDTH;
                prefabY = mapY * MapData.ROOMHEIGHT;
                GameObject go;
                if (mapData.LargeRoomPositions.Contains(cPos))
                {
                    Generated.AddRange(getLargeRoomPositions(x, y));
                    go = GenRoom(prefabX, prefabY + MapData.ROOMHEIGHT, _64x64[mapData.Rng.Next(0, _64x64.Count)], true, cPos);
                    CameraZone camZone = go.GetComponent<CameraZone>();
                    BoxCollider2D camColl = go.GetComponent<BoxCollider2D>();

                    camZone.zoneWidth = MapData.ROOMWIDTH * 2;
                    camZone.zoneHeight = MapData.ROOMHEIGHT * 2;
                    camColl.size = new Vector2(MapData.ROOMWIDTH*2, MapData.ROOMHEIGHT*2);
                    camColl.offset = new Vector2(MapData.ROOMWIDTH, -MapData.ROOMHEIGHT);
                }
                else if (mapData.Map[x,y] == 1 && !Generated.Contains(cPos))
                {
                    go = GenRoom(prefabX, prefabY, _32x32[mapData.Rng.Next(0, _32x32.Count)], false, cPos);
                    if (cPos.Equals(StartPos))
                    {
                        CameraZone camZone = go.GetComponent<CameraZone>();
                        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().curCamZone = camZone;
                    }
                }
                
                mapX++;
            }
        }
    }
    MapPos findmapend(MapPos startpos)
    {
        MapPos f = new MapPos(-1, -1);
        MapPos endPos = f;
        int endDist = 0;
        foreach( MapPos deadend in mazeGen.DeadEnds)
        {
            int newdist = (int)Mathf.Abs(startpos.x - deadend.x) + (int)Mathf.Abs(startpos.y - deadend.y);
            if (endPos.Equals(f) || endDist < newdist)
            {
                endPos = deadend;
                endDist = newdist;
            }
        }
        print("End Pos: " + endPos.ToString());
        print("Dist to End: " + endDist.ToString());
        return endPos;
    }
    //void getFiles ()
    //{
    //    if (GotFiles)
    //    {
    //        return;
    //    }

    //    string[] paths = new string[2]
    //    {
    //        "\\Tiled2Unity\\Prefabs\\Resources\\",
    //        "\\Prefabs\\Resources\\"
    //    };

    //    DirectoryInfo info;
    //    for(int i = 0; i < paths.GetLength(0); i++)
    //    {
    //        info = new DirectoryInfo(Application.dataPath + paths[i]);
    //        var fileInfo = info.GetFiles("*.prefab");
    //        foreach (FileInfo file in fileInfo)
    //        {
    //            string name = file.Name.Replace(".prefab", "");
    //            if (name.Contains("32x18"))
    //            {
    //                _32x32.Add(name);
    //            }
    //            else if (name.Contains("64x36"))
    //            {
    //                _64x64.Add(name);
    //            }
    //            else if (name.Contains("Enemy"))
    //            {

    //            }
    //            else
    //            {
    //                Items.Add(name);
    //            }
    //        }
    //    }
        
    // }

        void getFiles ()
    {
        if (MapConfig == null)
            return;

        _32x32 = MapConfig.SmallRooms;
        _64x64 = MapConfig.LargeRooms;
    }

    void InitLargeRooms()
    {
        for (int x = 0; x < mapData.width; x = x + 2)
        {
            for (int y = 0; y < mapData.height; y = y + 2)
            {
                MapPos cPos = new MapPos(x, y);
                List<MapPos> roomdata = getLargeRoomPositions(x, y);

                if (roomdata.Count == 4)
                {
                    bool isNotInMaze = true;

                    foreach (MapPos pos in roomdata)
                    {
                        if (mazeGen.RoomPositions.Contains(pos))
                        {
                            isNotInMaze = false;
                        }
                    }

                    if (isNotInMaze)
                    {
                        mazeGen.RoomPositions.AddRange(roomdata);
                        mapData.LargeRoomPositions.Add(cPos);
                    }
                }

                if (!mazeGen.RoomPositions.Contains(cPos) && mapData.Map[x, y] == 1 )//&& mazeGen.IsDeadEnd(x, y))
                {
                    mazeGen.DeadEnds.Add(cPos);
                }
            }

        }
    }

    void CutExcessLargeRooms ()
    {
        while (mapData.LargeRoomPositions.Count > MaxLargeRooms)
        {
            MapPos roompos = mapData.LargeRoomPositions[mapData.Rng.Next(0, mapData.LargeRoomPositions.Count - 1)];
            mapData.LargeRoomPositions.Remove(roompos);

            // fimnd all dead ends in room then add them to the dead ends list
            for (int x = roompos.x; x < roompos.x + 2; x = x + 2)
            {
                for (int y = roompos.y; y < roompos.y + 2; y = y + 2)
                {
                    if (mazeGen.IsDeadEnd(x, y))
                    {
                        mazeGen.DeadEnds.Add(new MapPos(x, y));
                    }
                }
            }

        }
    }
}
