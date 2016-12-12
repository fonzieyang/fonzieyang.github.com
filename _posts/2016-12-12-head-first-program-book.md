---

layout:     post
title:      "最简编程基本功指南"
subtitle:   "那些年追过的CS书本"
categories: 心得
tags: [心得, tag]
header-img: "img/lena_title.jpg"

---


最近鹅厂技术专家miloyip大牛发布了[游戏程序员学习指南](https://miloyip.github.io/game-programmer/game-programmer.pdf)(我的译著[《游戏编程算法与技巧》](https://book.douban.com/subject/26906838/)原版也在其中:>)，除了For Kid系列很多都看过，确实都是好书，深有同感，看着封面就有亲切感。

但我觉得这条道路对于国内还没进入状态的小白来讲，门槛太高，光是英语就卡掉一批人。特别是前些天的[游戏蛮牛在线答疑](http://www.manew.com/thread-97988-1-1.html)里，发现很多人都不知道如何进一步学习。

在这里抛砖引玉，放一些我认为能够轻松阅读并且不错的中文教材的教材。如果某本书不那么容易消化，也会有标记，请放心进食。

这里的书对于CS专业的同学都非常适用，不限于游戏行业，欢迎补充~

PS: 作为程序员还是要能够阅读英语材料，可以从翻译海外博文&看美剧开始练习

# 学习路线

整理分为四大块：**编程语言**(能写代码)->**程序设计**(能写好代码)->**计算机底层**(能做别人做不到)->**计算机理论**(能做别人想不到)

不过以我的经历，实际过程中会螺旋上升，编程语言->程序设计->计算机底层->计算机理论->编程语言->...，因为一下啃完一整块是不现实的。

PS: 在最后追加了一个数学

---

## 编程语言

编程语言系列C是必学的，然后可以根据**编程范式**和**语言类型**有目的地去学习，比如面向对象C#、函数式lisp、逻辑式编程prolog等等。学完编译型还可以选择一门脚本型语言，比如Lua。

**其中C++系列的书都不会太简单，坑非常非常深，慎入。**

这里没提太多，因为很多用到就搜高分著作，搜到就学，也没太多特别好的书，**基本都当做工具书使用**。

* C语言

![C程序设计语言 : 第 2 版·新版](https://img3.doubanio.com/mpic/s1106934.jpg)

* C++

![C++ Primer 中文版（第 4 版）](https://img3.doubanio.com/mpic/s1638975.jpg)

![深度探索C++对象模型](https://img3.doubanio.com/mpic/s3301634.jpg)

![C++反汇编与逆向分析技术揭秘](https://img3.doubanio.com/mpic/s6940605.jpg)

![Effective C++中文版](https://img3.doubanio.com/mpic/s1441361.jpg)

![C++设计新思维 : 泛型编程与设计模式之应用](https://img3.doubanio.com/mpic/s1617593.jpg)

![C++语言的设计和演化](https://img3.doubanio.com/mpic/s24436633.jpg)

---


## 程序设计

如果过了第一个坎，那么恭喜，这里都比较轻松，比较偏向感性哲理一些，**这里需要大量练习与反思**。

主要看前两本就可以了，因为程序设计不是理论有多难多深，而是要通过多练习，将它们运用起来，内化起来，才能掌握的。另外学完之后还要了解一下UML和领域设计。

另外第三本作者松本行弘的一句话**“程序设计就是语言设计”**是影响我程序设计思路最深的，感兴趣可以看看。

![深入浅出设计模式（影印版）](https://img3.doubanio.com/mpic/s2414323.jpg)

![冒号课堂 : 编程范式与OOP思想](https://img1.doubanio.com/mpic/s3976237.jpg)

![代码的未来 : 代码的未来](https://img5.doubanio.com/mpic/s26393136.jpg)

![UML精粹 : 标准对象建模语言简明指南](https://img3.doubanio.com/mpic/s11335615.jpg)

![代码大全（第2版）](https://img1.doubanio.com/mpic/s1495029.jpg)

![程序员修炼之道 : 从小工到专家](https://img5.doubanio.com/mpic/s4646956.jpg)

![程序员的思维修炼 : 开发认知潜能的九堂课](https://img1.doubanio.com/mpic/s4548399.jpg)

![卓有成效的程序员 : 一本揭示高效程序员的思考模式，一本告诉你如何缩短你与优秀程序员的](https://img1.doubanio.com/mpic/s3668809.jpg)

![禅与摩托车维修艺术](https://img5.doubanio.com/mpic/s6927676.jpg)

![编程珠玑 : 第2版](https://img3.doubanio.com/mpic/s4687321.jpg)

---

## 计算机底层

这里属于好奇者的天堂，功利者的地狱。看完这里，过去很多迷惑都会消失，了解很多**计算机系统是如何搭建**起来的。

除了《深入理解计算机系统》(俗称csapp)之外，其他都是可以躺在沙发上就能轻松看完的好书（误）。

最好的汇编教材，没有之一，感谢王爽老师。我看的时候还是第一版

![汇编语言(第3版)](https://img3.doubanio.com/mpic/s28015785.jpg)

从手电筒开始，教你构建cpu，真正的深入浅出，作者非常牛逼

![编码的奥秘](https://img3.doubanio.com/mpic/s26535990.jpg)

语言生动有趣，对操作系统有个感性的认知

![计算机的心智 : 操作系统之哲学原理](https://img3.doubanio.com/mpic/s3742943.jpg)

简单了解Linux与Linux源码

![Linux体系与编程](https://img3.doubanio.com/mpic/s6169371.jpg)


手把手实现操作系统

![Orange'S:一个操作系统的实现](https://img3.doubanio.com/mpic/s3788445.jpg)


手把手实现编译器

![编程语言实现模式](https://img5.doubanio.com/mpic/s7661036.jpg)


把程序启动的过程拆解给你看

![程序员的自我修养 : 链接、装载与库](https://img3.doubanio.com/mpic/s3724604.jpg)

比TCP三卷要轻松易读很多，不会陷入到细节里面

![图解TCP/IP : 第5版](https://img1.doubanio.com/mpic/s26676928.jpg)

通过精心安排的案例教你怎么组建网络

![深入浅出Networking](https://img1.doubanio.com/mpic/s7646877.jpg)

教你怎么组建更加复杂的网络

![奠基 : 计算机网络](https://img1.doubanio.com/mpic/s7644349.jpg)

建立计算机系统的大局观

![深入理解计算机系统](https://img3.doubanio.com/mpic/s1470003.jpg)

---

## 计算机理论

可能对日常工作帮助不大，但了解完这块之后，能够了解很多事情的前因后果。

这块理论都不好啃，但这些书都已经非常平易近人、生动有趣。

* 可计算性

类似于科普著作，讲计算机诞生的故事。

![逻辑的引擎](https://img5.doubanio.com/mpic/s2912176.jpg)


这本书讲到了计算的本质，对比另外一本书《计算的本质》，你就知道这本书有多好。

![图灵的秘密 : 他的生平、思想及论文解读](https://img3.doubanio.com/mpic/s23127964.jpg)

刘未鹏的博文：[康托尔、哥德尔、图灵——永恒的金色对角线(rev#2)](http://mindhacks.cn/2006/10/15/cantor-godel-turing-an-eternal-golden-diagonal/)

我的另外一篇博文：[浅谈Y组合子](http://jjyy.guru/y-combinator)

* 函数式编程

对话体课本，边学边练，轻松愉快掌握lisp。
只有英文版，不过非常简单。
完全颠覆对递归的认识，并且能让你平时写代码更加优雅简洁。

![The Little Schemer - 4th Edition](https://img5.doubanio.com/mpic/s29105516.jpg)

![The Seasoned Schemer](https://img3.doubanio.com/mpic/s11709222.jpg)

![The Reasoned Schemer](https://img1.doubanio.com/mpic/s2552408.jpg)

* 算法

![算法导论（原书第3版）](https://img3.doubanio.com/mpic/s25648004.jpg)

![算法竞赛入门经典](https://img1.doubanio.com/mpic/s6092408.jpg)

* 信息论

没看到特别适合入门的书:P，有合适请告知

---

## 数学

这块也是不好啃，所以深入浅出的书非常少。

一定要做习题！

一定要做习题！

一定要做习题！

* 线性代数

在游戏引擎里面学线性代数特别容易，可以拿个Unity对着写就好了

![3D数学基础 : 图形与游戏开发](https://img3.doubanio.com/mpic/s1986494.jpg)

* 统计学

应用数学之王，必学的分支，并且对未来人工智能的学习有帮助

![深入浅出统计学](https://img1.doubanio.com/mpic/s7014029.jpg)

* 微积分

微积分是数学的主干，是必定要掌握的。尹逊波老师的《工科数学分析》课程是我上过最好的数学课，感谢中国大学MOOC和网易。

![]({{ site.url }}/img/yixunbo.png)






