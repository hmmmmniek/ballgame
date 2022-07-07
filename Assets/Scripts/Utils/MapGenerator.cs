using System;
using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;
using static CreateSessionController;

public class MapGenerator {


    public float mapSmallWidth = 45f;
    public float mapSmallLength = 60f;
    public float mapSmallHeight = 25f;
    public float mapSmallGoalWidth = 15f;
    public float mapSmallGoalHeight = 6f;
    public float mapSmallGoalDepth = 6f;
    public float mapSmallGoalPostRadius = 0.4f;
    public int mapSmallGoalPostSegments = 12;
    public float mapSmallLightMaxDistanceBetween = 60f;
    public float mapSmallLightSpotAngle = 100f;
    public float mapSmallLightInnerSpotAngle = 90f;
    public float mapSmallLightIntensity = 250f;
    public float mapSmallLightRotation = 50f;
    public float mapSmallLightRange = 90f;

    public float mapMediumWidth = 55f;
    public float mapMediumLength = 90f;
    public float mapMediumHeight = 35f;
    public float mapMediumGoalWidth = 15f;
    public float mapMediumGoalHeight = 6f;
    public float mapMediumGoalDepth = 6f;
    public float mapMediumGoalPostRadius = 0.4f;
    public int mapMediumGoalPostSegments = 12;
    public float mapMediumLightMaxDistanceBetween = 50f;
    public float mapMediumLightSpotAngle = 110f;
    public float mapMediumLightInnerSpotAngle = 100f;
    public float mapMediumLightIntensity = 300f;
    public float mapMediumLightRotation = 70f;
    public float mapMediumLightRange = 90f;

    public float mapLargeWidth = 80f;
    public float mapLargeLength = 135f;
    public float mapLargeHeight = 45f;
    public float mapLargeGoalWidth = 15f;
    public float mapLargeGoalHeight = 6f;
    public float mapLargeGoalDepth = 6f;
    public float mapLargeGoalPostRadius = 0.4f;
    public int mapLargeGoalPostSegments = 12;
    public float mapLargeLightMaxDistanceBetween = 40f;
    public float mapLargeLightSpotAngle = 110f;
    public float mapLargeLightInnerSpotAngle = 100f;
    public float mapLargeLightIntensity = 200f;
    public float mapLargeLightRotation = 70f;
    public float mapLargeLightRange = 100f;

    public Material material;


    public MapGenerator() {

    }

    public void GenerateMesh(MapSize size) {

#if UNITY_EDITOR
        float mapWidth = 0;
        float mapLength = 0;
        float mapHeight = 0;
        float mapGoalWidth = 0;
        float mapGoalHeight = 0;
        float mapGoalDepth = 0;
        float mapGoalPostRadius = 0;
        int mapGoalPostSegments = 0;
        string meshName = "";
        switch ((int)size) {
            case (int)MapSize.Small: {
                    mapWidth = mapSmallWidth;
                    mapLength = mapSmallLength;
                    mapHeight = mapSmallHeight;
                    mapGoalWidth = mapSmallGoalWidth;
                    mapGoalHeight = mapSmallGoalHeight;
                    mapGoalDepth = mapSmallGoalDepth;
                    mapGoalPostRadius = mapSmallGoalPostRadius;
                    mapGoalPostSegments = mapSmallGoalPostSegments;
                    meshName = "Small";
                    break;
                }
            case (int)MapSize.Medium: {
                    mapWidth = mapMediumWidth;
                    mapLength = mapMediumLength;
                    mapHeight = mapMediumHeight;
                    mapGoalWidth = mapMediumGoalWidth;
                    mapGoalHeight = mapMediumGoalHeight;
                    mapGoalDepth = mapMediumGoalDepth;
                    mapGoalPostRadius = mapMediumGoalPostRadius;
                    mapGoalPostSegments = mapMediumGoalPostSegments;
                    meshName = "Medium";
                    break;
                }
            case (int)MapSize.Large: {
                    mapWidth = mapLargeWidth;
                    mapLength = mapLargeLength;
                    mapHeight = mapLargeHeight;
                    mapGoalWidth = mapLargeGoalWidth;
                    mapGoalHeight = mapLargeGoalHeight;
                    mapGoalDepth = mapLargeGoalDepth;
                    mapGoalPostRadius = mapLargeGoalPostRadius;
                    mapGoalPostSegments = mapLargeGoalPostSegments;
                    meshName = "Large";
                    break;
                }
        }

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>
        {
            //Floor
            new Vector3(0, 0, 0),                       //0
            new Vector3(mapWidth, 0, 0),                //1
            new Vector3(0, 0, mapLength),               //2
            new Vector3(mapWidth, 0, mapLength),        //3

            //Ceiling
            new Vector3(0, mapHeight, 0),               //4
            new Vector3(mapWidth, mapHeight, 0),        //5
            new Vector3(0, mapHeight, mapLength),       //6
            new Vector3(mapWidth, mapHeight, mapLength),//7

            //South wall top
            new Vector3(0, mapGoalHeight, 0),           //8
            new Vector3(mapWidth, mapGoalHeight, 0),    //9
            
            //North wall top
            new Vector3(0, mapGoalHeight, mapLength),   //10
            new Vector3(mapWidth, mapGoalHeight, mapLength),//11

            //South wall goal
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0), //12
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),//13
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, 0), //14
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, 0),//15

            //North wall goal
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength), //16
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),//17
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength), //18
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength),//19

            //South wall goal back
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, -mapGoalDepth), //20
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, -mapGoalDepth),//21
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, -mapGoalDepth), //22
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, -mapGoalDepth),//23

            //North wall goal back
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength + mapGoalDepth), //24
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength + mapGoalDepth),//25
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength + mapGoalDepth), //26
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength + mapGoalDepth),//27

            //West side wall 
            new Vector3(0, 0, 0),                       //28
            new Vector3(0, 0, mapLength),               //29
            new Vector3(0, mapHeight, 0),               //30
            new Vector3(0, mapHeight, mapLength),       //31

            //East side wall
            new Vector3(mapWidth, 0, 0),                //32
            new Vector3(mapWidth, 0, mapLength),        //33
            new Vector3(mapWidth, mapHeight, 0),        //34
            new Vector3(mapWidth, mapHeight, mapLength),//35

            //South wall top additional
            new Vector3(0, mapHeight, 0),               //36
            new Vector3(mapWidth, mapHeight, 0),        //37

            //North wall top additional
            new Vector3(0, mapHeight, mapLength),       //6 38
            new Vector3(mapWidth, mapHeight, mapLength),//7 39

            //South wall goal west additional 
            new Vector3(0, 0, 0),                       //0 40
            new Vector3(0, mapGoalHeight, 0),           //8 41

            //South wall goal east additional
            new Vector3(mapWidth, 0, 0),                //1 42
            new Vector3(mapWidth, mapGoalHeight, 0),    //9 43

            //North wall goal west additional
            new Vector3(0, 0, mapLength),               //2 44
            new Vector3(0, mapGoalHeight, mapLength),   //10 45

            //North wall goal east additional
            new Vector3(mapWidth, 0, mapLength),        //3 46
            new Vector3(mapWidth, mapGoalHeight, mapLength),//11 47

            //South wall goal net west additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0), //12 48
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, 0), //14 49

            //South wall goal net east additional
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),//13 50
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, 0),//15 51

            //South wall goal net top additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0), //12 52
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),//13 53
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, -mapGoalDepth), //20 54
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, -mapGoalDepth),//21 55

            //South wall goal net bottom additional
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, 0), //14 56
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, 0),//15 57
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, -mapGoalDepth), //22 58

            //South wall goal net back additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, -mapGoalDepth), //20 59
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, -mapGoalDepth),//21 60
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, -mapGoalDepth), //22 61
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, -mapGoalDepth),//23 62


            //North wall goal net west additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength), //16 63
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength), //18 64


            //North wall goal net east additional
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),//17 65
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength),//19 66


            //North wall goal net top additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength), //16 67
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),//17 68
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength + mapGoalDepth), //24 69
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength + mapGoalDepth),//25 70

            //North wall goal net bottom additional
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength), //18 71
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength),//19 72
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength + mapGoalDepth), //26 73

            //North wall goal net back additional
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength + mapGoalDepth), //24 74
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength + mapGoalDepth),//25 75
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength + mapGoalDepth), //26 76
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength + mapGoalDepth),//27 77

            //South wall goal net bottom additional 2
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, -mapGoalDepth),//23 78

            //North wall goal net bottom additional 2
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength + mapGoalDepth),//27 79

            //South wall goal line
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0), //80
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),//81
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, 0), //82
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, 0),//83

            //North wall goal line
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength), //84
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),//85
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength), //86
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength),//87

        };

        //South wall goal post west bottom
        int southWallGoalPostWestBottomFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, 0),
            0,
            false,
            false
        ));

        //South wall goal post west top
        int southWallGoalPostWestTopFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0),
            0,
            false,
            true
        ));

        //South wall goal post east bottom
        int southWallGoalPostEastBottomFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, 0),
            90,
            false,
            false
        ));

        //South wall goal post east top
        int southWallGoalPostEastTopFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),
            90,
            false,
            true
        ));

        //South wall goal post top west
        int southWallGoalPostTopWestFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, 0),
            0,
            true,
            true,
            true
        ));

        //South wall goal post top east
        int southWallGoalPostTopEastFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, 0),
            0,
            true,
            true
        ));

        //North wall goal post west bottom
        int northWallGoalPostWestBottomFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, 0, mapLength),
            270,
            false,
            false
        ));

        //North wall goal post west top
        int northWallGoalPostWestTopFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength),
            270,
            false,
            true
        ));

        //North wall goal post east bottom
        int northWallGoalPostEastBottomFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), 0, mapLength),
            180,
            false,
            false
        ));

        //North wall goal post east top
        int northWallGoalPostEastTopFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),
            180,
            false,
            true
        ));

        //North wall goal post top west
        int northWallGoalPostTopWestFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3((mapWidth - mapGoalWidth) / 2, mapGoalHeight, mapLength),
            270,
            true,
            true,
            true
        ));

        //South wall goal post top east
        int northWallGoalPostTopEastFirstVertexIndex = vertices.Count;
        vertices.AddRange(GetGoalPostVertices(
            mapGoalPostSegments,
            mapGoalPostRadius,
            new Vector3(mapWidth - ((mapWidth - mapGoalWidth) / 2), mapGoalHeight, mapLength),
            270,
            true,
            true
        ));


        mesh.vertices = vertices.ToArray();
        List<int> tris = new List<int>
        {
            //Floor
            0, 2, 1,
            2, 3, 1,
            
            //Ceiling
            5, 6, 4,
            5, 7, 6,

            //West side wall
            28, 30, 31,
            28, 31, 29,

            //East side wall
            35, 34, 32,
            33, 35, 32,

            //South wall top
            37, 36, 8,
            9, 37, 8,

            //North wall top
            10, 38, 39,
            10, 39, 11,

            //South wall goal west
            40, 12, 41,
            14, 12, 40,

            //South wall goal east
            15, 43, 13,
            42, 43, 15,

            //North wall goal west
            45, 16, 44,
            44, 16, 18,

            //North wall goal east
            17, 47, 19,
            19, 47, 46,

            //South wall goal net west
            20, 48, 49,
            20, 49, 22,

            //South wall goal net east
            51, 50, 21,
            23, 51, 21,

            //South wall goal net top
            55, 52, 54,
            53, 52, 55,

            //South wall goal net bottom
            58, 56, 57,
            58, 57, 78,

            //South wall goal net back
            61, 60, 59,
            62, 60, 61,

            //North wall goal net west
            64, 63, 24,
            26, 64, 24,

            //North wall goal net east
            25, 65, 66,
            25, 66, 27,

            //North wall goal net top
            69, 67, 70,
            70, 67, 68,

            //North wall goal net bottom
            72, 71, 73,
            79, 72, 73,

            //North wall goal net back
            74, 75, 76,
            76, 75, 77,

            //South wall goal line
            82, 81, 80,
            83, 81, 82,

            //North wall goal line
            84, 85, 86,
            86, 85, 87
        };

        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, southWallGoalPostWestBottomFirstVertexIndex, southWallGoalPostWestTopFirstVertexIndex));
        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, southWallGoalPostEastBottomFirstVertexIndex, southWallGoalPostEastTopFirstVertexIndex));
        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, northWallGoalPostWestBottomFirstVertexIndex, northWallGoalPostWestTopFirstVertexIndex));
        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, northWallGoalPostEastBottomFirstVertexIndex, northWallGoalPostEastTopFirstVertexIndex));
        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, southWallGoalPostTopWestFirstVertexIndex, southWallGoalPostTopEastFirstVertexIndex));
        tris.AddRange(GetGoalPostTris(mapGoalPostSegments, northWallGoalPostTopWestFirstVertexIndex, northWallGoalPostTopEastFirstVertexIndex));


        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();

        Unwrapping.GenerateSecondaryUVSet(mesh);

        mesh.uv = mesh.uv2;

        string meshPath = $"Assets/Generated/{meshName}.mesh";
        if(AssetDatabase.FindAssets(meshPath).Length > 0) {
            AssetDatabase.DeleteAsset(meshPath);
        }
        AssetDatabase.CreateAsset(mesh, meshPath);
        AssetDatabase.SaveAssets();
#endif        

    }

    private Vector3 GetGoalPostVector(int segmentIndex, int segments, float radius, float baseRotation, bool topPost, bool diagonal, bool revertedTop) {
        float rotationAmount = (270f / (float)segments * (float)segmentIndex) - baseRotation;
        float x = 0;

        if (diagonal) {
            float b = baseRotation;
            if ((b >= 180 && !revertedTop) || (b <= 180 && revertedTop)) {
                b -= 180;
            }
            x = (float)segmentIndex / ((float)segments * (2f / 3f));
            x = (float)Math.Sin((x - (0.5f - (b / 180))) / (1 / Math.PI));
        }

        Vector3 vector = topPost ? new Vector3(diagonal ? x * -radius : 0, radius, 0) : new Vector3(-radius, diagonal ? x * -radius : 0, 0);
        vector = Quaternion.AngleAxis(rotationAmount, topPost ? Vector3.right : Vector3.up) * vector;
        return vector;
    }


    private Vector3[] GetGoalPostVertices(int segments, float radius, Vector3 center, float baseRotation, bool topPost, bool diagonal, bool revertedTop = false) {
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i <= segments; i++) {
            vertices.Add(center + GetGoalPostVector(i, segments, radius, baseRotation, topPost, diagonal, revertedTop));
        }
        return vertices.ToArray();
    }

    private int[] GetGoalPostTris(int segments, int firstBottomVertexIndex, int firstTopVertexIndex) {
        List<int> tris = new List<int>();
        for (int i = 0; i < segments; i++) {
            int[] quad = {
                i + firstBottomVertexIndex,
                i + firstBottomVertexIndex + 1,
                i + firstTopVertexIndex,
                i + firstBottomVertexIndex + 1,
                i + firstTopVertexIndex + 1,
                i + firstTopVertexIndex,
            };
            tris.AddRange(quad);
        }
        return tris.ToArray();
    }
}

