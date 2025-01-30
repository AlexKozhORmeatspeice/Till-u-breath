class Weapon
{
    public int rangeAttack;
    public int damage;

    public Weapon()
    {
        rangeAttack = 5;
        damage = 10;
    }

    public void Attack(IAgent agent)
    {

    }
}

class Shotgun : Weapon
{
    public Shotgun()
    {
        rangeAttack = 4;
        damage = 20;
    }
}

