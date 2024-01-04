using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetMaker : MonoBehaviour
{
    public GameObject floor;
    public GameObject[] rooms;
    public int length = 5;
    public int width = 5;

    void Start(){
        CombineInstance[] combine = new CombineInstance[(width*length)];
        int ii = 0;
        for(int x = 0; x < width; x += 1){
            for(int y = 0; y < length; y += 1){
                GameObject newfloor = Instantiate(floor, transform);
                newfloor.transform.position = new Vector3(x,0,y)*2;
                combine[ii].mesh = newfloor.GetComponentInChildren<MeshFilter>().sharedMesh;
                combine[ii].transform = newfloor.transform.localToWorldMatrix;
                ii += 1;
                Destroy(newfloor);
                
            }
        }
        

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.localScale = new Vector3(5,1,5);
        for(int x = 0; x < width; x += 1){
            for(int y = 0; y < length; y += 1){
                GameObject newroom = Instantiate(rooms[Random.Range(0,rooms.Length-1)], new Vector3((x*2*5),0,(y*2*5)), Quaternion.identity);
            }
        }
    }
}
