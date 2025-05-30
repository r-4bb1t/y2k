using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject deskPrefab;
    [SerializeField] private int numberOfDesks = 10;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private float gridSpacing = 2f;

    void Start()
    {
        GenerateDesks();
    }

    void GenerateDesks()
    {
        List<Vector3> gridPositions = new List<Vector3>();
        int yIndex = 0;

        for (float y = minY; y <= maxY; y += gridSpacing)
        {
            float offsetX = (yIndex % 2 == 0) ? 0.5f : 0f;

            for (float x = minX; x <= maxX; x += gridSpacing)
            {
                gridPositions.Add(new Vector3(x + offsetX, y + 0.5f, -10f));
            }

            yIndex++;
        }

        Shuffle(gridPositions);

        for (int i = 0; i < Mathf.Min(numberOfDesks, gridPositions.Count); i++)
        {
            Instantiate(deskPrefab, gridPositions[i], Quaternion.identity);
        }
    }


    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void Update()
    {

    }
}
