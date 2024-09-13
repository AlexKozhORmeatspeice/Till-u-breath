using System;
using Script;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
    private HexCell[] cells;

    public HexMesh terrain, rivers;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        ShowUI(false);
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

    public void AddCell(int ind, HexCell cell)
    {
        cells[ind] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(canvas.transform, false);
    }

    public void ShowUI(bool visible)
    {
        canvas.gameObject.SetActive(visible);
    }
    
    public void Triangulate()
    {
        terrain.Clear();
        rivers.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        terrain.Apply();
        rivers.Apply();
    }

    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    private void Triangulate(HexDirection dir, HexCell cell)
    {
        Vector3 center = cell.Position;
        Vector3 bridge = HexMetrics.GetBridge(dir);

        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(dir),
            center + HexMetrics.GetSecondSolidCorner(dir));

        if (cell.HasRiver)
        {
            if (cell.HasRiverThroughEdge(dir))
            {
                e.v3.y = cell.StreamBedY;
                if (cell.HasRiverBeginOrEnd)
                {
                    TriangulateWithRiverBeginOrEnd(dir, cell, center, e);
                }
                else
                {
                    TriangulateWithRiver(dir, cell, center, e);
                }
            }
            else
            {
                TriangulateAdjacentToRiver(dir, cell, center, e);
            }
        }
        else
        {
            TriangulateEdgeFan(center, e, cell.Color);
        }

        if (dir <= HexDirection.SE)
        {
            TriangulateConnection(dir, cell, e);
        }
    }
    
    void TriangulateWithRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        Vector3 centerL, centerR;
        if (cell.HasRiverThroughEdge(dir.Opposite()))
        {
            centerL = center + HexMetrics.GetFirstSolidCorner(dir.Previous()) * 0.25f;
            centerR = center + HexMetrics.GetSecondSolidCorner(dir.Next()) * 0.25f;
        }
        else if (cell.HasRiverThroughEdge(dir.Next()))
        {
            centerL = center;
            centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
        }
        else if (cell.HasRiverThroughEdge(dir.Previous()))
        {
            centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
            centerR = center; 
        }
        else if (cell.HasRiverThroughEdge(dir.Next2()))
        {
            centerL = center;
            centerR = center + HexMetrics.GetSolidEdgeMiddle(dir.Next()) * 0.5f * HexMetrics.innerToOuter;
        }
        else
        {
            centerL = center + HexMetrics.GetSolidEdgeMiddle(dir.Previous()) * 0.5f * HexMetrics.innerToOuter;
            centerR = center;
        }

        center = Vector3.Lerp(centerL, centerR, 0.5f);
        
        EdgeVertices m =
            new EdgeVertices(
                Vector3.Lerp(centerL, e.v1, 0.5f),
                Vector3.Lerp(centerR, e.v5, 0.5f),
                1f/6f);
        

        m.v3.y = center.y = e.v3.y;


        terrain.AddTriangle(m.v1, m.v2, centerL);
        terrain.AddTriangleColor(cell.Color);
        terrain.AddTriangle(m.v4, m.v5, centerR);
        terrain.AddTriangleColor(cell.Color);

        terrain.AddQuad(centerL, center, m.v2, m.v3);
        terrain.AddQuadColor(cell.Color, cell.Color);
        terrain.AddQuad(center, centerR, m.v3, m.v4);
        terrain.AddQuadColor(cell.Color, cell.Color);
        
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

        bool reversed = cell.IncomingRiver == dir;
        
        TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
        TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);

    }

    void TriangulateWithRiverBeginOrEnd(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        EdgeVertices m =
            new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f));
        m.v3.y = e.v3.y;
        
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
        TriangulateEdgeFan(center, m, cell.Color);

        bool reversed = cell.HasIncomingRiver;
        TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);

        center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
        rivers.AddTriangle(center, m.v2, m.v4);
        if (reversed)
        {
            rivers.AddTriangleUV(
                new Vector2(0.5f, 0.4f),
                new Vector2(1f, 0.2f),
                new Vector2(0f, 0.2f)
            );
        }
        else
        {
            rivers.AddTriangleUV(
                new Vector2(0.5f, 0.4f),
                new Vector2(0f, 0.6f),
                new Vector2(1f, 0.6f)
            );
        }
    }

    void TriangulateAdjacentToRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        if (cell.HasRiverThroughEdge(dir.Next()))
        {
            if (cell.HasRiverThroughEdge(dir.Previous()))
            {
                center += HexMetrics.GetSolidEdgeMiddle(dir) * 0.5f * HexMetrics.innerToOuter;
            }
            else if (cell.HasRiverThroughEdge(dir.Previous2()))
            {
                center += HexMetrics.GetFirstSolidCorner(dir) * 0.25f;
            }
        }
        else if (cell.HasRiverThroughEdge(dir.Previous()) && cell.HasRiverThroughEdge(dir.Next2()))
        {
            center += HexMetrics.GetSecondSolidCorner(dir) * 0.25f;
        }
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f));
        
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
        TriangulateEdgeFan(center, m, cell.Color);
    }
    
    void TriangulateConnection(HexDirection dir, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(dir);
        if (neighbor == null)
            return;
        
        Vector3 bridge = HexMetrics.GetBridge(dir);
        bridge.y = neighbor.Position.y - cell.Position.y;
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        if (cell.HasRiverThroughEdge(dir))
        {
            e2.v3.y = neighbor.StreamBedY;
            TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4,
                                cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                                cell.HasIncomingRiver && cell.IncomingRiver == dir);
        }
        
        if (cell.GetEdgeType(dir) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        }
        else
        {
            TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);
        }

        HexCell nextNeighbor = cell.GetNeighbor(dir.Next());

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

    void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);
        
        
        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;

            e2 = EdgeVertices.TerraceLerp(begin, end, step);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, step);

            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        TriangulateEdgeStrip(e2, c2, end, endCell.Color);
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
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
                return;
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
            terrain.AddTriangleColors(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    void TriangulateCornerTerraces(Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(bottom, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(bottom, right, 1);

        Color c3 = HexMetrics.TerraceLerp(bottomCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerp(bottomCell.Color, rightCell.Color, 1);

        terrain.AddTriangle(bottom, v3, v4);
        terrain.AddTriangleColors(bottomCell.Color, c3, c4);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(bottom, left, step);
            v4 = HexMetrics.TerraceLerp(bottom, right, step);
            c3 = HexMetrics.TerraceLerp(bottomCell.Color, leftCell.Color, step);
            c4 = HexMetrics.TerraceLerp(bottomCell.Color, rightCell.Color, step);

            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadColors(c1, c2, c3, c4);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadColors(c3, c4, leftCell.Color, rightCell.Color);
    }

    void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell,
                                        Vector3 left,  HexCell leftCell,
                                        Vector3 right, HexCell rightCell)
    {
        float b = Mathf.Abs(1f / (rightCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
        Color boundaryC = Color.Lerp(beginCell.Color, rightCell.Color, b);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryC);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryC);
        }
        else
        {
            terrain.AddTriangleUnpertubed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColors(leftCell.Color, rightCell.Color, boundaryC);
        }
    }
    void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell,
                                        Vector3 left,  HexCell leftCell,
                                        Vector3 right, HexCell rightCell)
    {
        float b = Mathf.Abs(1f / (leftCell.Elevation - beginCell.Elevation));
        Vector3 boundary = Vector3.Lerp( HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryC = Color.Lerp(beginCell.Color, leftCell.Color, b);

        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryC);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryC);
        }
        else
        {
            terrain.AddTriangleUnpertubed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColors(leftCell.Color, rightCell.Color, boundaryC);
        }
    }

    void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell,
                                        Vector3 left, HexCell leftCell,
                                        Vector3 boundary, Color boundaryC)
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        
        terrain.AddTriangleUnpertubed(HexMetrics.Perturb(begin), v2, boundary);
        terrain.AddTriangleColors(beginCell.Color, c2, boundaryC);

        for (int step = 2; step < HexMetrics.terraceSteps; step++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, step));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, step);
            
            terrain.AddTriangleUnpertubed(v1, v2, boundary);
            terrain.AddTriangleColors(c1, c2, boundaryC);
        }
        
        terrain.AddTriangleUnpertubed(v2, HexMetrics.Perturb(left), boundary);
        terrain.AddTriangleColors(c2, leftCell.Color, boundaryC);
    }

    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, 
                            float y1, float y2, 
                            float v, bool isReversed)
    {
        v1.y = v2.y = y1;
        v3.y = v4.y = y2;
        
        rivers.AddQuad(v1, v2, v3, v4);

        if (isReversed)
        {
            rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
        }
        else
        {
            rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
        }
    }
    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, 
                             float y, float v, bool isReversed)
    {
        TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, isReversed);
    }
    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v4, edge.v5);
        terrain.AddTriangleColor(color);
    }
    
    
    void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        terrain.AddQuadColor(c1, c2);
    }
}