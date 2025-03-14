# 📚 Documentación - Sistema de Armas en Unity (Estilo Elden Ring)

## 📌 Introducción

Este documento describe la implementación de un sistema para obtener y mostrar información de armas en **Unity** utilizando la API de Elden Ring.  
El sistema sigue los principios **SOLID** y **Clean Code** y se estructura en las siguientes capas:

1. **Modelo de datos** (WeaponModel)
2. **Interfaz de servicio** (IWeaponService)
3. **Servicio de API** (WeaponService)
4. **ViewModel** (WeaponViewModel)
5. **Utilidad** (ImageLoader)

## 🛠 Requisitos Previos

- Unity (2021 o superior)
- Paquete **TextMeshPro** instalado
- Conexión a internet para obtener los datos desde la API

---

## 📜 1. WeaponModel.cs (Modelo de Datos)

Define la estructura de los datos de un arma. Asegúrate de que los nombres de las propiedades coincidan con los campos del JSON que retorna la API.

```csharp
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
public class WeaponResponse
{
    public Weapon[] data;
}
```

---

## 🌍 2. IWeaponService.cs (Interfaz del Servicio de Datos)

Define el contrato para obtener los datos de armas desde la API.

```csharp
using System.Threading.Tasks;

public interface IWeaponService
{
    Task<Weapon[]> GetWeaponsAsync();
}
```

---

## 🔄 3. WeaponService.cs (Servicio de API)

Implementa `IWeaponService` para realizar la solicitud HTTP a la API de Elden Ring utilizando HttpClient y JsonUtility.

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponService : IWeaponService
{
    private readonly string apiUrl = "https://eldenring.fanapis.com/api/weapons";
    private readonly HttpClient httpClient = new HttpClient();

    public async Task<Weapon[]> GetWeaponsAsync()
    {
        try
        {
            string json = await httpClient.GetStringAsync(apiUrl);
            WeaponResponse response = JsonUtility.FromJson<WeaponResponse>(json);
            return response?.data ?? Array.Empty<Weapon>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error fetching weapons: {ex.Message}");
            return Array.Empty<Weapon>();
        }
    }
}
```

---

## 🎮 4. WeaponViewModel.cs (ViewModel)

El ViewModel se encarga de interactuar con el servicio para obtener datos y actualizar la UI, incluyendo la generación de miniaturas y la presentación de detalles.

```csharp
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

    private string FormatWeaponStats(WeaponStat[] stats)
    {
        if (stats == null || stats.Length == 0)
            return "N/A";

        StringBuilder sb = new StringBuilder();
        foreach (WeaponStat stat in stats)
        {
            // Skip "RNG" stat if not needed
            if (stat.name.ToLower() == "rng")
                continue;
            sb.AppendLine($"{stat.name}: {stat.amount}");
        }
        return sb.ToString().Trim();
    }

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
```

---

## 🔧 5. ImageLoader.cs (Utility)

Clase de utilidad para cargar imágenes mediante UnityWebRequest. Coloca este archivo en la carpeta `Utilities`.

```csharp
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public static class ImageLoader
{
    public static IEnumerator LoadImageCoroutine(string url, RawImage targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                targetImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            else
            {
                Debug.LogError($"Error loading image from {url}: {request.error}");
            }
        }
    }
}
```

---

## 🔧 6. Configuración en Unity

1. **Crear los Scripts** en `Assets/Scripts/` con la siguiente estructura de carpetas:
   - **Models**: Contiene `WeaponModel.cs` (los modelos de datos).
   - **Services**: Contiene `IWeaponService.cs` y `WeaponService.cs`.
   - **ViewModels**: Contiene `WeaponViewModel.cs`.
   - **Utilities**: Contiene `ImageLoader.cs`.
2. Añade un Canvas en la escena (`GameObject → UI → Canvas`).
3. Crea los elementos de UI:
   - Usa `Text - TextMeshPro` para mostrar los detalles del arma.
   - Usa `RawImage` para mostrar la imagen del arma.
4. Crea un GameObject vacío (por ejemplo, `WeaponManager`) y asigna el script `WeaponViewModel`.
5. Arrastra los elementos de UI correspondientes al Inspector del `WeaponViewModel`.
6. Ejecuta la escena para visualizar los datos de la API.

---
