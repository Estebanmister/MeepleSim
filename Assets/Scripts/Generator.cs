using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Generator : MonoBehaviour
{
    // Generated a rectangular room
    public GameObject wall;
    public GameObject floorTile;
    public GameObject doorWay;
    public int length = 5;
    public int width = 5;
    public GameObject[] bathroomFurniture;
    public GameObject[] bedroomFurniture;
    public GameObject[] kitchenFurniture;
    public GameObject[] livingFurniture;
    float count = 0;

    // thanks Mark from stackoverflow
    int roundUp(int numToRound, int multiple)
    {
        if (multiple == 0)
            return numToRound;

        int remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;

        return numToRound + multiple - remainder;
    }
    void Update(){
        count += Time.deltaTime;
        if(count > 0.5f){
            count = 0;
            foreach(Transform child in transform.GetComponentInChildren<Transform>()){
                Destroy(child.gameObject);
            }
            length = Random.Range(10,40);
            width = Random.Range(10,40);
            length = roundUp(length, 2);
            width = roundUp(width, 2);
            GenerateWalls();
        }
        
    }
    void PutFurniture(int centerX, int centerY, int roomW, int roomL, string roompos, string type){
        int furnitures = 3;
        List<GameObject> toCreate = new List<GameObject>();
        if(type == "bathroom"){
            toCreate = bathroomFurniture.ToList();
        } else if(type == "bedroom"){
            toCreate = bedroomFurniture.ToList();
        } else if(type == "kitchen"){
            furnitures = 5;
            toCreate = kitchenFurniture.ToList();
        }else{
            furnitures = 4;
            toCreate = livingFurniture.ToList();
        }
        List<int> occupiedX = new List<int>();
        List<int> occupiedZ = new List<int>();
        for(int i = furnitures; i > 0; i-= 1){
            int indx = Random.Range(0, toCreate.Count);
            GameObject newfurn = Instantiate(toCreate[indx], transform);
            toCreate.RemoveAt(indx);
            int choice = Random.Range(0,2);
            
            if(roompos == "topleft"){
                if(choice == 0){
                    newfurn.transform.localPosition = new Vector3(centerX+roomW-1,0,Random.Range(centerY-roomL+2, centerY+roomL-2));
                    newfurn.transform.rotation = Quaternion.Euler(0,90,0);
                    
                }
                else if(choice == 1){
                    newfurn.transform.localPosition = new Vector3(Random.Range(centerX-roomW+2, centerX+roomW-2),0,centerY+roomL-1);
                    
                }
            }
            if(roompos == "topright"){
                if(choice == 0){
                    newfurn.transform.localPosition = new Vector3(centerX-roomW+1,0,Random.Range(centerY-roomL+2, centerY+roomL-2));
                    newfurn.transform.rotation = Quaternion.Euler(0,-90,0);
                }
                else if(choice == 1){
                    newfurn.transform.localPosition = new Vector3(Random.Range(centerX-roomW+2, centerX+roomW-2),0,centerY+roomL-1);
                }
            }
            if(roompos == "bottomleft"){
                if(choice == 0){
                    newfurn.transform.localPosition = new Vector3(centerX+roomW-1,0,Random.Range(centerY-roomL+2, centerY+roomL-2));
                    newfurn.transform.rotation = Quaternion.Euler(0,90,0);
                }
                else if(choice == 1){
                    newfurn.transform.localPosition = new Vector3(Random.Range(centerX-roomW+2, centerX+roomW-2),0,centerY-roomL+1);
                    newfurn.transform.rotation = Quaternion.Euler(0,180,0);
                }
            }
            if(roompos == "bottomright"){
                if(choice == 0){
                    newfurn.transform.localPosition = new Vector3(centerX-roomW+1,0,Random.Range(centerY-roomL+2, centerY+roomL-2));
                    newfurn.transform.rotation = Quaternion.Euler(0,-90,0);
                }
                else if(choice == 1){
                    newfurn.transform.localPosition = new Vector3(Random.Range(centerX-roomW+2, centerX+roomW-2),0,centerY-roomL+1);
                    newfurn.transform.rotation = Quaternion.Euler(0,180,0);
                }
            }
            if(choice == 0){
                while(occupiedZ.Contains((int)newfurn.transform.localPosition.z)){
                    newfurn.transform.localPosition += Vector3.forward;
                }
            } else {
                while(occupiedX.Contains((int)newfurn.transform.localPosition.x)){
                    newfurn.transform.localPosition += Vector3.right;
                }
            }
            occupiedX.Add((int)newfurn.transform.localPosition.x);
            occupiedZ.Add((int)newfurn.transform.localPosition.z);
            
        }
        
    }
    void GenerateWalls()
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

        // time to divide the apartment into rooms
        for(int x = 0; x<width; x +=1){
            if(x % (width/2) == 0){
                GameObject instan = Instantiate(doorWay, transform);
                instan.transform.localPosition = new Vector3(x, 0, length/2);
                x += 1;
            } else {
                GameObject instan = Instantiate(wall, transform);
                instan.transform.localPosition = new Vector3(x, 0, length/2);
            }
            
        }
        for(int y = 0; y<length; y +=1){
            if(y % (length/2) == 0){
                GameObject instan = Instantiate(doorWay, transform);
                instan.transform.localPosition = new Vector3(width/2, 0, y);
                instan.transform.rotation = Quaternion.Euler(0,-90,0);
                y += 1;
            } else {
                GameObject instan = Instantiate(wall, transform);
                instan.transform.localPosition = new Vector3(width/2, 0, y);
                instan.transform.rotation = Quaternion.Euler(0,-90,0);
            }
            
        }
        
        PutFurniture((width/2)+(width/4),(length/2)+(length/4),(width/4),(length/4),"topleft","bathroom");
        PutFurniture((width/4),(length/2)+(length/4),(width/4),(length/4),"topright","bedroom");
        PutFurniture((width/2)+(width/4),(length/4),(width/4),(length/4),"bottomleft","living");
        PutFurniture((width/4),(length/4),(width/4),(length/4),"bottomright","kitchen");

        
    }
}
