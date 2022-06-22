using System;
using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;
using static CreateSessionController;

public class MapController : NetworkBehaviour {
    
    public bool generateMeshes = false;

    public Material material;
    public Mesh smallMesh;
    public Mesh mediumMesh;
    public Mesh largeMesh;


    public void OnValidate() {
        if(generateMeshes) {
            generateMeshes = false;
            MapGenerator gen = new MapGenerator();
            gen.GenerateMesh(MapSize.Small);
            gen.GenerateMesh(MapSize.Medium);
            gen.GenerateMesh(MapSize.Large);
        }
    }

    public override void Spawned() {
        base.Spawned();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        float mapWidth = 0;
        float mapLength = 0;
        float mapHeight = 0;
        float mapGoalWidth = 0;
        float mapGoalHeight = 0;
        float mapGoalDepth = 0;
        float mapGoalPostRadius = 0;
        int mapGoalPostSegments = 0;
        float lightMaxDistanceBetween = 0;
        float lightSpotAngle = 0;
        float lightIntensity = 0;
        float lightRotation = 0;
        float lightRange = 0;
        Mesh mesh = null;
        SessionProperty mapSize;
        
        MapGenerator gen = new MapGenerator();

        if (Runner.SessionInfo.Properties.TryGetValue("mapSize", out mapSize)) {
            switch ((int)mapSize) {
                case (int)MapSize.Small: {
                        mapWidth = gen.mapSmallWidth;
                        mapLength = gen.mapSmallLength;
                        mapHeight = gen.mapSmallHeight;
                        mapGoalWidth = gen.mapSmallGoalWidth;
                        mapGoalHeight = gen.mapSmallGoalHeight;
                        mapGoalDepth = gen.mapSmallGoalDepth;
                        mapGoalPostRadius = gen.mapSmallGoalPostRadius;
                        mapGoalPostSegments = gen.mapSmallGoalPostSegments;
                        lightMaxDistanceBetween = gen.mapSmallLightMaxDistanceBetween;
                        lightSpotAngle = gen.mapSmallLightSpotAngle;
                        lightIntensity = gen.mapSmallLightIntensity;
                        lightRotation = gen.mapSmallLightRotation;
                        lightRange = gen.mapSmallLightRange;
                        mesh = smallMesh;
                        break;
                    }
                case (int)MapSize.Medium: {
                        mapWidth = gen.mapMediumWidth;
                        mapLength = gen.mapMediumLength;
                        mapHeight = gen.mapMediumHeight;
                        mapGoalWidth = gen.mapMediumGoalWidth;
                        mapGoalHeight = gen.mapMediumGoalHeight;
                        mapGoalDepth = gen.mapMediumGoalDepth;
                        mapGoalPostRadius = gen.mapMediumGoalPostRadius;
                        mapGoalPostSegments = gen.mapMediumGoalPostSegments;
                        lightMaxDistanceBetween = gen.mapMediumLightMaxDistanceBetween;
                        lightSpotAngle = gen.mapMediumLightSpotAngle;
                        lightIntensity = gen.mapMediumLightIntensity;
                        lightRotation = gen.mapMediumLightRotation;
                        lightRange = gen.mapMediumLightRange;
                        mesh = mediumMesh;
                        break;
                    }
                case (int)MapSize.Large: {
                        mapWidth = gen.mapLargeWidth;
                        mapLength = gen.mapLargeLength;
                        mapHeight = gen.mapLargeHeight;
                        mapGoalWidth = gen.mapLargeGoalWidth;
                        mapGoalHeight = gen.mapLargeGoalHeight;
                        mapGoalDepth = gen.mapLargeGoalDepth;
                        mapGoalPostRadius = gen.mapLargeGoalPostRadius;
                        mapGoalPostSegments = gen.mapLargeGoalPostSegments;
                        lightMaxDistanceBetween = gen.mapLargeLightMaxDistanceBetween;
                        lightSpotAngle = gen.mapLargeLightSpotAngle;
                        lightIntensity = gen.mapLargeLightIntensity;
                        lightRotation = gen.mapLargeLightRotation;
                        lightRange = gen.mapLargeLightRange;
                        mesh = largeMesh;
                        break;
                    }
            }
        } else {
            Debug.LogError("Corrupt session state");
            NetworkState.Dispatch<object>(NetworkState.Leave, null, () => { });
            return;
        }

        GameObject map = new GameObject("Map");
        map.transform.position = new Vector3(-mapWidth / 2, 0, -mapLength / 2);
        transform.position = new Vector3();
        map.transform.parent = transform;

        MeshRenderer meshRenderer = map.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        MeshFilter meshFilter = map.AddComponent<MeshFilter>();

        meshFilter.mesh = mesh;

        BoxCollider floor = map.AddComponent<BoxCollider>();
        float floorLength = mapLength + mapGoalDepth * 2 + 20;
        floor.size = new Vector3(mapWidth, 10, floorLength);
        floor.center = new Vector3(mapWidth / 2, -5, mapLength / 2);

        BoxCollider ceiling = map.AddComponent<BoxCollider>();
        ceiling.size = new Vector3(mapWidth, 10, floorLength);
        ceiling.center = new Vector3(mapWidth / 2, mapHeight + 5, mapLength / 2);

        BoxCollider southTop = map.AddComponent<BoxCollider>();
        southTop.size = new Vector3(mapWidth, mapHeight - mapGoalHeight, 10 + mapGoalDepth);
        southTop.center = new Vector3(mapWidth / 2, mapGoalHeight + (mapHeight - mapGoalHeight) / 2, -(10f + mapGoalDepth) / 2);

        BoxCollider northTop = map.AddComponent<BoxCollider>();
        northTop.size = new Vector3(mapWidth, mapHeight - mapGoalHeight, 10 + mapGoalDepth);
        northTop.center = new Vector3(mapWidth / 2, mapGoalHeight + (mapHeight - mapGoalHeight) / 2, (10f + mapGoalDepth) / 2 + mapLength);

        BoxCollider southGoalBack = map.AddComponent<BoxCollider>();
        southGoalBack.size = new Vector3(mapGoalWidth, mapGoalHeight, 10);
        southGoalBack.center = new Vector3(mapWidth / 2, mapGoalHeight / 2, -mapGoalDepth - 5);

        BoxCollider northGoalBack = map.AddComponent<BoxCollider>();
        northGoalBack.size = new Vector3(mapGoalWidth, mapGoalHeight, 10);
        northGoalBack.center = new Vector3(mapWidth / 2, mapGoalHeight / 2, mapLength + mapGoalDepth + 5);

        BoxCollider southGoalWest = map.AddComponent<BoxCollider>();
        southGoalWest.size = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 10 + mapGoalDepth);
        southGoalWest.center = new Vector3((mapWidth - mapGoalWidth) / 4, mapGoalHeight / 2, -(10 + mapGoalDepth) / 2);

        BoxCollider southGoalEast = map.AddComponent<BoxCollider>();
        southGoalEast.size = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 10 + mapGoalDepth);
        southGoalEast.center = new Vector3(mapWidth - (mapWidth - mapGoalWidth) / 4, mapGoalHeight / 2, -(10 + mapGoalDepth) / 2);

        BoxCollider northGoalWest = map.AddComponent<BoxCollider>();
        northGoalWest.size = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 10 + mapGoalDepth);
        northGoalWest.center = new Vector3((mapWidth - mapGoalWidth) / 4, mapGoalHeight / 2, mapLength + (10 + mapGoalDepth) / 2);

        BoxCollider northGoalEast = map.AddComponent<BoxCollider>();
        northGoalEast.size = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 10 + mapGoalDepth);
        northGoalEast.center = new Vector3(mapWidth - (mapWidth - mapGoalWidth) / 4, mapGoalHeight / 2, mapLength + (10 + mapGoalDepth) / 2);

        BoxCollider wallWest = map.AddComponent<BoxCollider>();
        wallWest.size = new Vector3(10, mapHeight + 20, mapLength + mapGoalDepth * 2 + 20);
        wallWest.center = new Vector3(-5, mapHeight / 2, mapLength / 2);

        BoxCollider wallEast = map.AddComponent<BoxCollider>();
        wallEast.size = new Vector3(10, mapHeight + 20, mapLength + mapGoalDepth * 2 + 20);
        wallEast.center = new Vector3(mapWidth + 5, mapHeight / 2, mapLength / 2);

        CapsuleCollider southWestGoalPost = map.AddComponent<CapsuleCollider>();
        southWestGoalPost.radius = mapGoalPostRadius;
        southWestGoalPost.height = mapGoalHeight + mapGoalPostRadius * 2;
        southWestGoalPost.center = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight / 2, 0);

        CapsuleCollider southEastGoalPost = map.AddComponent<CapsuleCollider>();
        southEastGoalPost.radius = mapGoalPostRadius;
        southEastGoalPost.height = mapGoalHeight + mapGoalPostRadius * 2;
        southEastGoalPost.center = new Vector3(mapWidth - (mapWidth - mapGoalWidth) / 2, mapGoalHeight / 2, 0);

        CapsuleCollider southTopGoalPost = map.AddComponent<CapsuleCollider>();
        southTopGoalPost.direction = 0;
        southTopGoalPost.radius = mapGoalPostRadius;
        southTopGoalPost.height = mapGoalWidth + mapGoalPostRadius * 2;
        southTopGoalPost.center = new Vector3(mapWidth / 2, mapGoalHeight, 0);

        CapsuleCollider northWestGoalPost = map.AddComponent<CapsuleCollider>();
        northWestGoalPost.radius = mapGoalPostRadius;
        northWestGoalPost.height = mapGoalHeight + mapGoalPostRadius * 2;
        northWestGoalPost.center = new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight / 2, mapLength);

        CapsuleCollider northEastGoalPost = map.AddComponent<CapsuleCollider>();
        northEastGoalPost.radius = mapGoalPostRadius;
        northEastGoalPost.height = mapGoalHeight + mapGoalPostRadius * 2;
        northEastGoalPost.center = new Vector3(mapWidth - (mapWidth - mapGoalWidth) / 2, mapGoalHeight / 2, mapLength);

        CapsuleCollider northTopGoalPost = map.AddComponent<CapsuleCollider>();
        northTopGoalPost.direction = 0;
        northTopGoalPost.radius = mapGoalPostRadius;
        northTopGoalPost.height = mapGoalWidth + mapGoalPostRadius * 2;
        northTopGoalPost.center = new Vector3(mapWidth / 2, mapGoalHeight, mapLength);


        Vector3 southWestLightPos = new Vector3(-mapWidth / 2 + 0.5f, mapHeight - 0.5f, -mapLength / 2 + 0.5f);
        Light southWestLight = CreateLight(
            southWestLightPos,
            Quaternion.Euler(lightRotation, 45, 0),
            lightRange,
            transform,
            lightIntensity,
            lightSpotAngle
        );

        Vector3 northWestLightPos = new Vector3(-mapWidth / 2 + 0.5f, mapHeight - 0.5f, +mapLength / 2 - 0.5f);
        Light northWestLight = CreateLight(
            northWestLightPos,
            Quaternion.Euler(lightRotation, 135, 0),
            lightRange,
            transform,
            lightIntensity,
            lightSpotAngle
        );

        Vector3 southEastLightPos = new Vector3(mapWidth / 2 - 0.5f, mapHeight - 0.5f, -mapLength / 2 + 0.5f);
        Light southEastLight = CreateLight(
            southEastLightPos,
            Quaternion.Euler(lightRotation, -45, 0),
            lightRange,
            transform,
            lightIntensity,
            lightSpotAngle
        );

        Vector3 northEastLightPos = new Vector3(mapWidth / 2 - 0.5f, mapHeight - 0.5f, +mapLength / 2 - 0.5f);
        Light northEastLight = CreateLight(
            northEastLightPos,
            Quaternion.Euler(lightRotation, -135, 0),
            lightRange,
            transform,
            lightIntensity,
            lightSpotAngle
        );


        CheckLights(southWestLightPos, northWestLightPos, lightMaxDistanceBetween, lightRange, Quaternion.Euler(lightRotation, 90, 0), lightIntensity, lightSpotAngle);
        CheckLights(southEastLightPos, northEastLightPos, lightMaxDistanceBetween, lightRange, Quaternion.Euler(lightRotation, -90, 0), lightIntensity, lightSpotAngle);


    }

    private Light CreateLight(Vector3 position, Quaternion rotation, float range, Transform parent, float intensity, float spotAngle) {

        GameObject light = new GameObject("Light");
        light.transform.parent = transform;
        light.transform.position = position;
        light.transform.rotation = rotation;
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Spot;
        lightComponent.range = range;
        lightComponent.intensity = intensity;
        lightComponent.spotAngle = spotAngle;
        lightComponent.shadows = LightShadows.Soft;

        return lightComponent;
    }


    private void CheckLights(Vector3 pos1, Vector3 pos2, float lightMaxDistanceBetween, float range, Quaternion rotation, float intensity, float spotAngle) {

        if ((pos1 - pos2).magnitude > lightMaxDistanceBetween) {
            Vector3 newLightPos = pos2 + (pos1 - pos2).normalized * ((pos1 - pos2).magnitude / 2);
            Light newLight = CreateLight(
                newLightPos,
                rotation,
                range,
                transform,
                intensity,
                spotAngle
            );
            CheckLights(pos1, newLightPos, lightMaxDistanceBetween, range, rotation, intensity, spotAngle);
            CheckLights(newLightPos, pos2, lightMaxDistanceBetween, range, rotation, intensity, spotAngle);
        }
    }

}
