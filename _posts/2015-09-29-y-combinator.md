---

layout:     post
title:      "浅谈Y组合子"
subtitle:   ""
categories: 计算机科学
tags: [Lambda, tag]
header-img: "img/WQtuk.jpg"

---

*这篇文章希望能够通俗地讲清楚Y组合子，如果对lambda演算感兴趣的同学可以看看最后的相关资料*

在lambda中，如果我们想要递归，以斐波那契数列为例，可以这样：

{% highlight scheme %}
let power = lambda n. IF_Else n==0 1 n*power(n-1)
{% endhighlight %}

然而，在"纯"lambda演算中，是没有let关键字的。我们需要换个方法，如果直接的不行，那么我们可以尝试间接的。很容易能想到通过参数把自己传给自己：

{% highlight scheme %}
let P = lambda self n. If_Else n==0 1 n*self(self, n-1)
P(P, 3)
{% endhighlight %}

这么做就显得很不优雅，我们要想一个办法，能够通用的把自己传给自己。就像上面一样。我们试着构造一下，把斐波那契数列的逻辑替换为任意函数：

{% highlight scheme %}
let gen = lambda self. AnyFunction(self(self))
gen(gen)
{% endhighlight %}

尝试写出斐波那契数列的AnyFunction实现：

{% highlight scheme %}
let AnyFunction = lambda self n. If_Else n==0 1 n*self(n-1)
{% endhighlight %}

经过展开之后，发现任何函数只要在AnyFunction那个位置，经过上面的代码之后，都能够实现递归。

其中gen(gen)展开如下：

{% highlight scheme %}
gen(gen) => AnyFunction(gen(gen))
{% endhighlight %}

可能你会疑问，gen(gen)为什么能够表达自己呢？因为gen(gen)=>AnyFunction(gen(gen))，它能够返回AnyFunction自身，这就得到自己了。并且这时会把这个gen(gen)再传给AnyFunction。而gen(gen)不到求值的时候都不展开，因此gen(gen)没有被调用时，没有任何作用，但是一旦AnyFunction内部调用了传进来的自身，那么就进行求值得到。通俗来讲，***与其说gen(gen)是自身，还不如说这是一个把能够得到自己，并且把gen(gen)再次传入的函数。***

在理解这个机制之后，通用的递归函数已经到手。封装一下就轻而易举了，这就是传说中的Y组合子：

{% highlight scheme %}
let Y = lambda f. 
	let gen = lambda self. f(self(self))
	gen(gen)
{% endhighlight %}

再把let去掉可得到Y的定义：

{% highlight scheme %}
lambda f. (lambda x. (f(x x)) lambda x. (f(x x)))
{% endhighlight %}

接下来可以试着使用一下：

{% highlight scheme %}
( ( lambda f. (lambda x. (f(x x)) lambda x. (f(x x))) )
  ( lambda f. lambda n. n==0 ? 1 : n*(f n-1) )
) 4
{% endhighlight %}

看，完美！证明了lambda只需要alpha/beta/eta三条规则而不需要命名。

---

### 相关资料，从易到难排序


* [g9的lambda calculus系列](http://blog.csdn.net/g9yuayon/) `有很多lambda的入门讲解，幽默风趣`
* [The Little Schemer](http://book.douban.com/subject/1632977/) `手把手学lambda`
* [The Seasoned Schemer](http://book.douban.com/subject/1726083/) `手把手2`
* [SICP](http://book.douban.com/subject/1451622/) `不用多说，看书评`
* [MIT讲SICP](http://groups.csail.mit.edu/mac/classes/6.001/abelson-sussman-lectures/) `MIT的课，值得一看`

### 其他相关资料

* [康托尔、哥德尔、图灵——永恒的金色对角线](http://mindhacks.cn/2006/10/15/cantor-godel-turing-an-eternal-golden-diagonal/) `讲了图灵机的起源--对角线法`
* [对角线方法之后的故事](http://www.matrix67.com/blog/archives/4812) `关于对角线法的误用`
* [计算的本质](http://book.douban.com/subject/26148763/) `手把手用Ruby讲图灵机，比较有趣，通俗易懂`
* [图灵的秘密](http://book.douban.com/subject/10779604/) `通俗易懂，引用图灵论文，有理有据。图灵机纸条部分比较枯燥` 
