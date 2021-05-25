using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator 
{
    public static void CreateWalls(HashSet<Vector2Int> floorPos, TilemapVisualizer tilemapVisualizer)
    {
        var basicwallPos = FindWallsInDirections(floorPos, Direction2D.cardinalDirList);
        var cornerWallPos = FindWallsInDirections(floorPos, Direction2D.diagonalDirList);
        CreateBasicWalls(tilemapVisualizer, basicwallPos, floorPos);
        CreateCornerWalls(tilemapVisualizer, cornerWallPos, floorPos);
    }

    private static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPos, HashSet<Vector2Int> floorPos)
    {
        foreach (var position in cornerWallPos)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.eightDirList)
            {
                var neighbourPos = position + direction;
                if(floorPos.Contains(neighbourPos))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }

    private static void CreateBasicWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicwallPos, HashSet<Vector2Int> floorPos)
    {
        foreach (var position in basicwallPos)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirList)
            {
                var neighbourPos = position + direction;
                if(floorPos.Contains(neighbourPos))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
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
