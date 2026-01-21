using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BindingAsset", menuName = "Scriptable Objects/BindingAsset")]

[Serializable]
public class BindingAsset : ScriptableObject
{
    public string LanguageLabel;
    public string ChangeLanguageButton;

    public string DropDownSample;
    public string DropDownLabel;
}
