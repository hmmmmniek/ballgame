using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using static CreateSessionController;

public class MapController : NetworkBehaviour {

    public bool generateMeshes = false;

    public Material material;
    public Volume goalPostProcessingVolume;
    public Mesh smallMesh;
    public Mesh mediumMesh;
    public Mesh largeMesh;
    public GameObject smallMapPrefab;
    public bool testSmallGeneration;

    [HideInInspector][Networked] public MapSize size {get; set;}


    public void OnValidate() {
        if (generateMeshes) {
            generateMeshes = false;
            MapGenerator gen = new MapGenerator();
            gen.GenerateMesh(MapSize.Small);
            gen.GenerateMesh(MapSize.Medium);
            gen.GenerateMesh(MapSize.Large);
        }
        if (testSmallGeneration) {
            testSmallGeneration = false;
            CreateMap(MapSize.Small);
        }
    }

    public override void Spawned() {
        base.Spawned();

        if(size != MapSize._) {
            SpawnMap(size);
        } else {
            SessionProperty mapSize;
            if (Runner.SessionInfo.Properties.TryGetValue("mapSize", out mapSize)) {
                SpawnMap((MapSize)(int)mapSize);
                if(Object.HasStateAuthority) {
                    size = (MapSize)(int)mapSize;
                }
            } else {
                Debug.LogError("Corrupt session state");
                NetworkState.Dispatch<object>(NetworkState.Leave, null, () => { });
                return;
            }
        }
    }

    public void SpawnMap(MapSize size) {
        switch(size) {
            case MapSize.Small: {
                Instantiate(smallMapPrefab);
                break;
            }
        }
    }

    private IEnumerator DestroyObject (GameObject go) {
        yield return null;
        DestroyImmediate(go);
    }



    private void CreateMap(MapSize mapSize) {
        foreach (Transform child in transform) {
           StartCoroutine(DestroyObject(child.gameObject));
        }
        MapGenerator gen = new MapGenerator();

        MapInfo mapInfo = gen.GetMapInfo(mapSize);
        mapInfo.mesh = mapSize == MapSize.Small ? smallMesh : (mapSize == MapSize.Medium ? mediumMesh : largeMesh);


        GameObject map = new GameObject("Map");
        map.transform.position = new Vector3(-mapInfo.mapWidth / 2, 0, -mapInfo.mapLength / 2);
        transform.position = new Vector3();
        map.transform.parent = transform;

        MeshRenderer meshRenderer = map.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        MeshFilter meshFilter = map.AddComponent<MeshFilter>();

        meshFilter.mesh = mapInfo.mesh;

        BoxCollider floor = map.AddComponent<BoxCollider>();
        float floorLength = mapInfo.mapLength + mapInfo.mapGoalDepth * 2 + 20;
        floor.size = new Vector3(mapInfo.mapWidth + 20, 10, floorLength);
        floor.center = new Vector3(mapInfo.mapWidth / 2, -5, mapInfo.mapLength / 2);

        BoxCollider ceiling = map.AddComponent<BoxCollider>();
        ceiling.size = new Vector3(mapInfo.mapWidth + 20, 10, floorLength);
        ceiling.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapHeight + 5, mapInfo.mapLength / 2);

        BoxCollider southTop = map.AddComponent<BoxCollider>();
        southTop.size = new Vector3(mapInfo.mapWidth + 20, mapInfo.mapHeight - mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        southTop.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight + (mapInfo.mapHeight - mapInfo.mapGoalHeight) / 2 + 5, -(10f + mapInfo.mapGoalDepth) / 2);

        BoxCollider northTop = map.AddComponent<BoxCollider>();
        northTop.size = new Vector3(mapInfo.mapWidth + 20, mapInfo.mapHeight - mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        northTop.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight + (mapInfo.mapHeight - mapInfo.mapGoalHeight) / 2 + 5, (10f + mapInfo.mapGoalDepth) / 2 + mapInfo.mapLength);

        BoxCollider southGoalBack = map.AddComponent<BoxCollider>();
        southGoalBack.size = new Vector3(mapInfo.mapGoalWidth + 10, mapInfo.mapGoalHeight + 10, 10);
        southGoalBack.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight / 2, -mapInfo.mapGoalDepth - 5);

        BoxCollider northGoalBack = map.AddComponent<BoxCollider>();
        northGoalBack.size = new Vector3(mapInfo.mapGoalWidth + 10, mapInfo.mapGoalHeight + 10, 10);
        northGoalBack.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight / 2, mapInfo.mapLength + mapInfo.mapGoalDepth + 5);

        BoxCollider southGoalWest = map.AddComponent<BoxCollider>();
        southGoalWest.size = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2 + 10, mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        southGoalWest.center = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 4 - 5, mapInfo.mapGoalHeight / 2, -(10 + mapInfo.mapGoalDepth) / 2);

        BoxCollider southGoalEast = map.AddComponent<BoxCollider>();
        southGoalEast.size = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2 + 10, mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        southGoalEast.center = new Vector3(mapInfo.mapWidth - (mapInfo.mapWidth - mapInfo.mapGoalWidth) / 4 + 5, mapInfo.mapGoalHeight / 2, -(10 + mapInfo.mapGoalDepth) / 2);

        BoxCollider northGoalWest = map.AddComponent<BoxCollider>();
        northGoalWest.size = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2 + 10, mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        northGoalWest.center = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 4 - 5, mapInfo.mapGoalHeight / 2, mapInfo.mapLength + (10 + mapInfo.mapGoalDepth) / 2);

        BoxCollider northGoalEast = map.AddComponent<BoxCollider>();
        northGoalEast.size = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2 + 10, mapInfo.mapGoalHeight + 10, 10 + mapInfo.mapGoalDepth);
        northGoalEast.center = new Vector3(mapInfo.mapWidth - (mapInfo.mapWidth - mapInfo.mapGoalWidth) / 4 + 5, mapInfo.mapGoalHeight / 2, mapInfo.mapLength + (10 + mapInfo.mapGoalDepth) / 2);

        BoxCollider wallWest = map.AddComponent<BoxCollider>();
        wallWest.size = new Vector3(10, mapInfo.mapHeight + 20, mapInfo.mapLength + mapInfo.mapGoalDepth * 2 + 20);
        wallWest.center = new Vector3(-5, mapInfo.mapHeight / 2, mapInfo.mapLength / 2);

        BoxCollider wallEast = map.AddComponent<BoxCollider>();
        wallEast.size = new Vector3(10, mapInfo.mapHeight + 20, mapInfo.mapLength + mapInfo.mapGoalDepth * 2 + 20);
        wallEast.center = new Vector3(mapInfo.mapWidth + 5, mapInfo.mapHeight / 2, mapInfo.mapLength / 2);

        CapsuleCollider southWestGoalPost = map.AddComponent<CapsuleCollider>();
        southWestGoalPost.radius = mapInfo.mapGoalPostRadius;
        southWestGoalPost.height = mapInfo.mapGoalHeight + mapInfo.mapGoalPostRadius * 2;
        southWestGoalPost.center = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2, mapInfo.mapGoalHeight / 2, 0);

        CapsuleCollider southEastGoalPost = map.AddComponent<CapsuleCollider>();
        southEastGoalPost.radius = mapInfo.mapGoalPostRadius;
        southEastGoalPost.height = mapInfo.mapGoalHeight + mapInfo.mapGoalPostRadius * 2;
        southEastGoalPost.center = new Vector3(mapInfo.mapWidth - (mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2, mapInfo.mapGoalHeight / 2, 0);

        CapsuleCollider southTopGoalPost = map.AddComponent<CapsuleCollider>();
        southTopGoalPost.direction = 0;
        southTopGoalPost.radius = mapInfo.mapGoalPostRadius;
        southTopGoalPost.height = mapInfo.mapGoalWidth + mapInfo.mapGoalPostRadius * 2;
        southTopGoalPost.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight, 0);

        CapsuleCollider northWestGoalPost = map.AddComponent<CapsuleCollider>();
        northWestGoalPost.radius = mapInfo.mapGoalPostRadius;
        northWestGoalPost.height = mapInfo.mapGoalHeight + mapInfo.mapGoalPostRadius * 2;
        northWestGoalPost.center = new Vector3((mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2, mapInfo.mapGoalHeight / 2, mapInfo.mapLength);

        CapsuleCollider northEastGoalPost = map.AddComponent<CapsuleCollider>();
        northEastGoalPost.radius = mapInfo.mapGoalPostRadius;
        northEastGoalPost.height = mapInfo.mapGoalHeight + mapInfo.mapGoalPostRadius * 2;
        northEastGoalPost.center = new Vector3(mapInfo.mapWidth - (mapInfo.mapWidth - mapInfo.mapGoalWidth) / 2, mapInfo.mapGoalHeight / 2, mapInfo.mapLength);

        CapsuleCollider northTopGoalPost = map.AddComponent<CapsuleCollider>();
        northTopGoalPost.direction = 0;
        northTopGoalPost.radius = mapInfo.mapGoalPostRadius;
        northTopGoalPost.height = mapInfo.mapGoalWidth + mapInfo.mapGoalPostRadius * 2;
        northTopGoalPost.center = new Vector3(mapInfo.mapWidth / 2, mapInfo.mapGoalHeight, mapInfo.mapLength);

        GameObject light = new GameObject("Light");
        light.transform.parent = transform;
        light.transform.position = new Vector3(0, mapInfo.mapHeight - 0.1f, 0);
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Point;
        lightComponent.range = mapInfo.lightRange;
        lightComponent.intensity = mapInfo.lightIntensity;
        lightComponent.shadows = LightShadows.Soft;
        lightComponent.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;

        Volume southGoalPostProcessingVolume = Instantiate(goalPostProcessingVolume);
        southGoalPostProcessingVolume.transform.parent = transform;
        southGoalPostProcessingVolume.transform.position = new Vector3(0, mapInfo.mapGoalHeight / 2, (mapInfo.mapLength + mapInfo.mapGoalDepth) / 2 + 0.5f);
        southGoalPostProcessingVolume.GetComponent<BoxCollider>().size = new Vector3(mapInfo.mapGoalWidth, mapInfo.mapGoalHeight, mapInfo.mapGoalDepth - 1f);

        Volume northGoalPostProcessingVolume = Instantiate(goalPostProcessingVolume);
        northGoalPostProcessingVolume.transform.parent = transform;
        northGoalPostProcessingVolume.transform.position = new Vector3(0, mapInfo.mapGoalHeight / 2, -(mapInfo.mapLength + mapInfo.mapGoalDepth) / 2 - 0.5f);
        northGoalPostProcessingVolume.GetComponent<BoxCollider>().size = new Vector3(mapInfo.mapGoalWidth, mapInfo.mapGoalHeight, mapInfo.mapGoalDepth - 1f);


        Vector3 cameraPos = new Vector3(0, mapInfo.mapHeight, 0);
        GameObject camera = new GameObject("Main Camera");
        Camera cameraComponent = camera.AddComponent<Camera>();
        camera.transform.position = cameraPos;
        camera.transform.rotation = Quaternion.Euler(90, -90, 0);
        camera.transform.parent = transform;
        cameraComponent.fieldOfView = 90;
        cameraComponent.farClipPlane = mapInfo.mapHeight + 1;
    }

}

