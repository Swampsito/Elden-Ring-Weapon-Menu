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
            Debug.Log("Llamando a la API...");
            string json = await httpClient.GetStringAsync(apiUrl);
            Debug.Log($"Respuesta JSON: {json}"); // Verifica la respuesta en la consola

            WeaponResponse response = JsonUtility.FromJson<WeaponResponse>(json);
            Debug.Log($"Armas obtenidas: {response.data.Length}");

            return response?.data ?? Array.Empty<Weapon>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener armas: {ex.Message}");
            return Array.Empty<Weapon>();
        }
    }
}
