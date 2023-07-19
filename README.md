# ReadMe: SoundInstanceManager (It's still in development so yeah, things can and will change)

## Overview
The **"SoundInstanceManager"** is a audio management tool that allows the user to manually or automatically manipulate various sound properties inside one handy manager script. It currently allows manipulation of Unity's built-in Audio Solution, aswell as FMOD events.

## Package installation

To install the package, unzip the archive, go to your Unity project folder and place unzipped archive into the **"Packages"** folder.

Next, open your Unity project and open the **"Package Manager"** under *"Window->Package Manager"*.

In the top left corner, click on the **plus sign** and select **"Add package from disc"** and select the **"SoundInstanceManager"** folder inside the Packages folder of your Unity project.

The package and its files should now be inside your project.

## How to use


### Sound Instance Editor (Unity)

To utilize the **Sound Instance Manager**, each audio source will need a **"Sound Instance Editor Unity"** script, as well as a **"Audio Source"**. 

The **"Audio Source"** is responsible for playing the audio source and manipulating various attributes of the sound. It is Unity's audio solution for playing sounds.

*Show picture Audio Source*

The **"Sound Instance Editor"** acts as a *"extension"* to the **"Audio Source"**. It will give the user a bit more control over the slider values for more fine-tuning. It also allows for an external script to manipulate the audio attributes.
Let's go through each individual field:

- The **"Audio Source"** field expects a ***"Audio Source"*** script. It won't work without one attached.

- The **"Script"** field is a optional field. When attached, it will look for any properties that will manipulate the audio attributes automatically. There will be more information on that later.

- The **"Editor Name"** field will give the Instance Editor a name, that will show up in the **"Sound Instance Manager"**

- The **"Editor Level"** is a uniform value that will controll all properties ***simultaneously***. When toggled **on**, it will override the current input values of all properties. It will also override all automatic bindings, by the external script. Just like other properties, the **"Editor Level"** can be handled manually or by a script.

- The **"Preset"** dropdown, will display all available user-defined presets. A **"preset"** is a collection of audio properties. For example, a **"Stress"** preset may contain *"Volume"*, *"Pitch"* and *"Spatial Blend"* as attributes.
- The "Property Type" , controls the evaluation of the input value. There are currently three property types: ***Curve***, ***Level***, and ***Linear***.
- The **"Curve"** property type will evaluate the output value given the **"AnimationCurve"**. The user can map the time and value to his liking, although it is recommended to use a ***normalized*** value for time!
- The **"Level"** property type will evaluate the output, through a min-max slider. If the input value resides between the sliders minimum and maximum values, the output will be either a 1 or 0. This is recommended for **boolean** properties.
- The **"Linear"** property type will evaluate the input linearly between a minimum and maximum value. This is usually recommended for properties like ***Volume*** or ***Pitch***.

- The **"Add new property"** button will add a new audio property to the currently selected preset. When clicked, you'll find a dropdown menu with a selection of different audio properties. Clicking **"add"** will add the audio property to the list of properties, in the selected preset.

- The **"Save configuration as preset"**, will let you save the current configuration of audio properties as a custom preset. It will display a window, which let's you save the preset, under a name and place of your choice. 
However, in order for the **"Instance Editor"** to show the presets, the asset will need to be placed inside the ***"SoundInstanceEditorUnityParameterPresets"*** folder.
This button will only show up when no preset has been selected.
Otherwise, changes are directly applied to the current preset.

### Sound Instance Manager (Unity)

The **Sound Instance Manager** displays all **Sound Instance Editors** in the scene. Additionally the Sound Instance Manager has a **Manager Level**, that will override ***all*** inputs and control all editor instances at once. Think of it like a master level.
A check will enable or disable the override.

### Sound Instance Editor (FMOD)

If the **"Unity Sound Instance Editor"** is like a extension to the **"Audio Source"** component, then the **"FMOD Sound Instance Editor"** is like a extension to **FMOD events**.

This Editor helps controlling basic sound properties and, most importantly, ***custom event parameters***.

In addition to the **"Unity Sound Instance Editor"**, the editor requires a GameObject with a **FMOD Sound Manager** script to be somewhere in the scene. The Sound Manager is responsible for managing instances of FMOD event references and is required for playing back any FMOD sounds.

Now, let's go through each part of the inspector of the instance editor.

- The **"Script"** field takes in a ***script*** from any GameObject and will bind properties to the FMOD Event parameters and basic sound property options of the instance editor.

- The **"Event Reference"** field provides a interface to select a FMOD Event from the Bank. A event is required for the editor to work.
- The **"Playback State"** will display the, you guessed it, current playback state of the event.
- The **"Presets"** dropdown will show user-defined presets for the basic audio properties ***Pitch***, ***Volume*** and ***Level***
- The **"Pitch"** Slider let's you control the pitch. It ranges from 0.5 to 2, which is equivalent to one octave down and up from the default value of one.
- The **"Value"** slider let's you control the master volume of the event. It ranges from 0 - 1, equivalent to 0 to 100% of volume.
- The **"Level"** Slider determines when the event is played. The input value of the SoundInstanceManager for FMOD will controll the playback of the event, if the value lies between the minimum and maximum value of the Level slider.
- **"Show Additional Parameters"**, will show all custom parameters set in the FMOD event.

### How to bind property value from script

The **"Instance Editor"** uses reflection to check for properties with the ***same name*** as the sound property.

In this example, the **"Test"** script will have a **"Volume"** and a **"Editor Level"** property, that will be updated randomly each second.

Drag and drop the script in the **"Script"** field of the **"Instance Editor"**.

If the properties are setup correctly, you'll see the values update together with the script. You'll also see a text indicating that the property is controlled by a property (or the **"editor level"**).


### Sound Instance Manager (FMOD)

The **Sound Instance Manager** for FMOD displays all **Sound Instance Editors** in the scene, just like the one for Unity. It is basically the same as the other Manager.
