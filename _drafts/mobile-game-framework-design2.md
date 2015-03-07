---

layout:     post
title:      "手游框架设计&lt;一&gt;"
subtitle:   "过去两年的手游框架设计总结"
categories: 游戏开发
tags: [框架, GamePlay, tag]
header-img: "img/lf2.jpg"

---

## 扩展引擎

手游框架设计<零>中我们讲了整体的框架架构，这里开始，我们自底向上地浅析一下各个模块。

![]({{ site.baseurl }}/img/framework-design/engine-layer2.jpg)

引擎层有很多可写，真要写得话绝对远远不是一篇博客可以讲完的。而且《手游框架》系列以(GamePlay)业务开发为主题，引擎层距离系列文章的主题太远，所以这里简单带过，以(hou)资(yan)源(wu)引(chi)用为主。

<br/>

---

<br/>

这里推荐一个引擎开发百科全书**[《游戏引擎架构》](http://book.douban.com/subject/25815142/)**，方方面面都讲得很到位，顽皮狗工作室大神写的，翻译也是大神，国外已经出第二版了。

<a href="http://book.douban.com/subject/25815142/">![](http://img3.douban.com/lpic/s27215120.jpg)</a>

<br/>

---

<br/>

引擎开发主要就是渲染了，对图形学感兴趣则可以看**[《游戏程序员养成计划》](http://www.cnblogs.com/clayman/archive/2009/05/17/1459001.html)**

<a href="http://book.douban.com/subject/3213439/">![](http://img3.douban.com/lpic/s4551492.jpg)</a>


<br/>

---

<br/>



觉得上面太难了，可以看这本我正在翻译的**[《Game Programming Algorithms and Techniques》](http://book.douban.com/subject/25779461/)**，一本挺不错的入门书籍，比如第一章就阐明了游戏程序最核心最本质的部分就是：**游戏循环**、**游戏时间管理**和**游戏对象模型**。

<a href="http://book.douban.com/subject/25779461/">![](http://img3.douban.com/lpic/s27158014.jpg)</a>

<br/>

---

<br/>

对于**刚入门的新人**，可以看看这份[代码](https://github.com/fonzieyang/BalanceBall)。是我大学期间一个晚上帮同学做出来的毕业设计。主要看Framework部分，应该是这个世界上最简单的Sprite实现了吧。




