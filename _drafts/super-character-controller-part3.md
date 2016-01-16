---

layout:     post
title:      "Unity超级角色控制器 - Part 3"
subtitle:   "物理API分析"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/super-mario-3d-world.jpg"

---

到目前为止，我讲了几个物理API，但是我们还没有谈到细节。相信机智的读者看到标题之后都已经猜到了，这就是接下来要讨论的。我们会讨论函数的可用性，分析会遇到的问题，以及相应的解决方案。

像之前一样，我尽可能避免造轮子，这里会大量参考fholm的[帖子](http://forum.unity3d.com/threads/142375-The-limitations-of-the-physics-API-and-creating-a-character-controller)。

![]({{ site.url }}img/character-controller/fholm.jpg)

### 物理API

由于很多函数都有很多变种，所以这里就不花太多时间在[物理文档](https://docs.unity3d.com/Documentation/ScriptReference/Physics.html)上。我不打算枯燥地把所有API都讲一遍，它们之间的差别无非就是光线是否打中物体就结束之类的。

**Raycast**: 打出一条指定方向与长度(也许无限远)的射线。如果有一个对象被打中了，就可以从**RaycastHit**结构体得到有用的信息:打中哪里，打中点表面的法线如何等等。因为它只能打出无限细小的射线，因此不会用于做碰撞处理。

**CapsuleCastAll**: 第一眼看上去，这个函数用在角色控制器上是非常理想的(由于它使用的是胶囊体形状)。值得注意的是，这是一个投射，它只能检测法线面上投射的表面，背面是无法检测出来的。也就是说，投射无法检测出包围了胶囊体初始位置的物体，也就是那些与初始位置相交的对象。如果我们想将其用于角色控制器的开发，这是一个必须要攻克的缺陷。

**CheckCapsule**: 这次我们有了一个解决刚刚提到的问题的候选人。CheckCapsule看上去正是可以解决CapsuleCastAll无法检测到的初始位置的问题。不幸的是，它只能返回布尔值，而不是一组碰撞体，缺少了我们所需要的信息。

**CheckSphere**: 与上面一样，只是换了球形。

**Linecast**: 标准的投射函数。只是换了一种定义射线初始位置、方向、长度的方式。

**OverlapSphere**: 现在我们终于找到了。OverlapSphere的行为就跟名字一样。请注意文档中的这一行:

> 注意：目前它只能检测出碰撞物体的包围盒，而不是真正的碰撞提。

我真的不知道它为什么要这么写。因为我已经测试过Box Collider、Sphere Collider、Mesh Collider，用起来确实是检测出了真正的碰撞体而不是包围盒。这里我所理解的包围盒是轴对齐包围盒。我只能认为是文档错误。

**RaycastAll**: 与Raycast一样，除了不会在第一个碰撞的对象就停下来之外。

**SphereCastAll**: 与CapsuleCastAll一样，有着同样无法检测初始位置相交对象的缺点。SphereCast也不能保证返回正确的碰撞法线。因为这是投射一个球出去，它可以与网格的边相交。当与网格的边碰撞时，hit.normal返回的是边所连接的两个顶点的法线插值。由于CapsuleCast也只是投射一个扫略体，因此与网格的边碰撞时也是同样处理。

除了上面提到的工具以外，Unity还提供了[Rigidbody.SweepTestAll](https://docs.unity3d.com/Documentation/ScriptReference/Rigidbody.SweepTestAll.html)方法。经过测试，它的行为看起来与投射方法差不多。包围了面片的碰撞体无法被检测出来。比起SweepTestAll，我更倾向于使用CapsuleCastAll和SphereCastAll，因为它们有着更多的可选项(比如选择初始位置)，但是SweepTest对于方形的角色很有用，因为没有BoxCast方法。

### 网格碰撞体

在我们进一步之前，我们讲讲网格碰撞体(Mesh Collider)。到目前为止，我们只研究了基础形状(Box、Sphere、Capsule等等)。但是在实际的关卡当中，关卡会用到网格碰撞体。

与基础形状不同的是，基础形状都通过预定义的参数来定义其形状(球体的半径、盒子的高度等等)，一个网格碰撞体的碰撞数据是通过3D网格来定义的。网格碰撞体主要分两类: 凸多边形与凹多边形。[这篇文档](http://www.rustycode.com/tutorials/convex.html)很好地解释了两者的异同。

凸多边形所有面片必须是封闭的，而且Unity限制了多边形数量上限为255，用它来表示错综复杂的关卡地形是不理想的。凹多边形可以是任意形状，但是缺点是不保证封闭。也就是说比起固体来讲，它们更多的是一块面片。这意味着我们再也不会检测出来某个对象在凸多边形面片内部。这也带来了一个相位问题。相位会在角色移动速度很快的时候发生。就是两帧之间直接就穿过去了，没有发生任何碰撞。而凸多边形使得问题更容易发生了。

![]({{ site.url }}img/character-controller/screen11.jpg)

* 角色控制器一帧的运动，他的速度大的足以让他一帧就直接穿过了墙体

假设我们就紧贴着网格碰撞提，而且法线正对着我们的方向，我们能够移动的最大距离是半径的两倍。想象一下我们碰撞处理的做法是将对象紧贴着墙面，因此这种情况很容易发生。如果你的角色大概2米高，半径为0.5米。那么角色的最大移动速度为每帧1米。如果游戏运行在30帧，那么每秒30米，每小时108千米。看上去很快，但是对于索尼克这类游戏来说还不够。

![]({{ site.url }}img/character-controller/screen21.jpg)

* 因为角色紧贴着表面，不能速度快过半径两倍，否则会发生相位。

一个方法就是让物理引擎每帧运行多次。也可以使用CapsuleCastAll来检测每一帧起点与终点之间的碰撞体。我们会在后续的文章中讲讲这部分的内容。


