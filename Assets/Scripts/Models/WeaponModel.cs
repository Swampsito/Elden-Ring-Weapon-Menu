[System.Serializable]
public class WeaponStat
{
    public string name;
    public float amount;  
}

[System.Serializable]
public class WeaponScale
{
    public string name;
    public string scaling;  
}

[System.Serializable]
public class Weapon
{
    public string id;
    public string name;
    public string image;
    public string description;
    public string category;
    public float weight;  
    public WeaponStat[] attack;
    public WeaponStat[] defence;
    public WeaponStat[] requiredAttributes;
    
    public WeaponScale[] scalesWith;
}

[System.Serializable]
public class WeaponResponse
{
    public Weapon[] data;
}
