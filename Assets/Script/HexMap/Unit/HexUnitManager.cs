using Script;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class HexUnitManager : MonoBehaviour
{
    private static HexGrid grid;
    private static Dictionary<int, IAgent> uniqueUnits;
    private void Awake()
    {
        if(uniqueUnits != null)
        {
            uniqueUnits.Clear();
        }
        else
        {
            uniqueUnits = new Dictionary<int, IAgent>();
        }
    }

    public static IAgent CreateUnit(HexCell cell, int unitInd)
    {
        HexUnit unit = HexMetrics.unitCollection[unitInd];

        if (uniqueUnits.ContainsKey(unitInd))
        {
            uniqueUnits[unitInd].Die();
            uniqueUnits.Remove(unitInd);
        }

        if (cell != null && (cell.Unit == null))
        {
            Transform prefab = Instantiate(unit.prefab.transform);
            prefab.SetParent(grid.transform, false);

            IAgent agent = prefab.GetComponent<IAgent>();
            agent.ChangeLocation(cell);
            agent.SetGrid(grid);
            
            if (unit.isUnique)
            {
                uniqueUnits[unitInd] = agent;
            }
            return agent;
        }

        return null;
    }

    public static int GetAgentIndex(IAgent agent)
    {
        int ind = -1;
        GameObject prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(agent.GetGameObject());
        for (int i = 0; i < HexMetrics.unitCollection.Length; i++)
        {
            HexUnit collectionUnit = HexMetrics.unitCollection[i];

            if (agent.GetGameObject().name.Contains(collectionUnit.prefab.name))
            {
                ind = i;
                break;
            }
        }
        return ind;
    }


    public static void SetGrid(HexGrid _grid)
    {
        grid = _grid;
    }
}
