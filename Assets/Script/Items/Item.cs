using Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class Item : MonoBehaviour, IPooledObj
{
    private IAgent owner;

    private HexCell onCell;
    public HexCell OnCell => onCell;

    public abstract void Use(IAgent agent);
    public abstract void Use(HexCell cell);

    public virtual void OnObjectSpawn()
    {
        
    }

    public void Take(IAgent agent)
    {
        Helper.ChangeRendererState(gameObject, false);
        
        owner = agent;
        onCell = null;
    }

    public void Drop(HexCell _onCell)
    {
        Helper.ChangeRendererState(gameObject, true);

        owner = null;
        ChangeLocation(_onCell);
    }

    private void ChangeLocation(HexCell cell)
    {
        HexCell cellNow = onCell;
        if (cellNow != null)
            cellNow.Item = null;

        onCell = cell;

        transform.localPosition = cell.Position;
        
        Vector3 pos = transform.localPosition;
        pos.z += Random.Range(-HexMetrics.innerRadius, HexMetrics.innerRadius);
        pos.x += Random.Range(-HexMetrics.innerRadius, HexMetrics.innerRadius);
        transform.localPosition = pos;

        cell.Item = this;
    }
}
