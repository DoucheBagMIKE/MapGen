﻿using UnityEngine;
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
                foreach (BoxCollider2D box2d in child.GetComponentsInChildren<BoxCollider2D>())
                {
                    BoxCollider2D.DestroyImmediate(box2d);

                }
            }
        }
    }
}