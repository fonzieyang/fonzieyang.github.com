---

layout:     post
title:      "「译」Unity3D中实现帧同步 - Part 2"
subtitle:   ""
categories: 游戏开发
tags: [Unity3d, tag]
header-img: "img/unity-girl4.jpg"

---


# Unity3D中实现帧同步 - Part 2

### 概览

在上一次实现的帧同步模型当中，游戏帧率和通信回合(也就是帧同步长度)长度是设置为固定间隔。但是实际上，延迟和性能都是不同的。update的原始版本会跟踪两个变量。第一个是于另一个玩家通信的时长。第二个则是游戏帧方法的性能时长。

### 移动平均数

为了处理延迟上的波动，我们想快速增加帧同步回合的时长，同时也想在低延迟的时候减少。如果游戏更新的节奏能够根据延迟的测量结果调节，而不是固定值的话，会使得游戏玩起来更加顺畅。我们可以累加所有的过去信息得到"移动平均数"，然后根据它作为调节的权重。

每当一个新值大于平均数，我们会设置平均数为新值。这会得到快速增加延迟的行为。当值小于当前平均值，我们会通过权重相信该值，我们有以下公式：

\\[
newAverage = currentAverage * (1 – w) + newValue * ( w)
\\]
其中0<w<1

在我的实现中，我设置w=0.1。而且还会跟踪每个玩家的平均数，而且总是使用所有玩家当中的最大值。这里是一些增加新值的方法：

~~~javascript
public void Add(int newValue, int playerID) {
    if(newValue > playerAverages[playerID]) {
         //rise quickly
         playerAverages[playerID] = newValue;
    } else {
        //slowly fall down
        playerAverages[playerID] = (playerAverages[playerID] * (9) + newValue * (1)) / 10;
    }
}
~~~

为了维护计算结果的确定性，计算只使用整数。因此公式调整如下：

\\[
newAverage = (currentAverage * (10 – w) + newValue * ( w)) / 10
\\]
其中0<w<1

而在我的例子中，w=1。

### 运行时间平均数

每次游戏帧更新的时间是由运行时间平均数决定的。如果游戏帧开始要更长时间，那么我们需要降低每次帧同步回合更新游戏帧的次数。另一方面，如果游戏帧执行得更快了，每次帧同步回合可以更新游戏帧的次数也多了。对于每次帧同步回合，最长的游戏帧会被添加到平均数中。每次帧同步回合的第一次游戏帧都包含了处理动作的时间。这里使用Stopwatch来计算流逝的时间。

~~~javascript
private void ProcessActions() {
    //process action should be considered in runtime performance
    gameTurnSW.Start ();
 
    ...
 
    //finished processing actions for this turn, stop the stopwatch
    gameTurnSW.Stop ();
}
 
private void GameFrameTurn() {
   ...
         
    //start the stop watch to determine game frame runtime performance
    gameTurnSW.Start();
 
    //update game
    ...
 
    GameFrame++;
    if(GameFrame == GameFramesPerLockstepTurn) {
        GameFrame = 0;
    }
 
    //stop the stop watch, the gameframe turn is over
    gameTurnSW.Stop ();
    //update only if it's larger - we will use the game frame that took the longest in this lockstep turn
    long runtime = Convert.ToInt32 ((Time.deltaTime * 1000))/*deltaTime is in secounds, convert to milliseconds*/ + gameTurnSW.ElapsedMilliseconds;
    if(runtime > currentGameFrameRuntime) {
        currentGameFrameRuntime = runtime;
    }
 
    //clear for the next frame
    gameTurnSW.Reset();
}
~~~

注意到我们也用到了Time.deltaTime。使用这个可能会在游戏以固定帧率执行的情况下与上一帧时间重叠。但是，我们需要用到它，这使得Unity为我们所做的渲染以及其他事情都是可测量的。这个重叠是可接受的，因为只是需要更大的缓冲区而已。

### 网络平均数

使用什么作为网络平均数在这里不太清晰。我最终使用了Stopwatch计算从玩家发送数据包到玩家确认动作的时间。这个帧同步模型发送的动作会在未来两个回合中执行。为了结束帧同步回合，我们需要所有玩家都确认了这个动作。在这之后，我们可能会有两个动作等待对方确认。为了解决这个问题，用到了两个Stopwatch。一个用于当前动作，另一个用于上一个动作。这被封装在ConfirmActions类当中。当帧同步回合往下走，上一个动作的Stopwatch会成为这一个动作的Stopwatch，而旧的"当前动作Stopwatch"会被复用作为新的"上一个动作Stopwatch"。

~~~javascript
public class ConfirmedActions
{
...
    public void NextTurn() {
        ...
        Stopwatch swapSW = priorSW;
             
        //last turns actions is now this turns prior actions
        ...
        priorSW = currentSW;
         
        //set this turns confirmation actions to the empty array
        ...
        currentSW = swapSW;
        currentSW.Reset ();
    }
}
~~~

每当有确认进来，我们会确认我们接收了所有的确认，如果接收到了，那么就暂停Stopwatch。

~~~javascript
public void ConfirmAction(int confirmingPlayerID, int currentLockStepTurn, int confirmedActionLockStepTurn) {
    if(confirmedActionLockStepTurn == currentLockStepTurn) {
        //if current turn, add to the current Turn Confirmation
        confirmedCurrent[confirmingPlayerID] = true;
        confirmedCurrentCount++;
        //if we recieved the last confirmation, stop timer
        //this gives us the length of the longest roundtrip message
        if(confirmedCurrentCount == lsm.numberOfPlayers) {
            currentSW.Stop ();
        }
    } else if(confirmedActionLockStepTurn == currentLockStepTurn -1) {
        //if confirmation for prior turn, add to the prior turn confirmation
        confirmedPrior[confirmingPlayerID] = true;
        confirmedPriorCount++;
        //if we recieved the last confirmation, stop timer
        //this gives us the length of the longest roundtrip message
        if(confirmedPriorCount == lsm.numberOfPlayers) {
            priorSW.Stop ();
        }
    } else {
        //TODO: Error Handling
        log.Debug ("WARNING!!!! Unexpected lockstepID Confirmed : " + confirmedActionLockStepTurn + " from player: " + confirmingPlayerID);
    }
}
~~~

### 发送平均数

为了让一个客户端向其他客户端发送平均数，Action接口修改为一个有两个字段的抽象类。

~~~javascript
[Serializable]
public abstract class Action
{
    public int NetworkAverage { get; set; }
    public int RuntimeAverage { get; set; }
 
    public virtual void ProcessAction() {}
}
~~~

每当处理动作，这些数字会加到运行平均数。然后帧同步回合以及游戏帧回合开始更新

~~~javascript
private void UpdateGameFrameRate() {
    //log.Debug ("Runtime Average is " + runtimeAverage.GetMax ());
    //log.Debug ("Network Average is " + networkAverage.GetMax ());
    LockstepTurnLength = (networkAverage.GetMax () * 2/*two round trips*/) + 1/*minimum of 1 ms*/;
    GameFrameTurnLength = runtimeAverage.GetMax ();
 
    //lockstep turn has to be at least as long as one game frame
    if(GameFrameTurnLength > LockstepTurnLength) {
        LockstepTurnLength = GameFrameTurnLength;
    }
 
    GameFramesPerLockstepTurn = LockstepTurnLength / GameFrameTurnLength;
    //if gameframe turn length does not evenly divide the lockstep turn, there is extra time left after the last
    //game frame. Add one to the game frame turn length so it will consume it and recalculate the Lockstep turn length
    if(LockstepTurnLength % GameFrameTurnLength > 0) {
        GameFrameTurnLength++;
        LockstepTurnLength = GameFramesPerLockstepTurn * GameFrameTurnLength;
    }
 
    LockstepsPerSecond = (1000 / LockstepTurnLength);
    if(LockstepsPerSecond == 0) { LockstepsPerSecond = 1; } //minimum per second
 
    GameFramesPerSecond = LockstepsPerSecond * GameFramesPerLockstepTurn;
 
    PerformanceLog.LogGameFrameRate(LockStepTurnID, networkAverage, runtimeAverage, GameFramesPerSecond, LockstepsPerSecond, GameFramesPerLockstepTurn);
}
~~~

### 更新：支持单个玩家

自从本文发出以来，增加了单人模式得支持。

特别感谢[redstinggames.com](redstinggames.com)的Dan提供。可以在以下看到修改：

[Single Player Update diff](https://bitbucket.org/brimock/dynamic-lockstep-sample/commits/11539478537f52cbafd8cfd575ea067fdd6a9e49)

### 源代码

[Source code on bitbucket – Dynamic Lockstep Sample](https://bitbucket.org/brimock/dynamic-lockstep-sample)

