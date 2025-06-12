using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System.Collections;
using Firebase.Auth;

[System.Serializable]
public class QRData
{
    public string uid;
}

public class QRCodeScanner : MonoBehaviour
{
    public RawImage rawImage;
    public FirebaseAuthen firebaseAuthen;

    private WebCamTexture camTexture;
    private bool isScanning = false;



    void Start()
    {
        StartCamera();
    }

    void StartCamera()
    {
        camTexture = new WebCamTexture();
        rawImage.texture = camTexture;
        rawImage.material.mainTexture = camTexture;
        camTexture.Play();
        isScanning = true;

        StartCoroutine(ScanQRCode());
    }

    public void StopCamera()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            camTexture.Stop();
            Debug.Log("Caméra arrêtée.");
        }
    }

    IEnumerator ScanQRCode()
    {
        IBarcodeReader reader = new BarcodeReader();

        while (isScanning)
        {
            try
            {
                var result = reader.Decode(camTexture.GetPixels32(),
                                           camTexture.width, camTexture.height);

                if (result != null)
                {
                    Debug.Log("QR Code détecté : " + result.Text);
                    isScanning = false;

                    try
                    {
                        QRData data = JsonUtility.FromJson<QRData>(result.Text);

                        if (data != null && !string.IsNullOrEmpty(data.uid))
                        {
                            Debug.Log("UID extrait du QR : " + data.uid);

                            // Utilise la référence publique
                            firebaseAuthen.RechercherUtilisateur(data.uid);
                            UserSession.userId = data.uid;
                        }
                        else
                        {
                            Debug.LogError("QR invalide ou UID manquant.");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Erreur lors du parsing JSON : " + e.Message);
                    }
                }
            }
            catch { }
            yield return new WaitForSeconds(0.5f);
        }
    }

}
