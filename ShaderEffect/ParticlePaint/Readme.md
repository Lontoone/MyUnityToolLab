# Particle Painting

I was inspired by some cool tools on the asset store like [this](https://assetstore.unity.com/packages/tools/particles-effects/ink-painter-86210#content). So I tried to make my own.

## Feature

- Lit Support. (receive shadow.)

![image](https://i.imgur.com/7HATenW.png)


## How to use

- Add `Paintable` and `GPUInstancer` component on your gameobject.
- Create material from `Painting.shader`. (`URP_Paintable.shader` also work) and put to the `Instance Material` variable of GPUInstancer.
- Create a particle system gameobject and add `SplashSystem` componenet.


## SRP

If you are not using URP, you can use `Painting.shader` and change the tag:
```C
Tags {
    //"RenderPipeline" = "UniversalPipeline"
    "RenderType" = "Transparent"
    "RenderQueue" = "Transparent"
}
    //Tags { "LightMode" = "UniversalForward" }

```

Preview:

(In case image can not be loaded.)
- https://i.imgur.com/61wyhmM.gif
- https://i.imgur.com/mpEvQXz.gif

![image](https://i.imgur.com/61wyhmM.gif)


The painting also sticks with objects.

![image](https://i.imgur.com/mpEvQXz.gif)

## Tutorial
🚧 [WIP]

