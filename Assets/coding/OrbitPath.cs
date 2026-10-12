using UnityEngine;

public class OrbitPath : MonoBehaviour // Pastikan kelas mewarisi dari MonoBehaviour
{
    // Deklarasi variabel diletakkan di sini, di dalam kelas
    [Header("Pengaturan Orbit")]
    public Transform centerObject;
    public int segments = 100;
    public float orbitRadius = 10f;
    public Color orbitColor = Color.white;
    public float lineWidth = 0.05f;

    private LineRenderer lineRenderer; // Variabel 'lineRenderer' dideklarasikan di sini

    // Metode Start() diletakkan di sini, di dalam kelas
    void Start()
    {
        // Coba dapatkan komponen LineRenderer yang sudah ada terlebih dahulu
        lineRenderer = GetComponent<LineRenderer>(); // 'GetComponent' adalah metode dari MonoBehaviour

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>(); // 'gameObject' adalah properti dari MonoBehaviour
            Debug.LogWarning($"LineRenderer tidak ditemukan di {gameObject.name}, jadi satu ditambahkan oleh OrbitPath.cs. Disarankan untuk menambahkannya manual di Inspector.");
        }

        lineRenderer.positionCount = segments + 1; // 'segments' adalah variabel yang sudah dideklarasikan di atas
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth; // 'lineWidth' adalah variabel yang sudah dideklarasikan di atas
        lineRenderer.endWidth = lineWidth;

        if (lineRenderer.sharedMaterial == null)
        {
            // Anda bisa menggunakan shader yang lebih sederhana jika partikel tidak ditemukan
            Material defaultLineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            if (defaultLineMaterial == null || !defaultLineMaterial.shader.isSupported)
            { // Cek jika shader ada dan didukung
                defaultLineMaterial = new Material(Shader.Find("Unlit/Color"));
                defaultLineMaterial.color = orbitColor; // Set warna jika menggunakan Unlit/Color
                Debug.LogWarning("Shader Legacy Shaders/Particles/Alpha Blended Premultiply tidak ditemukan/didukung, menggunakan Unlit/Color sebagai fallback.");
            }
            lineRenderer.material = defaultLineMaterial;
        }

        lineRenderer.startColor = orbitColor; // 'orbitColor' adalah variabel yang sudah dideklarasikan di atas
        lineRenderer.endColor = orbitColor;

        DrawOrbit(); // Memanggil metode DrawOrbit()
    }

    // Metode DrawOrbit() diletakkan di sini, di dalam kelas
    void DrawOrbit()
    {
        float angleStep = 360f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * orbitRadius; // 'orbitRadius' adalah variabel yang sudah dideklarasikan
            float z = Mathf.Sin(angle) * orbitRadius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z)); // 'lineRenderer' adalah variabel yang sudah dideklarasikan
        }
    }
} // Kurung kurawal penutup untuk kelas OrbitPath