using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ScriptableObject bindingAsset;
    private FieldInfo[] uiElements;
    private int languageIndex;
    private List<Action> unsubscribeCallbacks = new List<Action>();
    
    void OnEnable()
    {
        // Lookup all Fields in the binding asset 
        uiElements = bindingAsset.GetType().GetFields();

        // Initialize localization settings
        StartCoroutine(SetLangaugeIndex());
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        // Initialize Bindings - loop through all fieldnames in the Binding Asset for matching UI Elements
        // The SO field name is also used as key to lookup the localized string
        // Multiple values (e.g. Dropdown options) have to be comma separated in the localized string
        // no need to bind the TextElements in the UI Toolkit - they are updated in OnLanguageChanged
        // but you can bind them if needed cause the SO field is also updated
    
        foreach (FieldInfo uiElement in uiElements)
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var button = root.Q<Button>(uiElement.Name);
            if(button != null)
            {
                // subscribe to clicked event and add action to unsubscribe list
                button.clicked += () => {StartCoroutine("On" + uiElement.Name); };
                unsubscribeCallbacks.Add(() => button.clicked -= () => {StartCoroutine("On" + uiElement.Name); });
                continue;
            }
            var dropDown = root.Q<DropdownField>(uiElement.Name);
            if(dropDown != null)
            {
                List<string> choises = LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name).Split(',').ToList();
                dropDown.label = choises[0];
                choises.RemoveAt(0);
                dropDown.SetValueWithoutNotify(choises[0]);
                dropDown.choices = choises;
                // subscribe to value changed event and add action to unsubscribe list
                dropDown.RegisterValueChangedCallback(v => StartCoroutine("On" + uiElement.Name, v.newValue)); 
                unsubscribeCallbacks.Add(() => dropDown.UnregisterValueChangedCallback(v => StartCoroutine("On" + uiElement.Name, v.newValue)));
                continue;
            }
            //Debug.Log("Binding Button: " + uiElement.Name); 
        }
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        foreach(var unsubscribe in unsubscribeCallbacks)
        {
            unsubscribe();
        }
        unsubscribeCallbacks.Clear();
    }
    private IEnumerator SetLangaugeIndex()
    {
        // get the current language index
        yield return LocalizationSettings.InitializationOperation;
        languageIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        OnLanguageChanged(LocalizationSettings.SelectedLocale);
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        // loop through all fields in the binding asset and update the UIElemts and the SO value from the localized strings
        foreach (FieldInfo uiElement in uiElements)
        {
            uiElement.SetValue(bindingAsset, LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name));
            var root = GetComponent<UIDocument>().rootVisualElement;
            var text = root.Q<TextElement>(uiElement.Name);
            if(text != null)
            {
                text.text = LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name);
                Debug.Log(uiElement.Name + ": " + LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name));
            }

            var dropdown = root.Q<DropdownField>(uiElement.Name);
            if(dropdown != null)
            {
                List<string> choises = LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name).Split(',').ToList();

                int actualIndex = dropdown.choices.IndexOf(dropdown.value);
                dropdown.label = choises[0];
                choises.RemoveAt(0);
                
                dropdown.SetValueWithoutNotify(choises[actualIndex]);
                dropdown.choices = choises;
            } 
        }
    }

    private IEnumerator OnChangeLanguageButton()
    {
        yield return LocalizationSettings.InitializationOperation;
        //  yield return LocalizationSettings.StringDatabase.PreloadOperation; // maybe needed in some cases
        languageIndex++;
        if (languageIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            languageIndex = 0;
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
        
    }
    private IEnumerator OnDropDownSample(string newValue)
    {
        yield return null;
        Debug.Log("Selected value: " + newValue);
    }
}
