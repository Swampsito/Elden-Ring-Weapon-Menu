using System.Threading.Tasks;

public interface IWeaponService
{
    Task<Weapon[]> GetWeaponsAsync();
}
