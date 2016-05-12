---

layout:     post
title:      "Unity黑与白"
subtitle:   "Anything awsome have dark side"
categories: 心得
tags: [心得, tag]
header-img: "img/minecraft.jpg"

---

1. 深入使用Unity会发现几乎一切都不可靠，尽可能轻度使用Unity的功能，尽量用常规方式做游戏
	
	就跟Linux极少使用保护模式提供的各种分段类似，只在关键的任务上依赖X86，这样遇到的坑也最少。从而让系统更加简洁，不被提供的接口导致误入歧途。
	涉及的模块包括但不限于：场景、Prefab、PlayerPref、Log、CharacterController、序列化、脚本。
	存储方式尽可能用自己的文件格式，比如XML
	脚本使用Lua替代做到热更新
	只要用得简单，后面更加复杂的assetbundle、内存优化、异步加载等等都更容易组合

2. 尽可能利用好Unity的编辑器+自定义数据存储

3. 做好检查，代码单元测试，为美术资源、游戏配置，写好检查工具

4. 底层代码尽量简单，复杂度随着层级提升，把复杂暴露给上层换取稳定，最好是无状态的

5. 给问题定位一个便捷的方式

6. 用UnityServer提升发布速度，小心32位Unity4内存不足问题
7. 设定好目标机型与美术规格与流程
8. 利用好白膜制作原型，千万不要把美术文件作为根节点，让每个场景都单独可用