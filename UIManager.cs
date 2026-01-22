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
                // set button text from localized string
                button.text = LocalizationSettings.StringDatabase.GetLocalizedString(uiElement.Name);
                // subscribe to clicked event and add action to unsubscribe list
                Action actiontoAdd = () => StartCoroutine("On" + uiElement.Name);
                button.clicked += actiontoAdd;
                unsubscribeCallbacks.Add(() => button.clicked -= actiontoAdd);

                continue;
            }
            var dropDown = root.Q<DropdownField>(uiElement.Name);
            if(dropDown != null)
            {
                // set dropdown label and choices from localized string
                SetDropDownValues(dropDown, uiElement.Name);
                //subscribe to value changed event and add action to unsubscribe list
                Action action = () => StartCoroutine("On" + uiElement.Name, dropDown.value);
                dropDown.RegisterValueChangedCallback(v => action()); 
                unsubscribeCallbacks.Add(() => dropDown.UnregisterValueChangedCallback(v => action()));
                continue;
            }
            //Debug.Log("Binding Button: " + uiElement.Name); 
        }
    }

    void OnDisable()
    {
        // actually unsubscribing from DropDowns with UnregisterValueChangedCallback does not work. Actions are not a valid reference
        // maybe in the futrue unity will provide a way to unregister all callbacks. 
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        foreach(var unsubscribe in unsubscribeCallbacks)
        {
            unsubscribe();
            Debug.Log("Unsubscribed a callback");
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

    private void SetDropDownValues(DropdownField dropdown, string fieldName)
    {
        string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(fieldName);
        List<string> choises = localizedString.Split(',').ToList();
        dropdown.label = choises[0];
        choises.RemoveAt(0);
        dropdown.SetValueWithoutNotify(choises[0]);
        dropdown.choices = choises;
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        // loop through all fields in the binding asset and update the UIElemts and the SO value from the localized strings
        Debug.Log("Language changed to: " + newLocale.Identifier.Code);
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
                SetDropDownValues(dropdown, uiElement.Name);
            } 
        }
    }

    
    #region UI Callbacks
    // Example UI Callbacks - add your own here matching the field names in the Binding Asset with "On" prefix
    private IEnumerator OnChangeLanguageButton()
    {
        Debug.Log("OnChangeLanguageButton clicked");
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
        Debug.Log("Selected value: " + newValue);
        yield return null;
        
    }

    #endregion
}
