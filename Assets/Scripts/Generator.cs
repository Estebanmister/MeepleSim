using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    // Generated a rectangular room
    public GameObject wall;
    public GameObject floorTile;
    public int length = 5;
    public int width = 5;
    void Start()
    {
        // Put down individual walls to make up the sides
        for(int i = 1; i <= length; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(width, 0, i);
            instan.transform.rotation = Quaternion.Euler(0,90,0);
        }
        for(int i = 0; i < length; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(0, 0, i);
            instan.transform.rotation = Quaternion.Euler(0,-90,0);
        }
        for(int i = 0; i < width; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(i, 0, length);
            instan.transform.rotation = Quaternion.Euler(0,0,0);
        }
        for(int i = 1; i <= width; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(i, 0, 0);
            instan.transform.rotation = Quaternion.Euler(0,180,0);
        }

        // Create floor by combining all meshes of tiles at the correct position
        CombineInstance[] combine = new CombineInstance[width*length];
        int ii = 0;
        for(int x = 0; x<width; x +=1){
            for(int y = 0; y<length; y+=1){
                GameObject instan = Instantiate(floorTile, transform);
                instan.transform.localPosition = new Vector3(x, 0, y);
                combine[ii].mesh = instan.GetComponent<MeshFilter>().sharedMesh;
                combine[ii].transform = instan.transform.localToWorldMatrix;
                // we do not want a crap ton of game objects for tiles, so we remove them after getting our mesh
                Destroy(instan);
                ii += 1;
            }
        }
        // render the floor
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;

        
    }
}
