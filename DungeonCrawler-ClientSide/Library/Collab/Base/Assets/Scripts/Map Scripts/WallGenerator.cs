using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator 
{
    public static void CreateWalls(HashSet<Vector2Int> floorPos, TilemapVisualizer tilemapVisualizer)
    {
        var basicwallPos = FindWallsInDirections(floorPos, Direction2D.cardinalDirList);
        foreach (var position in basicwallPos)
        {
            tilemapVisualizer.PaintSingleBasicWall(position);
        }
    }

    //Find what positions need walls
    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPos, List<Vector2Int> directionsList)
    {
        HashSet<Vector2Int> wallPos = new HashSet<Vector2Int>();
        foreach (var position in floorPos)
        {
            foreach (var direction in directionsList)
            {
                var neighbourPos = position + direction;
                if (floorPos.Contains(neighbourPos) == false)
                    wallPos.Add(neighbourPos);
            }
        }
        return wallPos;
    }
}
