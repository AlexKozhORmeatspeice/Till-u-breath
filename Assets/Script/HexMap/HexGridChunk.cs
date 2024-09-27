using System;
using Script;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
    private static Color tex1Color = new Color(1f, 0f, 0f);
    private static Color tex2Color = new Color(0f, 1f, 0f);
    private static Color tex3Color = new Color(0f, 0f, 1f);
    private HexCell[] cells;

    public HexMesh terrain, rivers, roads, water, waterShore;
    public HexFeatureManager features;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
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
        roads.Clear();
        water.Clear(); 
        waterShore.Clear();
        features.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        terrain.Apply();
        rivers.Apply();
        roads.Apply();
        water.Apply();
        waterShore.Apply();
        features.Apply();
    }

    private void Triangulate(HexCell cell)
    {
        HexFeatureManager.SetFeature(HexMetrics.featureCollections[cell.FeatureCollectionInd]);
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
        
        if(!cell.IsUnderwater && !cell.HasRiver && !cell.HasRoad)
            features.AddFeature(cell, cell.Position);
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
            TriangulateWithoutRiver(dir, cell, center, e);
            
            if(!cell.IsUnderwater && !cell.HasRoadThroughEdge(dir))
                features.AddFeature(cell,(center + e.v1 + e.v5) * (1f / 3f));
        }

        if (dir <= HexDirection.SE)
        {
            TriangulateConnection(dir, cell, e);
        }

        if (cell.IsUnderwater)
        {
            TriangulateWater(dir, cell, center);
        }
    }

    void TriangulateWater(HexDirection dir, HexCell cell, Vector3 center)
    {
        center.y = cell.WaterSurfaceY;
        HexCell neighbor = cell.GetNeighbor(dir);
        if (neighbor != null && !neighbor.IsUnderwater)
        {
            TriangulateWaterShore(dir, cell, neighbor, center);
        }
        else
        {
            TriangulateOpenWater(dir, cell, neighbor, center);
        }
    }

    private void TriangulateOpenWater(HexDirection dir, HexCell cell, HexCell neighbor, Vector3 center)
    {
        Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(dir);
        Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(dir);

        water.AddTriangle(center, c1, c2);

        if (dir <= HexDirection.SE && neighbor != null)
        {
            if(neighbor == null || !neighbor.IsUnderwater)
                return;

            Vector3 bridge = HexMetrics.GetWaterBridge(dir);
            Vector3 e1 = c1 + bridge;
            Vector3 e2 = c2 + bridge;
            water.AddQuad(c1, c2, e1, e2);

            if (dir <= HexDirection.E)
            {
                HexCell nextNeighbor = cell.GetNeighbor(dir.Next());
                if (nextNeighbor == null || !nextNeighbor.IsUnderwater)
                    return;
                
                water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(dir.Next()));
            }
        }
    }

    private void TriangulateWaterShore(HexDirection dir, HexCell cell, HexCell neighbor, Vector3 center)
    {
        EdgeVertices e1 = new EdgeVertices(
            center + HexMetrics.GetFirstWaterCorner(dir),
            center + HexMetrics.GetSecondWaterCorner(dir)
            );
        
        water.AddTriangle(center, e1.v1, e1.v2);
        water.AddTriangle(center, e1.v2, e1.v3);
        water.AddTriangle(center, e1.v3, e1.v4);
        water.AddTriangle(center, e1.v4, e1.v5);

        Vector3 bridge = HexMetrics.GetWaterBridge(dir);
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);
        
        waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        waterShore.AddQuadUV(0f, 0f, 0f, 1f);
        

        HexCell nextNeighbor = cell.GetNeighbor(dir.Next());
        if (nextNeighbor != null)
        {
            waterShore.AddTriangle(e1.v5, e2.v5, e1.v5 + HexMetrics.GetWaterBridge(dir.Next()));
            waterShore.AddTriangleUV(
                new Vector2(0f, 0f), 
                new Vector2(0f, 1f), 
                new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f));
        }
    }
    
    void TriangulateWithoutRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        TriangulateEdgeFan(center, e, cell.TerrainTypeInd);

        if (cell.HasRoad)
        {
            Vector2 interpolators = GetRoadInterpolators(dir, cell);
            TriangulateRoad(
                center, 
                Vector3.Lerp(center, e.v1, interpolators.x), 
                Vector3.Lerp(center, e.v5, interpolators.y), 
                e, cell.HasRoadThroughEdge(dir));
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
        terrain.AddTriangle(m.v4, m.v5, centerR);
        
        terrain.AddQuad(centerL, center, m.v2, m.v3);
        terrain.AddQuad(center, centerR, m.v3, m.v4);

        
        terrain.AddTriangleColor(tex1Color);
        terrain.AddTriangleColor(tex1Color);
        terrain.AddQuadColor(tex1Color, tex1Color);
        terrain.AddQuadColor(tex1Color, tex1Color);

        Vector3 types;
        types.x = types.y = types.z = cell.TerrainTypeInd;
        terrain.AddTriangleTerrainTypes(types);
        terrain.AddTriangleTerrainTypes(types);
        terrain.AddQuadTerrainTypes(types);
        terrain.AddQuadTerrainTypes(types);

        TriangulateEdgeStrip(m, tex1Color, cell.TerrainTypeInd, e, tex1Color, cell.TerrainTypeInd);


        if (!cell.IsUnderwater)
        {
            bool reversed = cell.IncomingRiver == dir;

            TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
            TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
        }
    }

    void TriangulateWithRiverBeginOrEnd(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        EdgeVertices m =
            new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f));
        m.v3.y = e.v3.y;
        
        TriangulateEdgeStrip(m, tex1Color, cell.TerrainTypeInd, e, tex1Color, cell.TerrainTypeInd);
        TriangulateEdgeFan(center, m, cell.TerrainTypeInd);

        if (!cell.IsUnderwater)
        {
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
    }

    void TriangulateAdjacentToRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        if (cell.HasRoad)
        {
            TriangulateRoadAdjacentToRiver(dir, cell, center, e);
        }
        
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
        
        TriangulateEdgeStrip(m, tex1Color, cell.TerrainTypeInd, e, tex1Color, cell.TerrainTypeInd);
        TriangulateEdgeFan(center, m, cell.TerrainTypeInd);
        
        if(!cell.IsUnderwater && !cell.HasRoadThroughEdge(dir))
            features.AddFeature(cell,(center + e.v1 + e.v5) * (1f / 3f));
    }

    void TriangulateRoadAdjacentToRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
    {
        bool hasRoadThroughEdge = cell.HasRoadThroughEdge(dir);
        bool prevHasRiver = cell.HasRiverThroughEdge(dir.Previous());
        bool nextHasRiver = cell.HasRiverThroughEdge(dir.Next());

        
        Vector2 interpolators = GetRoadInterpolators(dir, cell);
        Vector3 roadCenter = center;

        if (cell.HasRiverBeginOrEnd)
        {
            roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
        }
        else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
        {
            Vector3 corner;
            if (prevHasRiver)
            {
                corner = HexMetrics.GetSecondSolidCorner(dir);
            }
            else
            {
                corner = HexMetrics.GetFirstSolidCorner(dir);
            }

            roadCenter += corner * 0.5f;
            center += corner * 0.25f;
        }
        else if (cell.IncomingRiver == cell.OutgoingRiver.Previous())
        {
            roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
        }
        else if (cell.IncomingRiver == cell.OutgoingRiver.Next())
        {
            roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;   
        }
        else if (prevHasRiver && nextHasRiver)
        {
            if(!hasRoadThroughEdge)
                return;
            
            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(dir) * HexMetrics.innerToOuter;
            roadCenter += offset * 0.7f;
            center += offset * 0.5f;
        }
        else
        {
            HexDirection middle;
            if (prevHasRiver)
            {
                middle = dir.Next();
            }
            else if (nextHasRiver)
            {
                middle = dir.Previous();
            }
            else
            {
                middle = dir;
            }

            if (   !cell.HasRoadThroughEdge(middle)
                && !cell.HasRoadThroughEdge(middle.Previous())
                && !cell.HasRoadThroughEdge(middle.Next()))
            {
                return;
            }

            roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
        }
        
        Vector3 mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
        Vector3 mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
        
        TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge);

        if (prevHasRiver)
        {
            if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(dir.Next()))
            {
                return;
            }
            TriangulateRoadEdge(roadCenter, center, mL);
        }

        if (nextHasRiver)
        {
            if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(dir.Previous()))
            {
                return;
            }
            TriangulateRoadEdge(roadCenter, mR, center);
        }
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
            if (!cell.IsUnderwater)
            {
                if (!neighbor.IsUnderwater)
                {
                    TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4,
                        cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                        cell.HasIncomingRiver && cell.IncomingRiver == dir);
                }
                else
                {
                    TriangulateWaterfallInWater(e1.v2, e1.v4, e2.v2, e2.v4,
                        cell.RiverSurfaceY, neighbor.RiverSurfaceY, neighbor.WaterSurfaceY);
                }
            }
            else if (!neighbor.IsUnderwater && neighbor.Elevation > cell.WaterLevel)
            {
                TriangulateWaterfallInWater(e2.v4, e2.v2, e1.v4, e1.v2,
                    neighbor.RiverSurfaceY, cell.RiverSurfaceY, cell.WaterSurfaceY);
            }
        }
        
        if (cell.GetEdgeType(dir) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor, cell.HasRoadThroughEdge(dir));
        }
        else
        {
            TriangulateEdgeStrip(
                e1, tex1Color, cell.TerrainTypeInd, 
                e2, tex2Color, neighbor.TerrainTypeInd,
                cell.HasRoadThroughEdge(dir));
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

    void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad = false)
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

    void TriangulateCornerTerraces(Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
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
        terrain.AddTriangleColors(tex1Color, c3, c4);
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

    void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell,
                                        Vector3 left,  HexCell leftCell,
                                        Vector3 right, HexCell rightCell)
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
    void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell,
                                        Vector3 left,  HexCell leftCell,
                                        Vector3 right, HexCell rightCell)
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

    private void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughEdge = false)
    {
        if (hasRoadThroughEdge)
        {
            Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
            TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
        
            roads.AddTriangle(center, mL, mC);
            roads.AddTriangle(center, mC, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
        }
        else
        {
            TriangulateRoadEdge(center, mL, mR);
        }
    }

    void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR)
    {
        roads.AddTriangle(center, mL, mR);
        roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
    }
    void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, 
                             float y, float v, bool isReversed)
    {
        TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, isReversed);
    }
    
    void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
    {
        roads.AddQuad(v1, v2, v4, v5);
        roads.AddQuad(v2, v3, v5, v6);
        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);

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
        
        if (hasRoad)
        {
            TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
        }
    }
    
    public Vector2 GetRoadInterpolators(HexDirection dir, HexCell cell)
    {
        Vector2 interpolators = new Vector2(1f, 1f);

        if (cell.HasRoadThroughEdge(dir))
        {
            interpolators.x = interpolators.y = 0.5f;
        }
        else
        {
            interpolators.x = cell.HasRoadThroughEdge(dir.Previous()) ? 0.5f : 0.25f;
            interpolators.y = cell.HasRoadThroughEdge(dir.Next()) ? 0.5f : 0.25f;
        }
        
        return interpolators;
    }

    void TriangulateWaterfallInWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY)
    {
        v1.y = v2.y = y1;
        v3.y = v4.y = y2;
        
        v1 = HexMetrics.Perturb(v1);
        v2 = HexMetrics.Perturb(v2);
        v3 = HexMetrics.Perturb(v3);
        v4 = HexMetrics.Perturb(v4);

        float t = (waterY - y2) / (y1 - y2);
        v3 = Vector3.Lerp(v3, v1, t);
        v4 = Vector3.Lerp(v4, v2, t);
        
        rivers.AddQuadUnperturbed(v1, v2, v3, v4);
        rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
    }
}