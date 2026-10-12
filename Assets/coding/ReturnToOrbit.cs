using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI; // Namespace untuk ObjectManipulator

[RequireComponent(typeof(ObjectManipulator))]
[RequireComponent(typeof(PlanetRotation))]
public class ReturnToOrbit : MonoBehaviour
{
    [Header("Pengaturan Orbit Kembali")]
    public float orbitRadius; // Harus sama dengan orbitRadius di OrbitPath.cs
    // centerObject akan diambil dari PlanetRotation.cs

    private ObjectManipulator objectManipulator;
    private PlanetRotation planetRotationScript;
    private Transform centerObjectTransform; // Objek pusat orbit (Matahari)

    void Start()
    {
        objectManipulator = GetComponent<ObjectManipulator>();
        planetRotationScript = GetComponent<PlanetRotation>();

        if (planetRotationScript == null || planetRotationScript.GetCenterObject() == null)
        {
            Debug.LogError("PlanetRotation script atau CenterObject tidak ditemukan/diset pada planet ini!");
            enabled = false; // Matikan skrip ini jika setup tidak benar
            return;
        }

        centerObjectTransform = planetRotationScript.GetCenterObject();

        // Pastikan orbitRadius diisi di Inspector
        if (orbitRadius <= 0)
        {
            // Coba ambil dari OrbitPath jika ada di child object planet ini atau parent-nya
            OrbitPath path = GetComponentInChildren<OrbitPath>();
            if (path == null && transform.parent != null)
            {
                path = transform.parent.GetComponentInChildren<OrbitPath>(); // Cek jika OrbitPath adalah sibling dari planet dibawah satu parent (misal OrbitAnchor)
            }

            if (path != null)
            {
                orbitRadius = path.orbitRadius;
                Debug.LogWarning($"orbitRadius di ReturnToOrbit pada {gameObject.name} diambil dari OrbitPath: {orbitRadius}. Pastikan ini benar.");
            }
            else
            {
                Debug.LogError($"orbitRadius pada {gameObject.name} belum diatur dan tidak ditemukan OrbitPath. Planet mungkin tidak kembali ke orbit dengan benar.");
                // Anda bisa set default atau throw error lain
            }
        }


        // Dengarkan event dari ObjectManipulator
        if (objectManipulator != null)
        {
            objectManipulator.OnManipulationStarted.AddListener(HandleManipulationStarted);
            objectManipulator.OnManipulationEnded.AddListener(HandleManipulationEnded);
        }
        else
        {
            Debug.LogError("ObjectManipulator tidak ditemukan pada planet ini!");
            enabled = false;
        }
    }

    private void HandleManipulationStarted(ManipulationEventData eventData)
    {
        if (planetRotationScript != null)
        {
            planetRotationScript.PauseMovement(true); // Hentikan gerakan revolusi planet
        }
    }

    private void HandleManipulationEnded(ManipulationEventData eventData)
    {
        if (planetRotationScript != null && centerObjectTransform != null)
        {
            Debug.Log($"[ReturnToOrbit] HandleManipulationEnded for {gameObject.name}.");
            Debug.Log($"[ReturnToOrbit] Current orbitRadius value: {orbitRadius}"); // Cek nilai orbitRadius
            Debug.Log($"[ReturnToOrbit] Center Object ({centerObjectTransform.name}) position: {centerObjectTransform.position}");
            Debug.Log($"[ReturnToOrbit] Planet current position (dropped): {transform.position}");

            Vector3 directionFromCenter = transform.position - centerObjectTransform.position;
            directionFromCenter.y = 0;
            directionFromCenter.Normalize();

            Vector3 targetPositionOnOrbit = centerObjectTransform.position + directionFromCenter * orbitRadius;
            Debug.Log($"[ReturnToOrbit] Calculated targetPositionOnOrbit: {targetPositionOnOrbit}");
            Debug.Log($"[ReturnToOrbit] Distance of targetPositionOnOrbit from center: {Vector3.Distance(targetPositionOnOrbit, centerObjectTransform.position)} (Should be ~{orbitRadius})");

            transform.position = targetPositionOnOrbit;
            Debug.Log($"[ReturnToOrbit] Planet position AFTER snap: {transform.position}");
            Debug.Log($"[ReturnToOrbit] Distance from center AFTER snap: {Vector3.Distance(transform.position, centerObjectTransform.position)} (Should be ~{orbitRadius})");

            planetRotationScript.PauseMovement(false);
        }
        else
        {
            Debug.LogError($"[ReturnToOrbit] PlanetRotation script or CenterObjectTransform is null for {gameObject.name}!");
        }
    }

    void OnDestroy()
    {
        // Pastikan untuk melepaskan listener saat objek dihancurkan
        if (objectManipulator != null)
        {
            objectManipulator.OnManipulationStarted.RemoveListener(HandleManipulationStarted);
            objectManipulator.OnManipulationEnded.RemoveListener(HandleManipulationEnded);
        }
    }
}