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
    public GameObject door;

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
    void Start(){
        GenerateWalls();
        
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
        bool maindoor = false;

        // to check if the  furniture we move around is touching the edge or not
        bool reachedCornerX = false;
        bool reachedCornerZ = false;

        List<int> occupiedX = new List<int>();
        List<int> occupiedZ = new List<int>();

        bool doorPlaced = false;
        bool reAttempt = false;
        int lastChoice = 0;
        int indx = Random.Range(0, toCreate.Count);
        for(int i = furnitures; i > 0; i-= 1){
            int choice = Random.Range(0,2);
            if(reAttempt){
                reAttempt = false;
                if(lastChoice == 0){
                    choice = 1;
                } else{
                    choice = 0;
                }
            } else {
                indx = toCreate.Count-1;
            }
            
            GameObject newfurn;
            if(!maindoor && type == "living"){
                maindoor = true;
                newfurn = Instantiate(door, transform);
                doorPlaced = true;
            } else {
                doorPlaced = false;
                newfurn = Instantiate(toCreate[indx], transform);
            }
            
            
            
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
            bool placed = true;
            List<int> sizeZ = Enumerable.Range((int)newfurn.transform.localPosition.z,(int)newfurn.transform.localPosition.z+(int)newfurn.transform.localScale.z-1).ToList();
            List<int> sizeX = Enumerable.Range((int)newfurn.transform.localPosition.x,(int)newfurn.transform.localPosition.x+(int)newfurn.transform.localScale.x-1).ToList();
            if(choice == 0){
                while(occupiedZ.Any(item => sizeZ.Contains(item))){
                    if(reachedCornerZ){
                        newfurn.transform.localPosition -= Vector3.forward;
                        if(newfurn.transform.localPosition.z < centerY-roomL){
                            Destroy(newfurn);
                            placed = false;
                            break;
                        }
                    } else {
                        newfurn.transform.localPosition += Vector3.forward;
                        if(newfurn.transform.localPosition.z > centerY+roomL){
                            reachedCornerZ = true;
                        }
                    }
                    sizeZ = Enumerable.Range((int)newfurn.transform.localPosition.z,(int)newfurn.transform.localPosition.z+(int)newfurn.transform.localScale.z-1).ToList();
                    
                }
            } else {
                while(occupiedX.Any(item => sizeX.Contains(item))){
                    if(reachedCornerX){
                        newfurn.transform.localPosition -= Vector3.right;
                        if(newfurn.transform.localPosition.x < centerX-roomW){
                            Destroy(newfurn);
                            placed = false;
                            break;
                        }
                    } else {
                        newfurn.transform.localPosition += Vector3.right;
                        if(newfurn.transform.localPosition.x > centerX+roomW){
                            reachedCornerX = true;
                        }
                    }
                    sizeX = Enumerable.Range((int)newfurn.transform.localPosition.x,(int)newfurn.transform.localPosition.x+(int)newfurn.transform.localScale.x-1).ToList();
                }
            }
            if(placed){
                occupiedX.AddRange(sizeX);
                occupiedZ.AddRange(sizeZ);
                if(!doorPlaced){
                    toCreate.RemoveAt(indx);
                }
                
            } else {
                reAttempt = true;
                lastChoice = choice;
            }
            
            
        }
        
    }
    void GenerateWalls()
    {
        // Put down individual walls to make up the sides
        for(int i = 1; i <= length+1; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(width+1, 0, i);
            instan.transform.rotation = Quaternion.Euler(0,90,0);
        }
        for(int i = 0; i <= length; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(0, 0, i);
            instan.transform.rotation = Quaternion.Euler(0,-90,0);
        }
        for(int i = 0; i <= width; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(i, 0, length+1);
            instan.transform.rotation = Quaternion.Euler(0,0,0);
        }
        for(int i = 1; i <= width+1; i += 1){
            GameObject instan = Instantiate(wall, transform);
            instan.transform.localPosition = new Vector3(i, 0, 0);
            instan.transform.rotation = Quaternion.Euler(0,180,0);
        }

        // Create floor by combining all meshes of tiles at the correct position
        CombineInstance[] combine = new CombineInstance[width*length];
        int ii = 0;
        Vector3 holdme = transform.position;
        transform.position = Vector3.zero;
        for(int x = 0; x<width-1; x +=1){
            for(int y = 0; y<length-1; y+=1){
                GameObject instan = Instantiate(floorTile, transform);
                instan.transform.localPosition = new Vector3(x, 0, y);
                combine[ii].mesh = instan.GetComponentInChildren<MeshFilter>().sharedMesh;
                combine[ii].transform = instan.transform.localToWorldMatrix;
                // we do not want a crap ton of game objects for tiles, so we remove them after getting our mesh
                Destroy(instan);
                ii += 1;
            }
        }
        // render the floor
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine,true,true);
        mesh.Optimize();
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.position = holdme;
        // time to divide the apartment into rooms
        for(int x = 0; x<=width; x +=1){
            if(x % (width/2) == 0 && x != width){
                GameObject instan = Instantiate(doorWay, transform);
                instan.transform.localPosition = new Vector3(x, 0, length/2);
                x += 1;
            } else {
                GameObject instan = Instantiate(wall, transform);
                instan.transform.localPosition = new Vector3(x, 0, length/2);
            }
            
        }
        for(int y = 0; y<=length; y +=1){
            if(y % (length/2) == 0 && y !=length){
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
