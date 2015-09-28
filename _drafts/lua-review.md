---

layout:     post
title:      "重读Lua源码"
subtitle:   "Lua精巧的设计，来源于多年的打磨"
categories: 心得
tags: [心得, tag]
header-img: "img/unity-girl4.jpg"

---

用了Lua有些年头了，源码也简单读过2、3遍。
这里简单写一下Lua的机制，做个汇总。
为以后做Unity框架做铺垫。

## 模块化

Lua通过require(modname)加载指定模块进来，Lua会先检查package.loaded[modname]，如果pack	age.loaded[modname]存在，那么直接返回，如果不存在，

## Binding

```sequence
a->b: hi
b->c: go
```

## VM

## 开发

## 总结

