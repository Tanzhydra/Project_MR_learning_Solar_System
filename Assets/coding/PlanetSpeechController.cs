using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using UnityEngine;

// Definisi Kelas PlanetSpeechInfo
[System.Serializable]
public class PlanetSpeechInfo
{
    [Tooltip("Nama planet untuk referensi internal/debug di Inspector.")]
    public string planetEditorName;

    [Tooltip("GameObject aktual dari planet yang akan dikontrol.")]
    public GameObject planetGameObject;

    [Tooltip("Daftar kata kunci suara untuk planet ini (misalnya, 'Bumi', 'Earth'). Perbandingan akan case-insensitive.")]
    public List<string> speechKeywords;

    [Tooltip("GameObject yang menampilkan ToolTip/label nama untuk planet ini.")]
    public GameObject toolTipGameObject;

    [Tooltip("Komponen LineRenderer yang menampilkan jalur orbit untuk planet ini.")]
    public LineRenderer orbitLineRenderer;
}

public class PlanetSpeechController : MonoBehaviour, IMixedRealitySpeechHandler
{
    [Tooltip("Konfigurasi daftar planet yang bisa dikontrol dengan perintah suara.")]
    public List<PlanetSpeechInfo> planetsToControl;

    [Tooltip("Drag komponen Renderer (MeshRenderer atau SkinnedMeshRenderer) dari GameObject Matahari ke sini.")]
    public Renderer sunRenderer; // Variabel untuk Renderer Matahari

    // Daftar kata kunci untuk perintah global "tampilkan semua"
    private List<string> showAllKeywords = new List<string> { "tampilkan semua", "show all", "reset tata surya", "show all planets", "reset" };

    private Dictionary<string, PlanetSpeechInfo> keywordToPlanetInfoMap;

    void Start()
    {
        Debug.Log("[PlanetSpeechController] Start()_called.");
        keywordToPlanetInfoMap = new Dictionary<string, PlanetSpeechInfo>(System.StringComparer.OrdinalIgnoreCase);

        if (planetsToControl == null || planetsToControl.Count == 0)
        {
            Debug.LogError("[PlanetSpeechController] Daftar 'Planets To Control' kosong atau belum di-assign di Inspector! Tidak ada planet yang bisa dikontrol suara.");
            // return; // Sebaiknya tidak return agar OnEnable/OnDisable tetap bisa berjalan untuk unregister jika perlu
        }

        if (sunRenderer == null)
        {
            Debug.LogWarning("[PlanetSpeechController] Sun Renderer belum di-assign di Inspector! Fitur menghilangkan/memunculkan Matahari tidak akan berfungsi.");
        }

        if (planetsToControl != null) // Hanya proses jika list tidak null
        {
            Debug.Log($"[PlanetSpeechController] Memproses {planetsToControl.Count} entri planet dari Inspector.");
            foreach (var planetInfo in planetsToControl)
            {
                if (planetInfo.planetGameObject == null)
                {
                    Debug.LogError($"[PlanetSpeechController] Planet GameObject belum di-assign untuk entri dengan nama editor '{planetInfo.planetEditorName}'. Entri ini akan dilewati.");
                    continue;
                }
                // Peringatan jika komponen opsional tidak di-assign
                if (planetInfo.toolTipGameObject == null) { Debug.LogWarning($"[PlanetSpeechController] ToolTip GameObject tidak di-assign untuk '{planetInfo.planetEditorName}' (GameObject: {planetInfo.planetGameObject.name})."); }
                if (planetInfo.orbitLineRenderer == null) { Debug.LogWarning($"[PlanetSpeechController] Orbit LineRenderer tidak di-assign untuk '{planetInfo.planetEditorName}' (GameObject: {planetInfo.planetGameObject.name})."); }

                if (planetInfo.speechKeywords == null || planetInfo.speechKeywords.Count == 0)
                {
                    Debug.LogWarning($"[PlanetSpeechController] Tidak ada speech keyword yang di-assign untuk '{planetInfo.planetEditorName}' (GameObject: {planetInfo.planetGameObject.name}). Planet ini tidak akan bisa dikontrol suara.");
                    continue;
                }

                foreach (var keyword in planetInfo.speechKeywords)
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        Debug.LogWarning($"[PlanetSpeechController] Ditemukan keyword kosong atau hanya spasi untuk planet '{planetInfo.planetEditorName}'. Keyword ini akan dilewati.");
                        continue;
                    }
                    if (keywordToPlanetInfoMap.ContainsKey(keyword))
                    {
                        Debug.LogWarning($"[PlanetSpeechController] Keyword duplikat (case-insensitive) '{keyword}' terdeteksi. Akan di-map ke planet '{planetInfo.planetEditorName}'. Mapping sebelumnya adalah untuk planet '{keywordToPlanetInfoMap[keyword].planetEditorName}'.");
                    }
                    keywordToPlanetInfoMap[keyword] = planetInfo;
                }
            }
            Debug.Log($"[PlanetSpeechController] Inisialisasi selesai. Total {keywordToPlanetInfoMap.Count} keyword unik (planet) terdaftar untuk mapping.");
        }
    }

    void OnEnable()
    {
        if (CoreServices.InputSystem != null)
        {
            CoreServices.InputSystem.RegisterHandler<IMixedRealitySpeechHandler>(this);
            Debug.Log("[PlanetSpeechController] Berhasil mendaftar sebagai speech handler ke MRTK InputSystem.");
        }
        else
        {
            Debug.LogError("[PlanetSpeechController] CoreServices.InputSystem belum siap atau tidak ditemukan saat OnEnable. Tidak bisa mendaftar speech handler.");
        }
    }

    void OnDisable()
    {
        if (CoreServices.InputSystem != null)
        {
            CoreServices.InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
            Debug.Log("[PlanetSpeechController] Berhasil membatalkan pendaftaran sebagai speech handler dari MRTK InputSystem.");
        }
    }

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        string recognizedKeyword = eventData.Command.Keyword;
        Debug.Log($"[PlanetSpeechController] Perintah suara dikenali oleh MRTK: '{recognizedKeyword}'");

        bool isShowAllCommand = false;
        foreach (string showAllKey in showAllKeywords)
        {
            if (string.Equals(recognizedKeyword, showAllKey, System.StringComparison.OrdinalIgnoreCase))
            {
                isShowAllCommand = true;
                break;
            }
        }

        if (isShowAllCommand)
        {
            Debug.Log("[PlanetSpeechController] Perintah 'Tampilkan Semua' diterima.");
            HandleShowAllPlanets();
        }
        else if (keywordToPlanetInfoMap.TryGetValue(recognizedKeyword, out PlanetSpeechInfo selectedPlanetInfo))
        {
            Debug.Log($"[PlanetSpeechController] Planet '{selectedPlanetInfo.planetEditorName}' dipilih. Fokus pada planet ini.");
            HandleFocusOnPlanet(selectedPlanetInfo);
        }
        else
        {
            Debug.LogWarning($"[PlanetSpeechController] Keyword '{recognizedKeyword}' tidak dikenali sebagai perintah planet atau perintah global. Pastikan keyword terdaftar di MRTK Speech Profile DAN di Inspector untuk PlanetSpeechController.");
        }
    }

    private void HandleFocusOnPlanet(PlanetSpeechInfo selectedPlanetInfo)
    {
        Debug.Log($"[PlanetSpeechController] Fokus pada: {(selectedPlanetInfo.planetGameObject != null ? selectedPlanetInfo.planetGameObject.name : selectedPlanetInfo.planetEditorName)}");

        // Nonaktifkan visual Matahari (Renderer-nya)
        if (sunRenderer != null)
        {
            sunRenderer.enabled = false;
            Debug.Log($"[PlanetSpeechController] > Sun Renderer enabled: false");
        }
        else
        {
            Debug.LogWarning("[PlanetSpeechController] Sun Renderer tidak di-assign, tidak bisa menonaktifkan visual Matahari.");
        }

        if (planetsToControl == null) return;
        foreach (var pInfo in planetsToControl)
        {
            bool isThisTheSelectedPlanet = System.Object.ReferenceEquals(pInfo, selectedPlanetInfo);

            if (pInfo.planetGameObject != null)
            {
                pInfo.planetGameObject.SetActive(isThisTheSelectedPlanet);
            }
            if (pInfo.toolTipGameObject != null)
            {
                pInfo.toolTipGameObject.SetActive(isThisTheSelectedPlanet);
            }
            if (pInfo.orbitLineRenderer != null)
            {
                pInfo.orbitLineRenderer.enabled = false; // Semua LineRenderer orbit path dinonaktifkan
            }
        }
        Debug.Log($"[PlanetSpeechController] Pengaturan visibilitas untuk FOKUS selesai.");
    }

    private void HandleShowAllPlanets()
    {
        Debug.Log("[PlanetSpeechController] Menampilkan semua planet & orbit, menyembunyikan semua tooltip, menampilkan Matahari.");

        // Aktifkan kembali visual Matahari (Renderer-nya)
        if (sunRenderer != null)
        {
            sunRenderer.enabled = true;
            Debug.Log($"[PlanetSpeechController] > Sun Renderer enabled: true");
        }
        else
        {
            Debug.LogWarning("[PlanetSpeechController] Sun Renderer tidak di-assign, tidak bisa mengaktifkan visual Matahari.");
        }

        if (planetsToControl == null) return;
        foreach (var pInfo in planetsToControl)
        {
            if (pInfo.planetGameObject != null)
            {
                pInfo.planetGameObject.SetActive(true); // Tampilkan semua planet
            }
            if (pInfo.toolTipGameObject != null)
            {
                pInfo.toolTipGameObject.SetActive(false); // Sembunyikan semua tooltip (default state)
            }
            if (pInfo.orbitLineRenderer != null)
            {
                pInfo.orbitLineRenderer.enabled = true; // Tampilkan semua jalur orbit
            }
        }
        Debug.Log($"[PlanetSpeechController] Pengaturan visibilitas untuk TAMPILKAN SEMUA selesai.");
    }
}