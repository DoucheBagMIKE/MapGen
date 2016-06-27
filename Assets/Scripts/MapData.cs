using UnityEngine;
using System.Collections.Generic;

public class MapData : MonoBehaviour {

    public int width;
    public int height;
    public int[,] Map;

    public System.Random Rng;
    public bool randomSeed;
    public string seed;
    public List<MapPos> LargeRoomPositions;

	// Use this for initialization
	void Awake () {
        LargeRoomPositions = new List<MapPos>();
        if (randomSeed)
        {
            seed = System.DateTime.Now.ToString();
        }
        
        Rng = new System.Random(seed.GetHashCode());

        Map = new int[width, height];
    }

    public int RandomEven(int min, int max)
    {
        int rVal = Rng.Next(min, max);
        while (rVal % 2 != 0)
        {
            rVal = Rng.Next(min, max);
        }
        return rVal;
    }
}
