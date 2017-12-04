---

layout:     post
title:      "Unity动画系统经验谈"
subtitle:   ""
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/unity-girl3.jpg"

---

这里总结一下，自己使用Unity以来的心得，大部分属于随手解决但还有印象或者觉得效果不错。

## 状态机与状态机设计
![]({{ site.url }}/img/anim-state-machine.jpg)
角色的状态机以0层作为主层，然后以待机作混合树为中心进行切换。
![]({{ site.url }}/img/anim-state-machine2.jpg)
主层中会有一些复杂一些的混合树，通过参数调节做出动作融合，比如八方向Straft。
![]({{ site.url }}/img/anim-state-machine3.jpg) 主层还有会一个节点专门用于Override，后续会提到。
![]({{ site.url }}/img/anim-state-machine4.jpg)
在副层，用于非常规行为的动作融合，比如边走边打、边走边防御。
￼

## 动画事件

动画事件其实用得不多，主要在于标记动画某一帧需要触发什么逻辑的这种情况。比如RootMotion的拉扯等等。我更倾向于自己写逻辑去计时而不是交给动画事件。

动画事件，在执行到动画某一帧触发事件。如果动画过渡时可能会事件失效，更健壮的做法是自定义时间段。
	
状态事件，状态机的切换事件，但仍然是自己计算更加可靠简单。

## IK
IK算法有多种，一般而言，性能好就用CCD，对效果有要求就用FRAB。具体实现网上有大量资料。

在开放世界中，会放置很多预先打好TAG的交互物件。比如攀爬的IK，计算触发对象上最近的作用点，进行相应的IK操作。

IK如果想要做的自然还有很多corner case，比如身随手动、bad zone等等。在GDC上有大量的资料可以查到。

## Retarget

![]({{ site.url }}/img/retarget.jpeg)

Retarget就是一段动画可以服用到别的模型上。

Unity可以基于节点名字进行retarget。随手做一段clip都支持这个功能。

对于通用人型动画humanoid则支持更友好一些，名字不一样，依然可以通过avatar进行retarget。

如果想在别的形体也进行retarget，那么可以使用Morpheme中间件。

## 换装系统

![]({{ site.url }}/img/avatar.png)

这里讲几套换装方案

1. 最简单的是材质层面的换装，比如换贴图

2. dummypoint的方式也非常简单，在做好的角色上做预制的挂点，然后换武器

3. 静态蒙皮，预先在模型上做好几套蒙皮了的服装，换装的时候对mesh进行开关。

4. 动态蒙皮，重新绑蒙皮的方式支持角色换衣服，只要基于同一套骨骼开发，然后换装的时候重新赋值bone信息即可完成重新绑定蒙皮。如果使用一套材质还可以通过合并Mesh减少Drawcall。

5. 支持捏脸，参考uma系统。原理就是先把做好的部位顶点拼接起来，并且蒙皮。捏脸也是对顶点骨骼进行操作。

## RootMotion

![]({{ site.url }}/img/rootmotion.jpeg)

用RootMotion能让动画更加自然（一些游戏滑步就是因为不会用RootMotion），也有很多项目会单独把RootMotion单独输出美术路径拆开使用。但是RootMotion是静态的，对于动态的环境需要一些技巧来让RootMotion更加自然。

对于动态位移的情况，我们经常会遇到RootMotion够不到目的地的情况，那么对RootMotion最常用的操作就是将其缩放到合适的位移。

对于动态旋转的情况，也会遇到RootMotion的旋转角度不满足游戏性的需要或者因为混合偏移了。一个做法就是通过融合参数将角度进行逼近来控制动画旋转到合适的角度。

## Override
一般而言，我们直接通过AnimatorOverrite就可以替换了。但是如果多个Override的动作之间需要过渡，那么Unity会导致角色穿地。可以使用下面这种方案，支持任意多个Override动画过渡。

![]({{ site.url }}/img/anim-state-machine3.jpg)
三个状态机作为placeholder，每次替换都将还没使用的节点替换出来，然后进行过渡。替换之后需要用Animator.Update来刷新一下。

好处就是提高单个AnimatorController复用率，减少动画状态以及初始化涉及的clip数量。

## 物理动画

![]({{ site.url }}/img/gang-beasts.jpg)

Unity的动画系统很简单，不支持复杂的状态机，没法在里面加入IK、物理之类的节点，也无法嵌套混合树。这里可以最终动画系统输出之后，比如LateUpdate的方式，再进行一遍处理来调节骨骼。

dynamicbone，这个插件可以对指定的骨骼加入效果，从而让某个骨骼有震荡的效果。

puppet，这个插件的方案也是类似的。等动画输出之后，物理系统根据动画的位置校正ragdoll，在物理碰撞之后，把ragdoll再反算回骨骼，从而让角色动画和物理环境很好地进行交互。


## 性能问题
Animator初始化卡顿，Animator会把所有状态的AnimationClip加载到内存，最佳方法是用Override减少不必要的加载。

动画压缩，可以自行预处理一遍，控制下精度能省不少空间。还可以考虑其他方法，比如省略通道、曲线退化、骨骼剔除等等。