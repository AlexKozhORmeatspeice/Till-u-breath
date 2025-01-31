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

    public override void Use(IAgent agent)
    {
        agent.ChangeHP(-damage);
    }

    public override void Use(HexCell cell)
    {
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

