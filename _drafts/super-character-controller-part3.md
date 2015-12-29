---

layout:     post
title:      "Unity自定义角色控制器 Part 3"
subtitle:   "物理的API分析"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/unity-girl4.jpg"

---

到目前为止，我讲了几个物理API，但是我们还没有谈到细节。相信机智的读者看到标题之后都已经猜到了，这就是接下来要讨论的。我们会讨论函数的可用性，分析会遇到的问题，以及相应的解决方案。

像之前一样，我尽可能避免造轮子，这里会大量参考fholm的[帖子](http://forum.unity3d.com/threads/142375-The-limitations-of-the-physics-API-and-creating-a-character-controller)。

![](img/character-controller/fholm.jpg)

### 物理API

由于很多函数都有很多变种，因此不需要花太多时间在[物理文档](https://docs.unity3d.com/Documentation/ScriptReference/Physics.html)上。我不打算枯燥地把所有变种都讲一遍，它们之间的差别无非就是光线是否打中物体就结束之类的。