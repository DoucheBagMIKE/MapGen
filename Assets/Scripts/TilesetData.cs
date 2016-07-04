using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAreaConfig", menuName = "MapGenerator/AreaConfig", order = 1)]

[System.Serializable]
public class TilesetData : ScriptableObject
{
    public List<GameObject> Items;

    public List<string> Enemys;

    public List<string> SmallRooms;
    public List<string> LargeRooms;

    public int width;
    public int height;

    [Range(0,10)]
    public int MaxLargeRooms;

    [Range(0, 10)]
    public int RemoveDeadEndsIterations;
    [Range(0, 10)]
    public int DeadCells;

    [Range(0f, 1f)]
    public float loopPercent;
    [Range(0f, 1f)]
    public float windyOrRandomPercent;

    void OnEnable ()
    {
        if (Items == null)
        {
            Items = new List<GameObject>();
        }
        if (Enemys == null)
        {
            Enemys = new List<string>();
        }
        if (SmallRooms == null)
        {
            SmallRooms = new List<string>();
        }
        if (LargeRooms == null)
        {
            LargeRooms = new List<string>();
        }
    }

}
