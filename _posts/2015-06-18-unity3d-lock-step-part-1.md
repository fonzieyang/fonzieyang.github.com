---

layout:     post
title:      "Unity3D中实现帧同步 - Part 1"
subtitle:   ""
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/unity-girl3.jpg"

---


在帧同步模型中，每个客户端都会对整个游戏世界进行模拟。这种方法的好处在于减少了需要发送的信息。帧同步只需要发送用户的输入信息，而对于反过来的中心服务器模型来说，单位的信息则发送越频繁越好。

比如说你在游戏世界中移动角色。在中心服务器模型中，物理模拟只会在服务器执行。客户端告诉服务器，角色要往哪个方向移动。服务器会执行寻路而且开始移动角色。服务器紧接着就会尽可能频繁地告知每个客户端该角色的位置。对于游戏世界中的每个角色都要运行这样的过程。对于实时策略游戏来说，同步成千上万的单位在中心服务器模型中几乎是不可能的任务。

在帧同步模型中，在用户决定移动角色之后，就会告诉所有客户端。每个客户端都会执行寻路以及更新角色位置。只有用户输入的时候才需要通知每个客户端，然后每个客户端都会自己更新物理以及位置。

这个模型带来了一些问题。每个客户端的模拟都必须执行得一模一样。这意味着，物理模拟必须执行同样的更新次数而且每个动作都需要同样的顺序执行。如果不这么做，其中一个客户端就会跑在其他客户端之前或者之后，然后在新的命令发出之后，跑得太快或者太慢的客户端走出的路径就会不同。这些不同会根据不同的游戏玩法而不同。

另一个问题就是跨不同的机器和平台的确定性问题。计算上很小的不同都会对游戏造成蝴蝶效应。这个问题会在后续的文章中讲到。

这里的实现方案灵感来自于这篇文章：[《1500个弓箭手》](http://www.gamasutra.com/view/feature/3094/)。每个玩家命令都会在后续的两个回合中执行。在发送动作与处理动作之间存在延迟有助于对抗网络延迟。这个实现还给我们留下了根据延迟以及机器性能动态调整每回合时长的空间。这部分在这里先不讨论，会在后续文章再说。

对于这个实现，我们有如下定义：

**帧同步回合**

###***帧同步回合可以由多个游戏回合组成。玩家在一个帧同步回合执行一个动作。帧同步回合长度会根据性能调整。目前硬编码为200ms。***
	
**游戏回合**	

###***游戏回合就是游戏逻辑和物理模拟的更新。每个帧同步回合拥有的游戏回合次数是由性能控制的。目前硬编码为50ms，也就是每次帧同步回合有4次游戏回合。也就是每秒有20次游戏回合。***
	
**动作**	

###***一个动作就是玩家发起的一个命令。比如说在某个区域内选中单位，或者移动选中单位到目的地。***
	
注意：我们将不使用unity3d的物理引擎。而是使用一个确定性的自定义引擎。在后续文章中会有实现。	

### 游戏主循环

Unity3d的循环是运行在单线程下的。可以通过在这两个函数插入自定义代码：

* Update()
* FixedUpdate()

Unity3d的主循环每次遍历更新都会调用Update()。主循环会以最快速度运行，除非设置了固定的帧率。FixedUpdate()会根据设置每秒执行固定次数。在主循环遍历中，它会被调用零次或多次，取决于上次遍历所花费的时间。FixedUpdate()有着我们想要的行为，就是每次帧同步回合都执行固定时长。但是，FixedUpdate()的频率只能在运行之前设置好。而我们希望可以根据性能调节我们的游戏帧率。

### 游戏帧回合

这个实现有着与FixedUpdate()在Update()函数中执行所类似的逻辑。主要不同的地方在于，我们可以调整频率。这是通过增加"累计时间"来完成的。每次调用Update()函数，上次遍历所花费的时间会添加到其中。这就是Time.deltaTime。如果累计时间大于我们的固定游戏回合帧率(50ms)，那么我们就会调用gameframe()。我们每次调用gameframe()都会在累计时间上减去50ms，所以我们一直调用，知道累计时间小于50ms。

{% highlight C++ %}
private float AccumilatedTime = 0f;
 
private float FrameLength = 0.05f; //50 miliseconds

//called once per unity frame

public void Update() {
    //Basically same logic as FixedUpdate, but we can scale it by adjusting FrameLength

    AccumilatedTime = AccumilatedTime + Time.deltaTime;
 
    //in case the FPS is too slow, we may need to update the game multiple times a frame

    while(AccumilatedTime > FrameLength) {
        GameFrameTurn ();
        AccumilatedTime = AccumilatedTime - FrameLength;
    }
}
{% endhighlight %}

我们跟踪当前帧同步回合中游戏帧的数量。每当我们在帧同步回合中达到我们想要的游戏回合次数，我们就会更新帧同步回合到下一轮。如果帧同步还不能到下一轮，我们就不能增加游戏帧，而且我们会在下一次同样执行帧同步检查。

{% highlight C++ %}
private void GameFrameTurn() {
    //first frame is used to process actions

    if(GameFrame == 0) {
        if(LockStepTurn()) {
            GameFrame++;
        }
    } else {
        //update game
 
        //...
         
        GameFrame++;
        if(GameFrame == GameFramesPerLocksetpTurn) {
            GameFrame = 0;
        }
    }
}
{% endhighlight %}

在游戏回合中，物理模拟会更新而且我们的游戏逻辑也会更新。游戏逻辑是通过接口(IHasGameFrame)来实现的，而且添加这个对象到集合中，然后我们就可以进行遍历。

{% highlight C++ %}
private void GameFrameTurn() {
    //first frame is used to process actions

    if(GameFrame == 0) {
        if(LockStepTurn()) {
            GameFrame++;
        }
    } else {
        //update game

        SceneManager.Manager.TwoDPhysics.Update (GameFramesPerSecond);
         
        List<IHasGameFrame> finished = new List<IHasGameFrame>();
        foreach(IHasGameFrame obj in SceneManager.Manager.GameFrameObjects) {
            obj.GameFrameTurn(GameFramesPerSecond);
            if(obj.Finished) {
                finished.Add (obj);
            }
        }
         
        foreach(IHasGameFrame obj in finished) {
            SceneManager.Manager.GameFrameObjects.Remove (obj);
        }
         
        GameFrame++;
        if(GameFrame == GameFramesPerLocksetpTurn) {
            GameFrame = 0;
        }
    }
}
{% endhighlight %}

IHasGameFrame接口有一个方法叫做GameFrameTurn，它以当前每秒游戏帧的个数为参数。一个具体的带游戏逻辑的对象应该基于GameFramesPerSecond来计算。比如说，如果一个单位正在攻击另一个单位，而且他攻击频率为每秒钟10点伤害，你可能会通过将它除以GameFramesPerSecond来添加伤害。而GameFramesPerSecond会根据性能进行调整。

IHasGameFrame接口也有属性标记着结束。这使得实现IHasGameFrame的对象可以通知游戏帧循环自己已经结束。一个例子就是一个对象跟着路径行走，而在到达目的地之后，这个对象就不再需要了。

### 帧同步回合

为了与其他客户端保持同步，每次帧同步回合我们都要问以下问题：

* 我们已经收到了所有客户端的下一轮动作了吗？
* 每个客户端都确认得到我们的动作了吗？

我们有两个对象，ConfirmedActions和PendingActions。这两个都有各自可能收到消息的集合。在我们进入下一个回合之前，我们会检查这两个对象。

{% highlight C++ %}
private bool NextTurn() {       
    if(confirmedActions.ReadyForNextTurn() && pendingActions.ReadyForNextTurn()) {
        //increment the turn ID

        LockStepTurnID++;
        //move the confirmed actions to next turn

        confirmedActions.NextTurn();
        //move the pending actions to this turn
        
        pendingActions.NextTurn();
         
        return true;
    }
     
    return false;
}
{% endhighlight %}

### 动作

动作，也就是命令，都通过实现IAction接口来通信。有着一个无参数函数叫做ProcessAction()。这个类必须为Serializable。这意味着这个对象的所有字段也是Serializable的。当用户与UI交互，动作的实例就会创建，然后发送到我们的帧同步管理器的队列中。队列通常在游戏太慢而用户在一个帧同步回合中发送多于一个命令的时候用到。虽然每次只能发送一个命令，但没有一个会忽略。

当发送动作到其他玩家的时候，动作实例会序列化为字节数组，然后被其他玩家反序列化。一个默认的"非动作"对象会在用户没有执行任何操作的时候发送。而其他则会根据特定游戏逻辑而定。这里是一个创建新单位的动作：

{% highlight C++ %}
using System;
using UnityEngine;
 
[Serializable]
public class CreateUnit : IAction
{
    int owningPlayer;
    int buildingID;
     
    public CreateUnit (int owningPlayer, int buildingID) {
        this.owningPlayer = owningPlayer;
        this.buildingID = buildingID;
    }
     
    public void ProcessAction() {
        Building b = SceneManager.Manager.GamePieceManager.GetBuilding(owningPlayer, buildingID);
        b.SpawnUnit();
    }
}
{% endhighlight %}

这个动作会依赖于SceneManager的静态引用。如果你不喜欢这个实现，可以修改IAction接口，使得ProcessAction接收一个SceneManager实例。

实例代码可以在下面找到：

[Bitbucket – Sample Lockstep](https://bitbucket.org/brimock/lockstep-sample/overview)

### [英文原文链接](http://clintonbrennan.com/2013/12/lockstep-implementation-in-unity3d/)