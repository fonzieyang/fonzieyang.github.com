---

layout:     post
title:      "Unity超级角色控制器 - Part 5"
subtitle:   "Release 1.0.0"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/super-mario-3d-world.jpg"

---

现在有两种下载控制器的途径：要么得到包含工程的.zip文件，要么是.unitypackage文件。例子工程中包含了一个例子场景，应用到了我们的角色控制器。

![]({{ site.url }}img/character-controller/controllerexample.jpg)

[下载角色控制器]({{ site.url }}super-character-controller-part0)

不管是打开工程文件还是导入unitypackage，都会有一个目录下存放着核心代码，叫做SuperCharacterController®。目录下有一个RPGController文件夹包含了[fholm](http://forum.unity3d.com/members/fholm.59346/)编写的用于创建网格树的类，一个README文件，一个Math3d类(by [BitBarrelMedia](http://bitbarrelmedia.wordpress.com/))，DebugDraw类(by [Roystan Ross](https://roystanross.wordpress.com/))以及一个Core目录存放着角色控制器相关的所有代码。

你最好不要修改RPGController目录下的任何代码，除非你打算重写网格树。Math3d则是广泛应用在角色控制器里面。DebugDraw是一个在屏幕上绘制线段的类，可以在普通函数里面调用绘制，非常方便。

Core目录下有五个类。SuperMath是Mathf中所没有的一些数学函数的静态类。SuperCollider也是一个静态类，唯一的用途就是查找几何体(盒子、球体等等)的最近点。SuperCollisionType是用来放在那些需要与角色控制器交互的物体上，从而自定义该物体的交互属性(地表类型、斜率等等)。

SuperStateMachine则是来自[Unity Gems Finite State Machine tutorial](https://web.archive.org/web/20140702051240/http://unitygems.com/fsm1/)的修改版本。我的版本则是更加简单，同时也更加强大。简单易用的状态机实现对于大部分游戏来说都很重要。这个状态机是专门与SuperCharacterController一起使用的。角色的状态机逻辑实现在SuperStateMachine子类当中。我通常按照"角色名+Machine"的方式使用。比如我的Mario64项目中，有MarioMachine、GoombaMachine、BobombMachine，它们都继承自SuperStateMachine。

最后，是SuperCharacterController©™®的个人时间。通常来讲，只需要将该组件添加到对象上就完事了。它会派发"SuperUpdate"消息出去，在这里是实现角色逻辑以及移动。就跟使用Unity角色控制器一样，经常会cache角色控制器引用。这样，就可以直接访问public变量以及public方法。对于更深入的使用，请看例子工程。

我尽可能地自己测试工程，但这是我第一次发布这么大的项目，因此我真的希望代码能够顺利在各位的机器上运行。如果发现任何问题，请与我反馈。

最后，呃，其实SuperCharacterController并没有任何商标/授权/版权。

