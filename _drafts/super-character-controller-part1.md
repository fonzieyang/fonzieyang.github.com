---

layout:     post
title:      "从DSL谈抽象"
subtitle:   "Unity自定义Character Controller 1 - 碰撞处理"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/unity-girl4.jpg"

---

在使用Unity开发了多个项目之后，我得出了两个结论：第一个就是，对于所有对游戏开发感兴趣的人来讲，Unity都是一个非常好的引擎。第二个就是其内建的Character Controller非常糟糕。在自定义Character Controller的工作上，已经持续了数周，查找相关资料是非常困难的。自从我找不到可读资料起，我就打算自己写这些资料。计划分享一下自己所学到的以及所处理的问题。在实现方面，我会使用上面提到的Unity游戏引擎来开发。你可以在[这里](https://unity3d.com)访问他们的网站，以及[这里](https://unity3d.com/unity/download)下载他们最新的版本。我实在很喜欢用Unity。它在处理很多底层问题的同时有给了开发者很多自由。而且它还有着非常活跃的社区，起到了很大的帮助。

不幸的是，之前也说了，Unity也带着世界上最糟糕的Character Controller。在与Unity对比之前，这里我阐明一下广义上的Character Controller是什么。那么Character Controller无非就是处理角色与游戏世界碰撞的一堆代码。它不像箱子、栅栏之类的可以有物理引擎统一处理，角色行为上的特殊性需要代码特殊处理。由于我们需要做碰撞检测，我们还是需要选择一个几何形体去表示我们的角色。大多数3D游戏都会选择胶囊体。

![](img/character-controller/class-charactercontroller-0.jpg)

胶囊体的广泛使用有着很多原因，具体原因我们晚一点再讨论。现在，为了让问题简单，我们先看看二维情况下的Character Controller。

你会看到