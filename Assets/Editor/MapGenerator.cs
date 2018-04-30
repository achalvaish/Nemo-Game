using UnityEngine;
using System.Collections;
using UnityEditor;

public class MapGenerator : ScriptableWizard
{

    public GameObject obj;
    public int height, width;

    [MenuItem("My Tools/Map Generator")]
    static void MapGeneratorWizard()
    {
        DisplayWizard<MapGenerator>("Map Generator");
    }

    void OnWizardCreate()
    {
        float minX = -(float)width / 2.0f;
        float minY = -(float)height / 2.0f;

        GameObject Terrain = new GameObject();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    Instantiate(obj, Terrain.transform);
                    obj.transform.position = new Vector3(minX + x, minY + y, 0);
                }
            }
        }
    }
}
