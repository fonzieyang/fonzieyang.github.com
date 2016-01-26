---

layout:     post
title:      "Unity超级角色控制器 - Part 2"
subtitle:   "具体实现"
categories: 游戏开发
tags: [Unity3D, GamePlay, tag]
header-img: "img/super-mario-3d-world.jpg"

---

现在我们了解了角色控制器的基本碰撞处理，接下来马上就演示如何在Unity中实现上一章所展示的效果。

在开始以前，先确保你的Unity是否已经完成下载安装。这篇文章中所使用的版本是Unity 4.3.4f1。(检查Unity版本的方法是Help->About Unity)打开一个现有的工程或者创建一个新的来开始这篇教程。创建一个新的场景(Scene)，然后创建一个立方体(Cube)和一个球体(Sphere)。虽然我们最终会用胶囊体作为我们的控制器形状，但是刚开始还是让事情保持简单一些。将球体命名为Player，立方体命名为Wall。改变墙体每个轴的缩放到6。为了更加形象，我还给Player加了蓝色的材质，给Wall加了绿色的材质。将Player上的Sphere Collider组件移除掉。

![]({{ site.url }}/img/character-controller/screen1.jpg)

创建新的C#脚本，然后命名为SuperCharacterController.cs。为了表示我们的角色，拷贝和粘贴一下脚本，然后挂到Player身上：

{% highlight C# %}
using UnityEngine;
using System;
using System.Collections.Generic;
 
public class SuperCharacterController : MonoBehaviour {
	 
	[SerializeField]
	float radius = 0.5f;
	 
	private bool contact;
	 
	// Update is called once per frame
	void Update () {
		 
		contact = false;
		 
		foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
		{
			Vector3 contactPoint = col.ClosestPointOnBounds(transform.position);
			 
			Vector3 v = transform.position - contactPoint;
			 
			transform.position += Vector3.ClampMagnitude(v, Mathf.Clamp(radius - v.magnitude, 0, radius));
			 
			contact = true;
		}
	}
	 
	void OnDrawGizmos()
	{
		Gizmos.color = contact ? Color.cyan : Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
{% endhighlight %}

然后就完了。运行项目并且打开场景。将Player往墙体的边缘慢慢拖过去。你可以看到墙体在反推，让Player总是停留在边缘。那么这里做了什么呢？

Physics.OverlapSphere返回了与球体发生碰撞的一组Collider。这是个很好用的函数，参数很简单。只需要传入球心与半径就可以了。

一旦检测到任何碰撞，我们就会开始处理。为了找到box collider上的最近点，我们用了ClosestPointOnBounds函数。紧接着我们就可以通过contactPoint得到我们的位置。contactPoint的长度就是我们所需要推出去的距离。

你可能会注意到，我实现了OnDrawGizmos函数，这样OverlapSphere的碰撞就一清二楚了。

![]({{ site.url }}/img/character-controller/screen2.jpg)

* 两帧演示了碰撞被检测，而后被处理

相当简单。但是我们至今为止的胜利可能只是个开始。创建一个DebugDraw.cs的类，然后添加如下代码。

{% highlight C# %}
using UnityEngine;
using System.Collections;
 
public static class DebugDraw {
	 
	public static void DrawMarker(Vector3 position, float size, Color color, float duration, bool depthTest = true)
	{
		Vector3 line1PosA = position + Vector3.up * size * 0.5f
		Vector3 line1PosB = position - Vector3.up * size * 0.5f;
		 
		Vector3 line2PosA = position + Vector3.right * size * 0.5f;
		Vector3 line2PosB = position - Vector3.right * size * 0.5f;
		 
		Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
		Vector3 line3PosB = position - Vector3.forward * size * 0.5f;
		 
		Debug.DrawLine(line1PosA, line1PosB, color, duration, depthTest);
		Debug.DrawLine(line2PosA, line2PosB, color, duration, depthTest);
		Debug.DrawLine(line3PosA, line3PosB, color, duration, depthTest);
	}
}
{% endhighlight %}

这是一个我写的挺有用的帮助函数，它可以让我们在任何地方绘制在编辑器中(与此相对的，我们只能在OnDrawGizmos函数中绘制)。修改foreach循环如下。

{% highlight C# %}
foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
{
	Vector3 contactPoint = col.ClosestPointOnBounds(transform.position);
	 
	DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
	 
	Vector3 v = transform.position - contactPoint;
	 
	transform.position += Vector3.ClampMagnitude(v, Mathf.Clamp(radius - v.magnitude, 0, radius));
	 
	contact = true;
}
{% endhighlight %}

运行代码，你会注意到当碰撞发生的时候，会在位置上绘制一个红色十字标记。现在，拖动player到墙体内，就能看到标记跟随者player。这对于ClosestPointOnBounds函数来说也不完全是个错误，但是如果要对应上上回提到的退回策略，我们真的希望有一个ClosestPointOnSurfaceOfBoundsOrSomething函数。

![]({{ site.url }}/img/character-controller/screen7.jpg)

问题就在于当我们的角色在碰撞体内部的时候，随着返回最近点函数失效，没法正确地处理碰撞。现在，我们就来处理这个问题。

将我们的墙体在y轴上旋转大概20度，然后运行场景。你回发现一切都不正常了。这是因为ClosestPointOnBounds函数返回的最近点实在轴对齐包围盒(AABB)上，而不是朝向包围盒(OBB)上。

![]({{ site.url }}/img/character-controller/screen3.jpg)

你可能已经在想如果这个问题扩展一下，不仅仅是盒子会是怎样。由于函数只能返回轴对齐包围盒的最近点，哪怕是其他类型的碰撞，它也是肯定没法得到表面的最近点的。因此这个问题是没有银弹的(也许是我没发现)，我们只能每个碰撞类型自己实现。

先让我们从最简单的开始：球体碰撞。在场景中创建一个新的球体游戏对象。找到表面上的最近点需要好几步，每一步都比较简单。要知道哪个方向推出玩家，我们计算从我们为之到球体中心的方向。由于球体表面每个点距离球心都一样，我们只要正规化我们的向量，然后乘以半径以及local scale因子即可。

下面是代码实现。你可以看到新的方法中，多加了一个检测当前OverlapSphere的碰撞类型。

{% highlight C# %}
using UnityEngine;
using System;
using System.Collections.Generic;
 
public class SuperCharacterController : MonoBehaviour {
	 
	[SerializeField]
	float radius = 0.5f;
	 
	private bool contact;
	 
	// Update is called once per frame
	void Update () {
		 
		contact = false;
		 
		foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
		{
			Vector3 contactPoint = Vector3.zero;
			 
			if (col is BoxCollider)
			{
				contactPoint = col.ClosestPointOnBounds(transform.position);
			}
			else if (col is SphereCollider)
			{
				contactPoint = ClosestPointOn((SphereCollider)col, transform.position);
			}
			 
			DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
			 
			Vector3 v = transform.position - contactPoint;
			 
			transform.position += Vector3.ClampMagnitude(v, Mathf.Clamp(radius - v.magnitude, 0, radius));
			 
			contact = true;
		}
	}
	 
	Vector3 ClosestPointOn(SphereCollider collider, Vector3 to)
	{
		Vector3 p;
		 
		p = to - collider.transform.position;
		p.Normalize();
		 
		p *= collider.radius * collider.transform.localScale.x;
		p += collider.transform.position;
		 
		return p;
	}
	 
	void OnDrawGizmos()
	{
		Gizmos.color = contact ? Color.cyan : Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
{% endhighlight %}

机智的读者可能会发现ClosestPointOn实际上返回的是球体表面的最近点。不像ClosestPointOnBounds返回的是包围盒的最近点。这很简单，但是在使用之前还有一些问题要解决。现在来看看第二种(也是今天的最后一种)碰撞类型的实现：朝向包围盒。

![]({{ site.url }}/img/character-controller/screen5.jpg)

* 图像演示了如何通过球心与控制器位置之间的向量推算出最近点

我们的一般做法是获取输入，然后clamp到box内部。这样的效果与内建的ClosestPointOnBounds是一样的，除了即使box带旋转也能处理之外。

Box Collider的扩展定义了局部大小x、y和z。为了将我们的点clamp到Box Collider内部，我们需要将作为从世界坐标系转换到Box Collider的局部坐标系。在完成之后，我们对位置clamp到包围盒内即可。最后，我们再将改点转换回世界坐标系。代码如下。

{% highlight C# %}
using UnityEngine;
using System;
using System.Collections.Generic;
 
public class SuperCharacterController : MonoBehaviour {
 
	[SerializeField]
	float radius = 0.5f;
	 
	private bool contact;
	 
	// Update is called once per frame
	void Update () {
		 
		contact = false;
		 
		foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
		{
			Vector3 contactPoint = Vector3.zero;
			 
			if (col is BoxCollider)
			{
				contactPoint = ClosestPointOn((BoxCollider)col, transform.position);
			}
			else if (col is SphereCollider)
			{
				contactPoint = ClosestPointOn((SphereCollider)col, transform.position);
			}
			 
			DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
			 
			Vector3 v = transform.position - contactPoint;
			 
			transform.position += Vector3.ClampMagnitude(v, Mathf.Clamp(radius - v.magnitude, 0, radius));
			 
			contact = true;
		}
	}
	 
	Vector3 ClosestPointOn(BoxCollider collider, Vector3 to)
	{
		if (collider.transform.rotation == Quaternion.identity)
		{
			return collider.ClosestPointOnBounds(to);
		}
		 
		return closestPointOnOBB(collider, to);
	}
	 
	Vector3 ClosestPointOn(SphereCollider collider, Vector3 to)
	{
		Vector3 p;
		 
		p = to - collider.transform.position;
		p.Normalize();
		 
		p *= collider.radius * collider.transform.localScale.x;
		p += collider.transform.position;
		 
		return p;
	}
	 
	Vector3 closestPointOnOBB(BoxCollider collider, Vector3 to)
	{
		// Cache the collider transform
		var ct = collider.transform;
		 
		// Firstly, transform the point into the space of the collider
		var local = ct.InverseTransformPoint(to);
		 
		// Now, shift it to be in the center of the box
		local -= collider.center;
		 
		// Inverse scale it by the colliders scale
		var localNorm =
		new Vector3(
		Mathf.Clamp(local.x, -collider.size.x * 0.5f, collider.size.x * 0.5f),
		Mathf.Clamp(local.y, -collider.size.y * 0.5f, collider.size.y * 0.5f),
		Mathf.Clamp(local.z, -collider.size.z * 0.5f, collider.size.z * 0.5f)
		);
		 
		// Now we undo our transformations
		localNorm += collider.center;
		 
		// Return resulting point
		return ct.TransformPoint(localNorm);
	}
	 
	void OnDrawGizmos()
	{
		Gizmos.color = contact ? Color.cyan : Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
{% endhighlight %}

你可能会注意到在主碰撞循环中做了一些修改，使得我们不管是轴对齐还是朝向的用ClosesPointOn就可以了。这里的大部分实现都参考自fholm的RPGController package。

![]({{ site.url }}/img/character-controller/screen6.jpg)

到这里我们第一部分的实现就结束了。在后面的文章中，我会讲讲Unity物理API会遇到的一些问题。然后开始为实现理想中的角色控制器开发一些组件。

### 参考

这篇文章主要的代码参考自fholm的[RPGController package](https://github.com/fholm/unityassets)。其中的推出来自RPGMotor.cs，最近点来自RPGCollisions.cs。

