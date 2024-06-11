![alttext](https://img.shields.io/badge/Unity%20version-6000-lightgrey&?style=for-the-badge&logo=unity&color=lightgray) ![alttext](https://img.shields.io/badge/O.S-Windiws%2011-lightgrey&?style=for-the-badge&color=purple)
# Texture organizing
Create a scriptable object for your "texture groups", a texture group is basicly a sets of PBR textures (albedo, normal, roughtness, metallic, ambient occlusion) for multiple objects in your Unity project. Textures that are used for the same shader can be considered to be in the same "Texture group". 
The textures can then easily be converted into Texture arrays for optimicing rendering performance. 


#### Installation
1. Download project and import using unity package manager with import local package option.
   or
2. Copy git URL and install with import from git url option.

#### How to use
1. Create a <b>Texture group settings</b> scriptable object by right click in your project view and select create > Texture Organizer > Texture Group settings.
2. Select an output path, make sure it exists and ends with '/'.
3. Select a filename for the Texture array output.
4. Expand the sets List and add a new set with the + button.
5. Add the different textures to the set
6. <b>The textures needs to be the same resolution</b>
7. Create an Texture array with the button.
8. The different Texture arrays will be output to the designated folder with the filename followed by a suffix to indicate which type it is for, abledo, normal, AO.
9. Create a new material with from the <b>"PBR_Texture_Array_URP_Lit_Shader"</b>. Add the different Texture arrays to the material.
10. On a the gameobject with the material and renderer, place a <b>SelectW</b> component. Use the slider to switch between the different textures slices.

That's it! Enjoy the improved performance!
The shader included can be expanded on if you need other effects. 

Feel free to come with sugesstions of features needed to make it more complete.
I wanted a minimalistic tool to easily keep your texture organized and performant and also easy to update the Texture arrays you create.
