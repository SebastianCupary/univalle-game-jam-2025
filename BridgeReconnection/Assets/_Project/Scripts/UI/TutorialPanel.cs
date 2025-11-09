using UnityEngine;
using UnityEngine.UI;

// Control de panel de tutorial con navegación básica.
// Mantén este GameObject (manager) siempre activo; se oculta solo el contenido.
public class TutorialPanel : MonoBehaviour
{
    [Header("Slides")] public Sprite[] slides; // imágenes del tutorial
    [Tooltip("Imagen donde se mostrará cada slide")] public Image slideImage;

    [Header("Panel Root")] [Tooltip("Objeto que se ocultará/mostrará. Si null, usa este GameObject")] public GameObject contentRoot;
    [SerializeField] private bool hideOnAwake = true;

    [Header("Buttons")] public Button prevButton; public Button nextButton; public Button closeButton; // dentro del panel
    [Header("Open Button (externo)")] public Button openButton; // botón fuera del panel que abre el tutorial

    [Header("Opciones")] public bool loopSlides = false; // si true, pasar del último al primero
    public bool hidePrevNextAtEnds = true; // oculta prev/next cuando está en extremos (si no hay loop)

    private int _index =0;
    private GameObject _root;

    private void Awake()
    {
        _root = contentRoot != null ? contentRoot : gameObject; // si no asignan root, usa self

        // Registrar listeners (el manager permanece activo así funciona el botón externo)
        if (prevButton) prevButton.onClick.AddListener(Prev);
        if (nextButton) nextButton.onClick.AddListener(Next);
        if (closeButton) closeButton.onClick.AddListener(Close);
        if (openButton) openButton.onClick.AddListener(Open);

        // Ocultar solo el contenido, no el script
        if (hideOnAwake && _root.activeSelf) _root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (prevButton) prevButton.onClick.RemoveListener(Prev);
        if (nextButton) nextButton.onClick.RemoveListener(Next);
        if (closeButton) closeButton.onClick.RemoveListener(Close);
        if (openButton) openButton.onClick.RemoveListener(Open);
    }

    public void Open()
    {
        if (slides == null || slides.Length ==0)
        {
            Debug.LogWarning("TutorialPanel: No hay slides asignados.");
            return;
        }
        _index = Mathf.Clamp(_index,0, slides.Length -1);
        if (!_root.activeSelf) _root.SetActive(true);
        UpdateSlide();
        AudioController.instance?.ButtonPressed();
    }

    public void Close()
    {
        if (_root.activeSelf) _root.SetActive(false);
        AudioController.instance?.ButtonPressed();
    }

    public void Next()
    {
        if (slides == null || slides.Length ==0) return;
        AudioController.instance?.ButtonPressed();
        if (_index >= slides.Length -1)
        {
            if (loopSlides) _index =0; else return;
        }
        else _index++;
        UpdateSlide();
    }

    public void Prev()
    {
        if (slides == null || slides.Length ==0) return;
        AudioController.instance?.ButtonPressed();
        if (_index <=0)
        {
            if (loopSlides) _index = slides.Length -1; else return;
        }
        else _index--;
        UpdateSlide();
    }

    private void UpdateSlide()
    {
        if (slideImage && slides != null && slides.Length >0)
        {
            slideImage.sprite = slides[_index];
        }
        if (hidePrevNextAtEnds && !loopSlides)
        {
            if (prevButton) prevButton.gameObject.SetActive(_index >0);
            if (nextButton) nextButton.gameObject.SetActive(_index < slides.Length -1);
        }
        else
        {
            if (prevButton) prevButton.gameObject.SetActive(true);
            if (nextButton) nextButton.gameObject.SetActive(true);
        }
    }
}
