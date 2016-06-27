using UnityEngine;
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

    List<string> _32x32 = new List<string>();
    List<string> _64x64 = new List<string>();
    List<string> Items = new List<string>();
    

    void Awake ()
    {
        mapData = gameObject.GetComponent<MapData>();
        mazeGen = gameObject.GetComponent<MazeGenerator>();
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
            MapPos[] dirs = new MapPos[4]
            {
                        new MapPos(cPos.x + 1, cPos.y),
                        new MapPos(cPos.x - 1, cPos.y),
                        new MapPos(cPos.x, cPos.y + 1),
                        new MapPos(cPos.x, cPos.y - 1)
            };
            foreach (MapPos dir in dirs)
            {
                if (dir.x < 0 | dir.x > mapData.width - 1 || dir.y < 0 || dir.y > mapData.height - 1)
                {
                    continue;
                }
                if (!visited.Contains(dir) && mapData.Map[dir.x, dir.y] == 1 && !fringe.Contains(dir))
                {
                    fringe.Enqueue(dir);
                }
            }
        }
        return roomPositions;
        
    }
    void SetDoors(int x, int y, DoorManager doorManager, bool isLargeRoom)
    {
        string[] dirs = new string[4] { "Up", "Down", "Left", "Right" };
        if (isLargeRoom == true)
        {
            Dictionary<string, MapPos> IsWall = new Dictionary<string, MapPos>();

            IsWall["Door_Bottom_Left"] = new MapPos(x, y - 1);
            IsWall["Door_Left_Bottom"] = new MapPos(x - 1, y);
            IsWall["Door_Bottom_Right"] = new MapPos(x + 2, y - 1);
            IsWall["Door_Right_Bottom"] = new MapPos(x + 3, y);
            IsWall["Door_Top_Left"] = new MapPos(x, y + 3);
            IsWall["Door_Left_Top"] = new MapPos(x - 1, y + 2);
            IsWall["Door_Top_Right"] = new MapPos(x + 2, y + 3);
            IsWall["Door_Right_Top"] = new MapPos(x + 3, y + 2);

            foreach (Door door in doorManager.doors)
            {
                foreach(string key in IsWall.Keys)
                {
                    if (door.gameObject.name.Contains(key) == true)
                    {
                        int dx = IsWall[key].x;
                        int dy = IsWall[key].y;
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
            if (child.name == "ItemSpawner")
            {
                GenItem((int)child.position.x, (int)child.position.y, Items[mapData.Rng.Next(0, Items.Count)], child);
            }
            else if (child.name == "EnemySpawner")
            {
                //...
            }
            
        }
        return go;
    }
    GameObject GenItem(int x, int y, string itemName, Transform spawner)
    {
        GameObject go = (GameObject)Instantiate(Resources.Load(itemName));
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
        getFiles();
        mapData.LargeRoomPositions = new List<MapPos>();
        mazeGen.Generate(mapData.RandomEven(0, mapData.width - 1), mapData.RandomEven(0, mapData.height - 1));
        

        if (mazeGen.DeadEnds.Count != 0)
        {
            MapPos StartPos = mazeGen.DeadEnds[mapData.RandomEven(0, mazeGen.DeadEnds.Count - 1)];
            print("Start Pos: " + StartPos.ToString());
            findmapend(StartPos);
            GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3((((StartPos.x*32)/2)+16), (((StartPos.y*32)/2)-16), -1);
        }
        else
        {
            GenPrefabMaze();
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

                if (mapX == (mapData.width / 2) + 1)
                {
                    mapX = 0;
                    mapY++;
                }

                prefabX = mapX * 32;
                prefabY = mapY * 32;
                GameObject go;
                if (mapData.LargeRoomPositions.Contains(new MapPos(x,y)))
                {
                    Generated.AddRange(getLargeRoomPositions(x, y));
                    go = GenRoom(prefabX, prefabY + 32, _64x64[mapData.Rng.Next(0, _64x64.Count)], true, new MapPos(x, y));
                }
                else if (mapData.Map[x,y] == 1 && !Generated.Contains(new MapPos(x, y)))
                {
                    go = GenRoom(prefabX, prefabY, _32x32[mapData.Rng.Next(0, _32x32.Count)], false, new MapPos(x, y));
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
    void getFiles ()
    {
        string[] paths = new string[2]
        {
            "\\Tiled2Unity\\Prefabs\\Resources\\",
            "\\Prefabs\\Resources\\"
        };

        DirectoryInfo info;
        for(int i = 0; i < paths.GetLength(0); i++)
        {
            info = new DirectoryInfo(Application.dataPath + paths[i]);
            var fileInfo = info.GetFiles("*.prefab");
            foreach (FileInfo file in fileInfo)
            {
                string name = file.Name.Replace(".prefab", "");
                if (name.Contains("32x32"))
                {
                    _32x32.Add(name);
                }
                else if (name.Contains("64x64"))
                {
                    _64x64.Add(name);
                }
                else if (name.Contains("Enemy"))
                {

                }
                else
                {
                    Items.Add(name);
                }
            }
        }
        
     }
}
