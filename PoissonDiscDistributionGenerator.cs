using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiscDistributionGenerator
{
    private static int MAX_ATTEMPTS = 30;

    /// <summary>
    /// Generates an even Poisson Distribution of points on a disc, with a max radius and minimum spread of points
    /// https://en.wikipedia.org/wiki/Poisson_distribution
    /// </summary>
    public static List<Vector3> GeneratePoints(ref List<Vector3> points, float spread, float maxRadius)
    {
        points.Clear();

        List<Vector2> newPoints = new List<Vector2>();

        Vector2 initialPoint = Random.insideUnitCircle;
        Vector3 newPoint = new Vector3();

        //Warm sampler with initial point
        newPoints.Add(initialPoint);

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
                //Generate a point from a random direction
                float rangle = Random.value * Mathf.PI * 2.0f;

                Vector2 direction = new Vector2(Mathf.Sin(rangle), Mathf.Cos(rangle));
                direction = centre + direction * Random.Range(spread, spread * 2.0f);

                //Check position to see if it's alright
                if (ValidPosition(direction, spread, maxRadius, points))
                {
                    //Add to final points and add to points to check from
                    newPoints.Add(direction);

                    points.Add(direction);

                    success = true;
                }
            }

            //Remove this point from the checklist as it's probably not got any room near it
            if (!success)
            {
                newPoints.RemoveAt(index);
            }
        }

        return points;
    }

    /// <summary>
    /// Checks the validity of a position, assuming that the centre of the disc is world zero.
    /// </summary>
    public static bool ValidPosition(Vector3 point, float spread, float maxRadius, List<Vector3> points)
    {
        float sqSpread = spread * spread;
        float sqRadius = maxRadius * maxRadius;

        //If it's off the disc, return early
        if (point.sqrMagnitude > sqRadius)
        {
            return false;
        }

        //Otherwise, check every point to see if it's far enough away
        for (int i = 0; i < points.Count; i++)
        {
            float sqDistance = (point - points[i]).sqrMagnitude;

            //Point is too close
            if (sqDistance < sqSpread)
            { 
                return false;
            }
        }

        return true;
    }
}
