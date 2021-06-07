using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PoissonDiscDistributionGenerator
{
    private static int MAX_ATTEMPTS = 3;

    /// <summary>
    /// Generates an even Poisson Distribution of points on a disc, with a max radius and minimum spread of points
    /// https://en.wikipedia.org/wiki/Poisson_distribution
    /// </summary>
    public static void GeneratePoints(List<Vector2> points, float spread, float maxRadius, int seed)
    {
        Random.InitState(seed);
        
        points.Clear();

        Debug.Log("Cleared points");
        int cellSize = Mathf.CeilToInt((maxRadius * 2) / spread);
        Vector2[,] grid = new Vector2[cellSize, cellSize];

        List<Vector2> newPoints = new List<Vector2>();

        Vector2 initialPoint = Random.insideUnitCircle;
        Vector2 newPoint = new Vector2();

        //Warm sampler with initial point
        newPoints.Add(initialPoint);

        Debug.Log("Seeded points");
        
        //Whilst we potentially still have points
        while (newPoints.Count > 0)
        {
            //Get a random point to check
            int index = Random.Range(0, newPoints.Count);
            Vector2 centre = newPoints[index];
            bool success = false;
            
            //Try each point up to our limit
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                Debug.Log("Trying point");
                //Generate a point from a random direction
                float rangle = Random.value * Mathf.PI * 2.0f;

                Vector2 direction = new Vector2(Mathf.Sin(rangle), Mathf.Cos(rangle));
                direction = centre + direction * Random.Range(spread, spread * 2.0f);

                //If it's off the disc, return early
                if (direction.sqrMagnitude > Mathf.Pow(maxRadius / 2, 2))
                {
                    continue;
                }
                
                Vector2Int gridPos = ToGridPos(direction, cellSize, maxRadius, maxRadius / 2);

                //Check position to see if it's alright
                if (ValidPosition(gridPos, direction, spread, grid))
                {
                    //Add to final points and add to points to check from
                    newPoints.Add(direction);
                    points.Add(direction);

                    grid[gridPos.x, gridPos.y] = direction;

                    success = true;
                }
            }

            //Remove this point from the checklist as it's probably not got any room near it
            if (!success)
            {
                newPoints.RemoveAt(index);
            }
        }

        bool done = true;
    }

    /// <summary>
    /// Checks the validity of a position, assuming that the centre of the disc is world zero.
    /// </summary>
    private static bool ValidPosition(Vector2Int point, Vector2 direction, float spread, Vector2[,] grid)
    {
        float sqSpread = spread * spread;

        int xMin = Mathf.Max(point.x - 1, 0);
        int yMin = Mathf.Max(point.y - 1, 0);
        int xMax = Mathf.Min(point.x + 1, grid.GetLength(0)  - 1);
        int yMax = Mathf.Min(point.y + 1, grid.GetLength(1) - 1);

        for (int y = yMin; y < yMax; y++)
        {
            for (int x = xMin; x < xMax; x++)
            {
                Vector2 sample = grid[x, y];
                if (sample != Vector2.zero)
                {
                    if ((sample - direction).sqrMagnitude < sqSpread)
                        return false;
                }
            }
        }
        
        return true;
    }

    private static Vector2Int ToGridPos(Vector2 position, float cellSize, float maxRadius, float offset)
    {
        int xGrid = Mathf.FloorToInt((position.x + offset) / (maxRadius / cellSize));
        int yGrid = Mathf.FloorToInt((position.y + offset) / (maxRadius / cellSize));
        
        return new Vector2Int(xGrid, yGrid);
    }
}
