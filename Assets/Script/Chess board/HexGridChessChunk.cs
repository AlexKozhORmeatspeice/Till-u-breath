using System;
using Script;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChessChunk : MonoBehaviour
{
    private static Color tex1Color = new Color(1f, 0f, 0f);
    private static Color tex2Color = new Color(0f, 1f, 0f);
    private static Color tex3Color = new Color(0f, 0f, 1f);
    private HexChessCell[] cells;

    public HexMesh terrain;

    private void Awake()
    {
        cells = new HexChessCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }
    
    private void LateUpdate()
    {
        Triangulate();
        enabled = false;
    }

    public void Refresh()
    {
        enabled = true;
    }

    public void AddCell(int ind, HexChessCell cell)
    {
        cells[ind] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
    }
    
    public void Triangulate()
    {
        terrain.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        terrain.Apply();
    }

    private void Triangulate(HexChessCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }

    }

    private void Triangulate(HexDirection dir, HexChessCell cell)
    {
        Vector3 center = cell.Position;
        Vector3 bridge = HexMetrics.GetBridge(dir);

        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(dir),
            center + HexMetrics.GetSecondSolidCorner(dir));
        TriangulateCell(dir, cell, center, e);
        if (dir <= HexDirection.SE)
        {
            TriangulateConnection(dir, cell, e);
        }
    }

    void TriangulateCell(HexDirection dir, HexChessCell cell, Vector3 center, EdgeVertices e)
    {
        TriangulateEdgeFan(center, e, cell.TerrainTypeInd);
    }
    void TriangulateConnection(HexDirection dir, HexChessCell cell, EdgeVertices e1)
    {
        HexChessCell neighbor = cell.GetNeighbor(dir);
        if (neighbor == null)
            return;
        
        Vector3 bridge = HexMetrics.GetBridge(dir);
        bridge.y = neighbor.Position.y - cell.Position.y;
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        
        if (cell.GetEdgeType(dir) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        }
        else
        {
            TriangulateEdgeStrip(
                e1, tex1Color, cell.TerrainTypeInd, 
                e2, tex2Color, neighbor.TerrainTypeInd);
        }

        HexChessCell nextNeighbor = cell.GetNeighbor(dir.Next());

        if (dir <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(dir.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
        }
    }

    void TriangulateEdgeTerraces(EdgeVertices begin, HexChessCell beginCell, EdgeVertices end, HexChessCell endCell, bool hasRoad = false)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(tex1Color, tex2Color, 1);
        float t1 = beginCell.TerrainTypeInd;
        float t2 = endCell.TerrainTypeInd;
        
        TriangulateEdgeStrip(begin, tex1Color, t1, e2, c2, t2, hasRoad);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;

            e2 = EdgeVertices.TerraceLerp(begin, end, step);
            c2 = HexMetrics.TerraceLerp(tex1Color, tex2Color, step);

            TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2, hasRoad);
        }

        TriangulateEdgeStrip(e2, c2, t1, end, tex2Color, t2, hasRoad);
    }

    void TriangulateCorner(Vector3 bottom, HexChessCell bottomCell,
        Vector3 left, HexChessCell leftCell,
        Vector3 right, HexChessCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }

        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else
        {
            terrain.AddTriangle(bottom, left, right);
            terrain.AddTriangleColors(tex1Color, tex2Color, tex3Color);
            Vector3 types;
            types.x = bottomCell.TerrainTypeInd;
            types.y = leftCell.TerrainTypeInd;
            types.z = rightCell.TerrainTypeInd;
            terrain.AddTriangleTerrainTypes(types);
        }
    }

    void TriangulateCornerTerraces(Vector3 bottom, HexChessCell bottomCell,
        Vector3 left, HexChessCell leftCell,
        Vector3 right, HexChessCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(bottom, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(bottom, right, 1);

        Color c3 = HexMetrics.TerraceLerp(tex1Color, tex2Color, 1);
        Color c4 = HexMetrics.TerraceLerp(tex1Color, tex3Color, 1);

        Vector3 types;
        types.x = bottomCell.TerrainTypeInd;
        types.y = leftCell.TerrainTypeInd;
        types.z = rightCell.TerrainTypeInd; 
        
        terrain.AddTriangle(bottom, v3, v4);
        terrain.AddTriangleColors(bottomCell.Color, c3, c4);
        terrain.AddTriangleTerrainTypes(types);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(bottom, left, step);
            v4 = HexMetrics.TerraceLerp(bottom, right, step);
            c3 = HexMetrics.TerraceLerp(tex1Color, tex2Color, step);
            c4 = HexMetrics.TerraceLerp(tex1Color, tex3Color, step);

            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadColors(c1, c2, c3, c4);
            terrain.AddQuadTerrainTypes(types);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadColors(c3, c4, tex2Color, tex3Color);
        terrain.AddQuadTerrainTypes(types);
    }

    void TriangulateCornerTerracesCliff(Vector3 begin, HexChessCell beginCell,
                                        Vector3 left,  HexChessCell leftCell,
                                        Vector3 right, HexChessCell rightCell)
    {
        float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
        Color boundaryC = Color.Lerp(tex1Color, tex3Color, b);
        Vector3 types;
        types.x = beginCell.TerrainTypeInd;
        types.y = leftCell.TerrainTypeInd;
        types.z = rightCell.TerrainTypeInd;
        
        
        TriangulateBoundaryTriangle(begin, tex1Color, left, tex2Color, boundary, boundaryC, types);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, tex2Color, right, tex3Color, boundary, boundaryC, types);
        }
        else
        {
            terrain.AddTriangleUnpertubed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColors(tex2Color, tex3Color, boundaryC);
            terrain.AddTriangleTerrainTypes(types);
        }
    }
    void TriangulateCornerCliffTerraces(Vector3 begin, HexChessCell beginCell,
                                        Vector3 left,  HexChessCell leftCell,
                                        Vector3 right, HexChessCell rightCell)
    {
        float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp( HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryC = Color.Lerp(tex1Color, tex2Color, b);
        Vector3 types;
        types.x = beginCell.TerrainTypeInd;
        types.y = leftCell.TerrainTypeInd;
        types.z = rightCell.TerrainTypeInd;
        
        TriangulateBoundaryTriangle(right, tex3Color, begin, tex1Color, boundary, boundaryC, types);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, tex2Color, right, tex3Color, boundary, boundaryC, types);
        }
        else
        {
            terrain.AddTriangleUnpertubed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColors(tex2Color, tex3Color, boundaryC);
            terrain.AddTriangleTerrainTypes(types);
        }
    }

    void TriangulateBoundaryTriangle(Vector3 begin, Color beginColor,
                                        Vector3 left, Color leftColor,
                                        Vector3 boundary, Color boundaryC, Vector3 types)
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);
        
        terrain.AddTriangleUnpertubed(HexMetrics.Perturb(begin), v2, boundary);
        terrain.AddTriangleColors(beginColor, c2, boundaryC);
        terrain.AddTriangleTerrainTypes(types);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, step));
            c2 = HexMetrics.TerraceLerp(beginColor, leftColor, step);
            
            terrain.AddTriangleUnpertubed(v1, v2, boundary);
            terrain.AddTriangleColors(c1, c2, boundaryC);
            terrain.AddTriangleTerrainTypes(types);
        }
        
        terrain.AddTriangleUnpertubed(v2, HexMetrics.Perturb(left), boundary);
        terrain.AddTriangleColors(c2, leftColor, boundaryC);
        terrain.AddTriangleTerrainTypes(types);
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float type)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        terrain.AddTriangle(center, edge.v4, edge.v5);

        terrain.AddTriangleColor(tex1Color);
        terrain.AddTriangleColor(tex1Color);
        terrain.AddTriangleColor(tex1Color);
        terrain.AddTriangleColor(tex1Color);

        Vector3 types;
        types.x = types.y = types.z = type;
        terrain.AddTriangleTerrainTypes(types);
        terrain.AddTriangleTerrainTypes(types);
        terrain.AddTriangleTerrainTypes(types);
        terrain.AddTriangleTerrainTypes(types);
    }
    
    
    void TriangulateEdgeStrip(
        EdgeVertices e1, Color c1, float type1,
        EdgeVertices e2, Color c2, float type2,
        bool hasRoad = false)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

        terrain.AddQuadColor(c1, c2);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuadColor(c1, c2);

        Vector3 types;
        types.x = types.z = type1;
        types.y = type2;
        terrain.AddQuadTerrainTypes(types);
        terrain.AddQuadTerrainTypes(types);
        terrain.AddQuadTerrainTypes(types);
        terrain.AddQuadTerrainTypes(types);
    }
}