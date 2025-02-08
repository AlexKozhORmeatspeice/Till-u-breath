class Weapon : Item
{
    public int rangeAttack;
    public int damage;

    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();

        rangeAttack = 5;
        damage = 10;
    }

    public override bool Use(IAgent agent)
    {
        agent.ChangeHP(-damage);
        return true;
    }

    public override bool Use(HexCell cell)
    {
        return true;
        //maybe in a future there would be some logic about destroying objects
    }
}

class Shotgun : Weapon
{
    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();

        rangeAttack = 4;
        damage = 20;
    }
}

