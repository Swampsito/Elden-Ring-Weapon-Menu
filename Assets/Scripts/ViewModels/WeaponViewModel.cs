using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponViewModel : MonoBehaviour
{
    [Header("Weapon Details UI")]
    [SerializeField] private TextMeshProUGUI weaponNameTitleText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private TextMeshProUGUI weaponCategoryText;
    [SerializeField] private TextMeshProUGUI weaponWeightText;
    [SerializeField] private TextMeshProUGUI weaponAttackText;
    [SerializeField] private TextMeshProUGUI weaponDefenceText;
    [SerializeField] private TextMeshProUGUI weaponAttributesText;
    [SerializeField] private TextMeshProUGUI weaponScaleText;
    [SerializeField] private RawImage weaponImage;
    
    [Header("Weapon Thumbnails Grid")]
    [SerializeField] private Transform weaponGridContainer;
    [SerializeField] private RawImage weaponThumbnailPrefab;

    private IWeaponService weaponService;
    private List<Weapon> weapons = new List<Weapon>();

    private async void Start()
    {
        // Dependency injection via interface for easier testing and future changes
        weaponService = new WeaponService();
        await LoadWeaponsAsync();
    }

    /// <summary>
    /// Loads the weapon list from the API and updates the UI.
    /// </summary>
    private async Task LoadWeaponsAsync()
    {
        Weapon[] fetchedWeapons = await weaponService.GetWeaponsAsync();
        if (fetchedWeapons != null && fetchedWeapons.Length > 0)
        {
            weapons = new List<Weapon>(fetchedWeapons);
            GenerateWeaponThumbnails();
            DisplayWeaponDetails(weapons[0]);
        }
        else
        {
            Debug.LogError("No weapon data received from API.");
        }
    }

    /// <summary>
    /// Generates thumbnails for each weapon and assigns click events.
    /// </summary>
    private void GenerateWeaponThumbnails()
    {
        foreach (Weapon weapon in weapons)
        {
            RawImage thumbnail = Instantiate(weaponThumbnailPrefab, weaponGridContainer);
            StartCoroutine(ImageLoader.LoadImageCoroutine(weapon.image, thumbnail));

            Button button = thumbnail.GetComponent<Button>() ?? thumbnail.gameObject.AddComponent<Button>();
            Weapon capturedWeapon = weapon;
            button.onClick.AddListener(() => DisplayWeaponDetails(capturedWeapon));
        }
    }

    /// <summary>
    /// Updates the UI with the details of the selected weapon.
    /// </summary>
    /// <param name="weapon">The weapon whose details will be displayed.</param>
    private void DisplayWeaponDetails(Weapon weapon)
    {
        weaponNameTitleText.text = weapon.name;
        weaponNameText.text = weapon.name;
        weaponDescriptionText.text = weapon.description;
        weaponCategoryText.text = weapon.category;
        weaponWeightText.text = weapon.weight.ToString();
        weaponAttackText.text = FormatWeaponStats(weapon.attack);
        weaponDefenceText.text = FormatWeaponStats(weapon.defence);
        weaponAttributesText.text = FormatWeaponStats(weapon.requiredAttributes);
        weaponScaleText.text = FormatWeaponScales(weapon.scalesWith);

        StartCoroutine(ImageLoader.LoadImageCoroutine(weapon.image, weaponImage));
    }

    /// <summary>
    /// Formats the weapon stats (WeaponStat[]) for display.
    /// </summary>
    /// <param name="stats">Array of WeaponStat.</param>
    /// <returns>Formatted stats string.</returns>
    private string FormatWeaponStats(WeaponStat[] stats)
    {
        if (stats == null || stats.Length == 0)
            return "N/A";

        StringBuilder sb = new StringBuilder();
        foreach (WeaponStat stat in stats)
        {
            // Skip the RNG stat (case-insensitive)
            if (stat.name.ToLower() == "rng")
                continue;
            sb.AppendLine($"{stat.name}: {stat.amount}");
        }
        return sb.ToString().Trim();
    }


    /// <summary>
    /// Formats the weapon scaling info (WeaponScale[]) for display.
    /// </summary>
    /// <param name="scales">Array of WeaponScale.</param>
    /// <returns>Formatted scaling string.</returns>
    private string FormatWeaponScales(WeaponScale[] scales)
    {
        if (scales == null || scales.Length == 0)
            return "N/A";

        StringBuilder sb = new StringBuilder();
        foreach (WeaponScale scale in scales)
        {
            sb.AppendLine($"{scale.name}: {scale.scaling}");
        }
        return sb.ToString().Trim();
    }
}
