using UnityEngine;

public interface IAgent
{
    public void UpdateStatePast();
    public void UpdateStateFuture();

    public void ChangeLocation(HexCell cell);

    public void ValidateLocation();

    public GameObject GetGameObject();

    public void Die();
}