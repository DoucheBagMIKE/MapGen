using UnityEngine;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEditor;

[CustomTiledImporter]
class ChunkImporter : ICustomTiledImporter
{
    public void HandleCustomProperties(UnityEngine.GameObject gameObject,
        IDictionary<string, string> props)
        
    {
        if(props.ContainsKey("Areas"))
        {
            foreach (string areaName in props["Areas"].Split('|'))
            {
                string[] guids = AssetDatabase.FindAssets(areaName, new string[1] { "Assets/Resources" }) ;
                TilesetData data = null;
                if(guids.Length == 0)
                {
                    // create a MapConfig;
                    data = ScriptableObject.CreateInstance<TilesetData>();
                    data.SmallRooms = new List<string>();
                    data.LargeRooms = new List<string>();
                    AssetDatabase.CreateAsset(data, "Assets/Resources/" + areaName + ".asset");
                }
                else
                {
                    // load the mapconfig and add this mapchunk to it.
                    data = AssetDatabase.LoadAssetAtPath<TilesetData>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    
                }
                if (props["IsLargeRoom"] == "true")
                {
                    if (!data.LargeRooms.Contains(gameObject.name))
                    {
                        data.LargeRooms.Add(gameObject.name);
                        
                    }

                }
                else
                {
                    if (!data.SmallRooms.Contains(gameObject.name))
                    {
                        data.SmallRooms.Add(gameObject.name);
                        
                    }

                }
                EditorUtility.SetDirty(data);
            }

        }

        if (props.ContainsKey("Facing"))
        {
            gameObject.AddComponent<Door>();
            Door door = gameObject.GetComponent<Door>();
            door.facing = props["Facing"];
        }

        if (props.ContainsKey("SpawnerType"))
        {
            gameObject.name = props["SpawnerType"] + "Spawner";
        }
    }


    public void CustomizePrefab(GameObject prefab)
    {
        CameraZone camZone = prefab.AddComponent<CameraZone>();
        BoxCollider2D camColl = prefab.GetComponent<BoxCollider2D>();
        camZone.zoneWidth = MapData.ROOMWIDTH;
        camZone.zoneHeight = MapData.ROOMHEIGHT;
        camColl.size = new Vector2(MapData.ROOMWIDTH, MapData.ROOMHEIGHT);
        camColl.offset = new Vector2(MapData.ROOMWIDTH/2, -(MapData.ROOMHEIGHT/2));
        camColl.isTrigger = true;

        prefab.AddComponent<DoorManager>();
        DoorManager doorManager = prefab.GetComponent<DoorManager>();
        if (doorManager.doors == null)
        {
            doorManager.doors = new List<Door>();
        }

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            GameObject child = prefab.transform.GetChild(i).gameObject;

            if (child.name.Contains("Door"))
            {                
                child.gameObject.SetActive(false);
                doorManager.doors.Add(child.GetComponent<Door>());
            }
            else if (child.name.Contains("Spawners"))
            {
                Vector3 cPos = child.gameObject.transform.position;
                cPos.Set(cPos.x + 0.5f, cPos.y + 0.5f, cPos.z);

                foreach (BoxCollider2D box2d in child.GetComponentsInChildren<BoxCollider2D>())
                {
                    BoxCollider2D.DestroyImmediate(box2d);

                }
            }
        }
    }
}
