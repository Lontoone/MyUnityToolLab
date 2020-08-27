# VR UI Button script
It contains `Pointer` and `PointerReciver` scripts. One is for user to open/close the ray, one act like unity button.

## Pointer Reciver

It provides `Pressed` `Hover` `Leave` Event. Just like Unity's button. Should be attached on a object with collider.

![image](https://i.imgur.com/n8gEB6i.png) 

## Pointer

It shoot ray as a pointer, and should be attached on player's hand.

![image](https://i.imgur.com/R66cld1.png) 

`Length` is for ray's length.
`Dot` is the end point of the ray. You can pot a sphere on it just for better visualization.
`Input Source` Which hand can use the ray.
`Open Input` Which button to open the ray.

