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
                Debug.LogError($"Error al cargar imagen desde {url}: {request.error}");
            }
        }
    }
}
