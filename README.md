UI Manager Readme
This little Manager for Unity is made for convenient coupling of the UI Toolkit Elements to the game logic.
Requirements:
•	Unity 6 (tested with Unity 6.3 LTS)
•	Unity UI from Package Manager (tested with 2.0.0) - and a UI of course
•	Localization from Package Manager (tested with 1.5.9)
•	Setup the Localization and create a localization table. (Quick Start Guide | Localization | 1.5.9)
•	Set the String Database Default Table Reference in Project Setting -> Localization
•	Adapt the FieldNames in the SampleBindingAsset Script to your UI Element Names and create a Binding Asset Scriptable Object from it. 
•	Create a GameObject and attach the UIManager Script and the Binding Asset Scriptable Object in the Inspector.

Goal:
Scriptable Object based UI Toolkit integration as highly automated as possible. The UIManager uses the Binding Asset SO field names to lookup the UI Elements. The SO field value is used as ley for the localization to lookup the entries. Multiple entries (as for DropDown fields) are separated by commas.
Current State:
Actually integrated are Labels, Buttons and Dropdowns.

License:
CC0-1.0 (feel free to do what you want)
