---

layout:     post
title:      "Unity超级角色控制器 - Part 6"
subtitle:   "地形检测"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/super-mario-3d-world.jpg"

---

虽然此前写了5篇角色控制器的文章，但我只是简短地提到了地形检测。知道角色站在什么地形上是非常重要的话题，因为很多角色的行为往往都取决于所站的地形。做好地形检测，可以明显提升体验的流畅度。

![]({{ site.url }}/img/character-controller/mario_glitch.gif)

* 一个差的地形检测的例子。

因此我们想知道什么在角色脚底下呢？首先是距离脚下有多远。我们会想知道角色的脚是否贴着地面还是在半空中。我们还会想知道脚下地面具体位置坐标，这对于[上一章]({{ site.url }}/super-character-conroller-part6)强调过的钳住地面是很重要的。第三个就是会想知道脚下地形的法线是多少。而最后很有必要知道脚下的**GameObject**是谁，这使得我们可以为地形添加组件(在超级角色控制器当中，会有**SuperCollisionType**可以添加到地形当中来描述地形)。

看看之前研究[Unity物理API的文章]({{ site.url }}/super-character-conroller-part3)，找到最合适的解决方案是Physics.Raycast。我们可以通过向下打射线找到地面。虽然**RaycastHit**结构体可以让我们得到碰撞点，打击距离，打击法线以及发生碰撞的对象。第一眼看上去就完全满足了我们的需求，但是仔细想想还是会有一些问题。

我们的角色控制器是用一系列的球体来表示的，这意味着当直接站在平面上的时候，平面上的最近点就恰好距离底部球体球心一个半径距离。但是如果角色站在斜坡上就会出问题。如果我们现在还是像之前一样直接向下打射线，所得的点就不再是底部球体的最近点了。这对于钳住地面来讲会出错的(具体后面会谈到)。

![]({{ site.url }}/img/character-controller/raycast.png)

* 射线直接从底部球心直接向下打出。在角色站在斜坡的情况下，下面的点是不正确的。而当角色钳住地面的时候，那个点会让角色稍稍与斜坡交叉。对于陡峭的坡体来说，这个问题就更加明显了。

幸运的是，我们有救兵Physics.SphereCast。与其投射一条细微的射线出去，我们可以投射一个球体。这样就能解决上面提到的问题，从而确保角色总是完全在地表之上。

在使用SphereCast能够顺利工作的同时，也带来了一些问题。首先是SphereCast碰到碰撞体边缘的时候，hit.normal返回的是两个相邻面的插值。这与Vector3.Lerp函数来类似。

![]({{ site.url }}/img/character-controller/spherecast_normal.gif)

* 动画演示了SphereCast返回的hit.normal的插值情况。

我发现很有必要知道所站的表面法线的真实值，而不是插值。为了解决SphereCast带来的插值问题。我用单独的射线分别跟踪SphereCast所得到邻近的两个面，这样就能够得到正确的法线(在超级角色控制器的ProbeGround方法中，这个称为nearHit与farHit，分别表示距离控制器中心的最近的面与最远的面)。

SphereCast的下一个问题是我们用了它来做地形检测，但还是会有准确性的问题。我们已经默认了SphereCast打中的所有碰撞体都是地形，我们的角色可以站上去(或者在有的游戏中是滑上去)。但实际上这不是总能行得通的。我们可以有理有据地认为游戏世界的物理表面(角色与之碰撞的对象)可以划分为地形与墙体，而只有地形表面才应该被地形检测所检测出来。最简单的划分方法就是将表面法线与某个向量(比如世界空间的Vector3.up)的夹角来划分。小于90度的是地面，大于90度的是墙面。这样我们就可以确保不会将墙体也当做地面来处理。我们的地形检测里这个问题很天然的解决了：我们总是向下投射球体，这就是说这不可能打中90度的墙体。但是，游戏中的墙体法线会接近90度，甚至处于85到90之间。我们想把这些85度的表面看做是墙体，也就是说它们应该忽略SphereCast。

![]({{ site.url }}/img/character-controller/spherecastwallangle.png)

* 我们的角色控制器是上不去85度墙的。对于这个墙来说我们的SphereCast的碰撞点是黄色标记，而不是脚下方的平面。这会使得我们的角色会认为自己站在陡坡上，而不是平地上。

最常见的办法是用[Physics.SphereCastAll](http://docs.unity3d.com/ScriptReference/Physics.SphereCastAll.html)。它会理想地同时打中墙体与地面，然后我们可以遍历所有碰撞点，从而找到合适的立足点。不幸的是，SphereCastAll只能每个对象拣选一个碰撞点，也就是说如果墙体与地面是同一个对象，那么这个方案会出错。

对于上面的问题，我们可以通过减小SphereCast的半径来解决。这样确实能解决一些问题，但不是全部。我们还是要想一种可靠的方法来来处理贴着陡坡这个问题。

「PS: 在SuperCharacterController中，陡坡的定义是SuperCollisionType组件的StandAngle值定义的，可以放在所有与角色碰撞的碰撞体上。」

为了解决这个问题，可以想象一下我们脚底下某种地形，如果没有被陡坡影响，我们是能够通过SphereCast检测出来的。为了找到这个地面，我们可以在SphereCast打中的地方用Raycast。这是主要用于检验是否存在这块地面，而且可以获取它的法线。

![]({{ site.url }}/img/character-controller/raycastdownslope.png)

* 最初SphereCast的碰撞点在黄色标记位置。因为我们与陡坡有碰撞，我们可以Raycast(红色)来检测陡坡之下是否有一块地面在我们之下(紫色)。

这让我们知道下面有什么，但是由于用了Raycast，而不是SphereCast，我们会又一次遇到之前的问题，碰撞点会让地形与球体重合。给定我们有的信息(脚下地形的法线)，我们可以将其转换为近似于SphereCast的数据吗？答案是Yes。

每当你从控制器底部向下SphereCast，然后碰到了一个表面，就会就在碰撞表面法线与碰撞点之间存在一种联系。在我们从斜坡碰撞点向下打射线得到正确地面的法线之后，我们的任务就是找到控制器底部向下SphereCast与地面的交点。下面我们看看二维的情况，然后再回来解决三维问题。

![]({{ site.url }}/img/character-controller/spherecast_point.gif)

* 动画演示了SphereCast的碰撞点与碰撞平面法线的关系。SphereCast的原点在黄色球那里，碰撞在红色球那里，碰撞点标记为蓝色。注意到随着斜坡越来越斜，红色圆形就越往上。

由于我们想找一个二维空间的点，而这两个值分别是x和y。如果计算正确，x和y会将我们圆形的往下与地面碰撞的点。对应上面的图来看，我们要用给定的地面法线，找到上图中蓝色交点的位置。

幸运的是，这其实并不困难。复习一下中学的数学知识，通过法线作为角度传入，我们可以使用三角函数来计算x和y的位置。

{% highlight C# %}

x = Mathf.Sin(groundAngle);
y = Mathf.Cos(groundAngle);

{% endhighlight %}

要注意的是，Unity的Sine和Cosine需要以弧度的方式传入夹角，需要转换一下。

![]({{ site.url }}/img/character-controller/sinandcosine1.png)

* 通过sine与cosine计算碰撞点的位置

现在我么可以用这些值来调整控制器的位置(乘上半径)。

「PS: 在SuperCharacterController中，近似计算SphereCast值的方法叫SimulateSphereCast。」

以上就是超级角色控制器中用到所有地形检测技术。不像之前的文章，这里地形检测话题是很开放的--上面只是其中的一种，但我法线它在实际中运行的很好。


