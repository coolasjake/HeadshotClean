using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager stat;

    public Canvas canvas;

    public Dictionary<string, GameObject> loadedUI = new Dictionary<string, GameObject>();

    public GameObject LoadOrGetUI(string nameOfUI)
    {
        if (loadedUI.ContainsKey(nameOfUI))
            return loadedUI[nameOfUI];
        else
        {
            GameObject elementPrefab = Resources.Load<GameObject>("UIElements/" + nameOfUI);
            GameObject element = Instantiate(elementPrefab, canvas.transform);
            element.transform.SetAsFirstSibling();
            loadedUI.Add(nameOfUI, element);
            return element;
        }
    }

    void Awake()
    {
        if (stat == null)
            stat = this;

        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
    }
}
