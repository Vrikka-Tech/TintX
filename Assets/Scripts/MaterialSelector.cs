using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MaterialSelector : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Dropdown colorDropdown;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject sphereCursor;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private UnityEngine.UI.Button screenshotButton;

    private Renderer selectedRenderer;
    private Dictionary<GameObject, Material> objectMaterials = new(); // Store materials per object
    private Dictionary<string, Color> colorDictionary = new()
    {
        { "Red", Color.red },
        { "Green", Color.green },
        { "Blue", Color.blue },
        { "Yellow", Color.yellow },
        { "Magenta", Color.magenta },
        { "Cyan", Color.cyan },
        { "Black", Color.black },
        { "White", Color.white },
        { "Gray", Color.gray }
    };

    private void Start()
    {
        popupPanel.SetActive(false);
        sphereCursor.SetActive(false);
        PopulateColorDropdown();
        LoadMaterials(); // Load saved materials
        screenshotButton?.onClick.AddListener(TakeScreenshot);
    }

    private void Update()
    {
        // Update the sphere cursor to follow the mouse.
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            sphereCursor.SetActive(true);
            sphereCursor.transform.position = hit.point + new Vector3(0, 0.1f, 0);
        }
        else
        {
            sphereCursor.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0) && selectedRenderer == null)
        {
            FreeCameraController.canRotate = false;
            RaycastHit hitObject;
            Ray objectRay = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(objectRay, out hitObject))
            {
                GameObject hitObjectGO = hitObject.collider.gameObject;

                if (hitObjectGO.GetComponent<Renderer>() != null)
                {
                    selectedRenderer = hitObjectGO.GetComponent<Renderer>();

                    // Assign a unique material instance if not already assigned
                    if (!objectMaterials.ContainsKey(hitObjectGO))
                    {
                        objectMaterials[hitObjectGO] = new Material(selectedRenderer.material);
                    }

                    selectedRenderer.material = objectMaterials[hitObjectGO]; // Assign unique material
                    ShowColorDropdown(hitObjectGO);
                }
            }
        }
    }

    private void PopulateColorDropdown()
    {
        colorDropdown.ClearOptions();
        colorDropdown.AddOptions(new List<string>(colorDictionary.Keys));
    }

    public void ShowColorDropdown(GameObject target)
    {
        popupPanel.SetActive(true);
        colorDropdown.onValueChanged.RemoveAllListeners();
        colorDropdown.onValueChanged.AddListener((value) => ChangeColor(value, target));
    }

    void ChangeColor(int selectedColorIndex, GameObject target)
    {
        if (objectMaterials.ContainsKey(target))
        {
            string selectedColorName = colorDropdown.options[selectedColorIndex].text;
            if (colorDictionary.TryGetValue(selectedColorName, out Color selectedColor))
            {
                objectMaterials[target].color = selectedColor;
                target.GetComponent<Renderer>().material = objectMaterials[target];
            }
        }
    }

    public void SaveChanges()
    {
        if (selectedRenderer != null)
        {
            SaveMaterial(selectedRenderer.gameObject);
        }

        popupPanel.SetActive(false);
        selectedRenderer = null;
        FreeCameraController.canRotate = true;
    }

    private void SaveMaterial(GameObject target)
    {
        if (objectMaterials.ContainsKey(target))
        {
            string key = $"MaterialColor_{target.name}";
            string colorString = ColorUtility.ToHtmlStringRGBA(objectMaterials[target].color);
            PlayerPrefs.SetString(key, colorString);
            PlayerPrefs.Save();
            // Debug.Log($"Saved color for {target.name}: {colorString}");
        }
    }

    private void LoadMaterials()
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.GetComponent<Renderer>() != null)
            {
                string key = $"MaterialColor_{obj.name}";
                if (PlayerPrefs.HasKey(key) && ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(key), out Color savedColor))
                {
                    Material newMaterial = new Material(obj.GetComponent<Renderer>().material)
                    {
                        color = savedColor
                    };

                    obj.GetComponent<Renderer>().material = newMaterial;
                    objectMaterials[obj] = newMaterial;
                    // Debug.Log($"Loaded color for {obj.name}: {savedColor}");
                }
            }
        }
    }

    public void ResetChanges()
    {
        if (selectedRenderer != null && objectMaterials.ContainsKey(selectedRenderer.gameObject))
        {
            objectMaterials[selectedRenderer.gameObject] = new Material(selectedRenderer.material);
            selectedRenderer.material = objectMaterials[selectedRenderer.gameObject];
        }
        FreeCameraController.canRotate = true;

        popupPanel.SetActive(false);
        selectedRenderer = null;
    }

    public void TakeScreenshot()
    {
        ScreenshotManager.Instance.CaptureScreenshot();
    }
}
