# VR Room Builder

My project is a VR room-building application that lets users design and customize a virtual room in an immersive environment. Users can open a floating furniture menu, spawn different furniture items, move them around the room with hand or ray interactions, rotate and scale them, change wall materials, and save or load room layouts for later editing. The purpose of the project is to make interior design more interactive and accessible by allowing users to experiment with furniture placement, room style, and layout ideas directly in VR before applying them in a real or digital space.

## Important Project Folders

The main Unity project files are located in the following folders:

- [`Assets`](./Assets) - contains the main project assets, scenes, scripts, prefabs, materials, furniture models, UI objects, and custom resources.
- [`Assets/Scripts`](./Assets/Scripts) - contains the C# scripts used for furniture spawning, selection, scaling, saving/loading, wall material selection, UI behavior, and VR interaction logic.
- [`Assets/Data`](./Assets/Data) - contains ScriptableObject data assets such as furniture item definitions and the furniture catalog.
- [`Assets/Prefab`](./Assets/Prefab) - contains furniture prefabs and UI prefabs used by the room builder system.
- [`Assets/Resources`](./Assets/Resources) - contains runtime-loadable resources, such as wall materials used by the wall material browser.

## Main Features

- Floating VR furniture menu
- Furniture category filtering
- Furniture spawning from item cards
- Hand grab and ray grab interaction
- Selected object highlighting
- Stick-based movement and rotation
- Scale handle with axis-based scaling
- Proportional scale lock and reset scale button
- Wall material browser
- Room save/load system
- Saved room browser with thumbnails

## Controls

- **Open/close furniture menu:** A button  
- **Select furniture:** Ray grab or hand grab  
- **Deselect furniture:** B button  
- **Rotate selected furniture:** Right stick left/right  
- **Move selected furniture:** Right stick up/down  
- **Scale selected furniture:** Pull the scale handle ball  
- **Save/load rooms:** Use the bottom buttons in the furniture menu  

## Project Video

Project video link:  
[Watch the project demo](https://www.youtube.com/watch?v=TmVBniyZB2M)
