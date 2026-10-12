using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    [Header("Rotasi Diri")]
    public float selfRotationSpeed = 10f;

    [Header("Revolusi Mengelilingi Matahari")]
    public Transform centerObject; // Biasanya matahari atau objek parent
    public float revolutionSpeed = 5f;

    private bool isMovementPaused = false; // Tambahkan variabel ini

    void Update()
    {
        // Rotasi pada sumbu sendiri
        transform.Rotate(Vector3.up * selfRotationSpeed * Time.deltaTime);

        // Hanya lakukan revolusi jika tidak sedang dipause
        if (!isMovementPaused && centerObject != null)
        {
            transform.RotateAround(centerObject.position, Vector3.up, revolutionSpeed * Time.deltaTime);
        }
    }

    // Metode publik untuk mengontrol pause
    public void PauseMovement(bool pause)
    {
        isMovementPaused = pause;
        if (!isMovementPaused) // Dipanggil tepat setelah PauseMovement(false) dari ReturnToOrbit
        {
            // Idealnya, kita ingin mendapatkan orbitRadius yang digunakan oleh ReturnToOrbit.
            // Untuk sementara, kita bisa log jarak saat ini saja.
            float expectedRadius = 0f;
            ReturnToOrbit returnToOrbitScript = GetComponent<ReturnToOrbit>();
            if (returnToOrbitScript != null)
            {
                expectedRadius = returnToOrbitScript.orbitRadius; // Ambil orbitRadius dari skrip ReturnToOrbit
            }

            Debug.Log($"[PlanetRotation] Resuming movement for {gameObject.name}.");
            Debug.Log($"[PlanetRotation] Current distance to center ({centerObject.name}): {Vector3.Distance(transform.position, centerObject.position)}");
            Debug.Log($"[PlanetRotation] Expected radius from ReturnToOrbit script: {expectedRadius}");
            Debug.Log($"[PlanetRotation] Planet position at resume: {transform.position}");
        }
    }

    // Metode publik untuk mendapatkan kecepatan revolusi (jika diperlukan di skrip lain)
    public float GetRevolutionSpeed()
    {
        return revolutionSpeed;
    }

    // Metode publik untuk mendapatkan objek pusat (jika diperlukan di skrip lain)
    public Transform GetCenterObject()
    {
        return centerObject;
    }
}