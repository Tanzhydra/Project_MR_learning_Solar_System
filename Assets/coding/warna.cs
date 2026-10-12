using UnityEngine;

public class Warna : MonoBehaviour
{
    private Renderer cubeRenderer;  // Komponen Renderer untuk ubah warna

    [Header("Pengaturan Warna")]
    public float interval = 3f;       // Interval waktu antar perubahan warna
    public bool randomizeColor = true; // Opsi untuk warna acak

    [Header("Warna Tetap (Non-Random)")]
    public Color color1 = Color.red;
    public Color color2 = Color.green;
    public Color color3 = Color.blue;

    private float timer = 0f;  // Timer untuk menghitung waktu
    private Color[] fixedColors;
    private int currentColorIndex = 0;

    void Start()
    {
        // Ambil komponen Renderer dari objek
        cubeRenderer = GetComponent<Renderer>();

        // Buat salinan material agar tidak ubah material global
        cubeRenderer.material = new Material(cubeRenderer.material);

        // Siapkan array warna tetap
        fixedColors = new Color[] { color1, color2, color3 };

        // Set warna awal
        cubeRenderer.material.color = randomizeColor ? GetRandomColor() : fixedColors[currentColorIndex];
    }

    void Update()
    {
        timer += Time.deltaTime;  // Tambah timer setiap frame

        if (timer >= interval)    // Jika sudah lewat waktu interval
        {
            timer = 0f;  // Reset timer

            // Tentukan warna baru
            if (randomizeColor)
            {
                // Warna acak
                cubeRenderer.material.color = GetRandomColor();
            }
            else
            {
                // Ganti ke warna berikutnya dari 3 warna tetap
                currentColorIndex = (currentColorIndex + 1) % fixedColors.Length;
                cubeRenderer.material.color = fixedColors[currentColorIndex];
            }
        }
    }

    // Fungsi untuk mendapatkan warna acak
    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }
}
