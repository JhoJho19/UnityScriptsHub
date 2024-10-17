using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using System.Text;

public class QRCodeGenerator : MonoBehaviour
{
    public int qrCodeWidth = 256;
    public int qrCodeHeight = 256;
    public Image qrCodeImage;

    void Start()
    {
        GenerateRandomQRCode();
    }

    public void GenerateRandomQRCode()
    {
        string randomContent = GenerateRandomString(10);
        Debug.Log("Generated QR code content: " + randomContent);

        Texture2D qrCodeTexture = GenerateQRCode(randomContent, qrCodeWidth, qrCodeHeight);

        if (qrCodeTexture != null)
        {
            Sprite qrCodeSprite = ConvertTextureToSprite(qrCodeTexture);
            qrCodeImage.sprite = qrCodeSprite;
        }
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder stringBuilder = new StringBuilder(length);
        System.Random random = new System.Random();
        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }
        return stringBuilder.ToString();
    }

    private Texture2D GenerateQRCode(string text, int width, int height)
    {
        var qrCodeWriter = new QRCodeWriter();
        var color32 = EncodeToQR(text, qrCodeWriter, width, height);
        var qrCodeTexture = new Texture2D(width, height);
        qrCodeTexture.SetPixels32(color32);
        qrCodeTexture.Apply();
        return qrCodeTexture;
    }

    private Color32[] EncodeToQR(string text, QRCodeWriter qrCodeWriter, int width, int height)
    {
        var bitMatrix = qrCodeWriter.encode(text, BarcodeFormat.QR_CODE, width, height);
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBlack = bitMatrix[x, y];
                pixels[y * width + x] = isBlack ? Color.black : Color.white;
            }
        }
        return pixels;
    }

    private Sprite ConvertTextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
