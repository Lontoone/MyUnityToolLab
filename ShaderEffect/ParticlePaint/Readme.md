# Particle Painting

I was inspired by some cool tools on the asset store like [this](https://assetstore.unity.com/packages/tools/particles-effects/ink-painter-86210#content). So I tried to make my own.

## Feature

- Lit Support. (receive shadow.)

![image](https://i.imgur.com/7HATenW.png)


## How to use

- Add `Paintable` and `GPUInstancer` component on your gameobject.
- Create material from `Painting.shader`. (If you are using URP, use `URP_Paintable.shader`) and put to the `Instance Material` variable of GPUInstancer.
- Create a particle system gameobject and add `SplashSystem` componenet.

Preview:

![image](https://i.imgur.com/61wyhmM.gif)


The painting also sticks with objects.

![image](https://i.imgur.com/mpEvQXz.gif)

## Tutorial
🚧 [WIP]

