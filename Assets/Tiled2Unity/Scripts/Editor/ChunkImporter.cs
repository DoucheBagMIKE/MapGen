using UnityEngine;
using System.Collections.Generic;
using Tiled2Unity;

[CustomTiledImporter]
class ChunkImporter : ICustomTiledImporter
{
    public void HandleCustomProperties(UnityEngine.GameObject gameObject,
        IDictionary<string, string> props)
    {

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
            Debug.Log(child.name);

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
