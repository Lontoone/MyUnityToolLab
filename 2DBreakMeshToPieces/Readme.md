# BreakToPiece.cs
The main part of the function. Call `DoBreak()` to break the mesh.

`DoBreak()` will also call the event so you can decide what to do with every piece, for example, I use `PiecesComponent.cs` to make piece born with rigibody and destory after few seconds.

This support spriteSheet texture.

To break from an sprite animation, you have to put spritesheet file in Resources folider and set `img_resource_path` to that folider.

In gif, Mesh seems twisted because I set the random value `minVertexRandom`.
![image](https://i.imgur.com/QzRMreo.gif) 

*NOTE

Please don't set `subDivide` value too high because it will create a lot of mesh objects. 1~5 is recommended.

See demo game.

![image](https://i.imgur.com/d7fK9f2.gif) 
