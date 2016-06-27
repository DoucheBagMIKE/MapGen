using UnityEngine;
using System.Collections;

public struct MapPos
{
    public int x, y;
    public MapPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {

        if (!(obj is MapPos))
        {
            return false;
        }

        MapPos pos = (MapPos)obj;
        if (pos.x != x || pos.y != y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override string ToString()
    {
        return x.ToString() + "," + y.ToString();
    }
}