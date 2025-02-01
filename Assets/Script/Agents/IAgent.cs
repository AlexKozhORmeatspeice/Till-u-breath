using System.IO;
using UnityEngine;

public interface IAgent
{
    public void UpdateStatePast();
    public void UpdateStateFuture();

    public void ChangeLocation(HexCell cell);
    public void ValidateLocation();

    public GameObject GetGameObject();
    public HexCell GetCell();

    public void ChangeHP(int points);
    public void ChangeEnergy(int points);
    public void ChangeInsanity(int points);

    public void Die();
    public void SetGrid(HexGrid grid);
    public abstract void SaveState(BinaryWriter writer, int time);
    public abstract void LoadState(BinaryReader reader, int time);
}