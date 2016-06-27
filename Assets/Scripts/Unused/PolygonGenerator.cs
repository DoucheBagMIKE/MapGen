using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolygonGenerator : MonoBehaviour {

    public MapData mapData;

	public List<Vector3> newVertices = new List<Vector3>();

	public List<int> newTriangles = new List<int>();

	public List<Vector2> newUV = new List<Vector2>();

	private Mesh mesh;

	float tUnitx = .5f;
    float tUnity = .125f;
    Vector2 tGrass = new Vector2(0,5);
	Vector2 tStone = new Vector2(1,0);
	int TileCount;

	public int[,] Tiles;
	
	// Use this for initialization
	void Awake () {

		mesh = GetComponent<MeshFilter>().mesh;

        mapData = gameObject.GetComponent<MapData>();
	
	}

    public void Generate()
    {
        GenTerrain();
        BuildMesh();
        RenderMesh();
    }

	void GenTile(int x, int y, Vector2 texture) {
		
		newVertices.Add( new Vector3 (x  , y  , 0 ));
		newVertices.Add( new Vector3 (x + 1 , y  , 0 ));
		newVertices.Add( new Vector3 (x + 1 , y-1  , 0 ));
		newVertices.Add( new Vector3 (x  , y-1  , 0 ));

		newTriangles.Add(TileCount*4);
		newTriangles.Add((TileCount*4)+1);
		newTriangles.Add((TileCount*4)+3);
		newTriangles.Add((TileCount*4)+1);
		newTriangles.Add((TileCount*4)+2);
		newTriangles.Add((TileCount*4)+3);
		
		newUV.Add(new Vector2 (tUnitx * texture.x, tUnity * texture.y + tUnity));
		newUV.Add(new Vector2 (tUnitx * texture.x + tUnitx, tUnity * texture.y + tUnity));
		newUV.Add(new Vector2 (tUnitx * texture.x + tUnitx, tUnity * texture.y));
		newUV.Add(new Vector2 (tUnitx * texture.x, tUnity * texture.y));

		TileCount++;
	}

	void RenderMesh() {
		mesh.Clear();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.Optimize();
		mesh.RecalculateNormals();

		TileCount=0;
		newVertices.Clear();
		newTriangles.Clear();
		newUV.Clear();
	}

	void GenTerrain(){
        Tiles = mapData.Map;
	}

	void BuildMesh(){
		for(int px=0;px<Tiles.GetLength(0);px++){
			for(int py=0;py<Tiles.GetLength(1);py++){
				
				if(Tiles[px,py]==0){
					GenTile(px,py,tStone);
				} else if(Tiles[px,py]==1){
					GenTile(px,py,tGrass);
				}
				
			}
		}
	}
}
