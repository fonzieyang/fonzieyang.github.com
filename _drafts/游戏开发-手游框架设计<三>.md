##### 网游协议开发
1) 使用中间语言定义协议
在过去的项目中，是这样做协议定义的：

	struct MSG_XX_MOVE {
		int x,y;
		int vx,vy;
		int ax,ay;
	};
	
这样定义结构体消息，然后前后端都用C++开发，一起include这份文件。这样做的最大问题是，不够简洁，这是因为C++描述这件事情不是那么在行，不能容易地表达出可选这种情形。而且一旦类中间加入各种方法，比如流处理等等，马上就庞大起来。更要命的是，每次变协议，代码编译时间长得吓人。

如果我们用中间格式来定义，那么事情就简单多了，比如protobuf:

	message MSG_XX_MOVE {
		required int32 x = 1;
		required int32 y = 2;
		optional int32 vx = 3;
		optional int32 vy = 4;
	}

我们可以再加入更加规范的用法使得协议变得清晰，比如我们规定协议分为两种模型，一种是请求-应答，一种是推送(服务器主动通知)，一般能够满足游戏需求。
那么规定协议后缀名分别为：
	XX_MOVE_Request     用于请求数据
	XX_MOVE_Response    用于接受数据
	XX_MOVE_Notify      用于推送数据
其中，请求必定有应答返回，推送则是服务器主动发包。另外还规定，Response必定有ReturnCode，告知业务上的情况。
下面是一个聊天协议例子：

	CharMsgRequest{
		required int32 recvId;
		required string content;
	}

	CharMsgResponse{
		enum ErrorCode {
			Error   = 0;
			Success = 1;
		}
		required ErrorCode returnCode = 1;
		optional int32 recvId = 2;
	}

	CharMsgNotify{
		required int32 senderId;
		required string content;
	}
是不是不需要解释什么，都能够弄清楚整个聊天流程呢？通过protobuf，我们很清楚地设计好了我们的协议。从而提升了生产力。

2) 自动编码、解码

有的项目，数据包需要自己手工进行流处理：
	
	-- 编码
	stream.writeInt(10)
	stream.writeString("hi")



云风的pbc库可以做到自动编码解码，一下是例子代码：
	
	-- 编码
	pbc.encode("CharMsgResquest", {recvId = 10, content = "hi"})
	-- 解码
	local buf = getBuf()
	local data = pbc.decode("CharMsgResponse", buf)
	print(data.returnCode)
	
只要把编码、解码封装在网络层，那么用起来就更加方便了。

2) 事件机制

在游戏开发中，我们希望各个功能模块尽可能的独立，事件机制是你的好朋友。
比如排行榜系统希望点击榜中角色可以跳转到详情界面，此事我们只要派发跳转详情消息到系统，系统就会把事件传递给感兴趣的对象，然后做相应的处理。
这样比起直接调用的好处：

1. 松耦合，不需要依赖详情系统，你只要派发事件。
2. 简洁，详情系统只管提供服务。
3. 灵活，其他系统可以对事件进行监听拦截，从而让系统更有弹性。

3) 网络层

网络层的核心功能：高效、低延迟、省流量、安全地与服务器通信、容易调试。
一个游戏时候流畅，很大程度与协议设计有关，网络层的性能有时候是其次。

下面是粗略的设计，其中的协议使用protobuf描述：


1 高效，在协议设计上，我们提供合并数据包的机会。

网络包示意：
	

	message NetworkPackage {
		required int32 msgCount = 1;
		repeated GameMsg msg = 2;
	}
	

一次发包，都尽可能把多个要发出的数据合并起来，上面的定义会告诉你有多少条消息，通过这个包头，我们就可以着手自动合并数据，自动节省传送次数，提升用户体验。


2 低延迟

像之前所说的，网络性能，很大程度与使用有关，比如对时服务、位置同步等等延迟敏感的，用tcp就是没udp效果好。另外关闭nag算法可以有效降低延迟。

3 省流量

对网络包做一下压缩编码，体积会小很多，配合合并消息包，可以节省不少数据量。

4 安全

一般openssl就够了
	
5 方便调试

此外，我还希望网络层可以方便调试，而且可以类似沙盒可以使用假数据。我会设计一个“假服务器”，它就像真的服务器一样接受和返回数据，但是中间都是在本地运行，返回的数据都是假的。
Fake Server的好处就是方便调试，特别是项目早期，服务器功能都不完善，联调又浪费时间。我在多个项目中使用Fake Server设计，结果大大加快了我们的开发速度，以及减少了bug的个数，很多bug直接在本地就被发现了。

	

4) 利用协程

通过协程，我们异步的方式可以完全不同，甚至你看不出来你的代码是异步的。
在我第一次接触协程的时候，是使用lua的时候，当时(lua5.1)非常兴奋，于是赶紧重写了框架，把协程封装在背后，使用者只管去用，但后来遇到很多问题，深入阅读源码后才发现是lua对协程支持不够，简单来说是当你lua中的c函数yield出来之后，没法resume回去。(原因是在c函数的调用栈帧已经被释放了)当然现在的版本已经有相当的支持，下面演示一下异步的时候用回调来完成与用协程来完成的区别。

回调：

	player.login(function()
					print("I'm login")
				 end)
				 
协程：

	player.login()
	print("I'm login")
	
协程实际上是做了什么呢？在上例中，当player.login()调用时，网络层是非阻塞地完成了发包。这里只不过是利用协程对回调进行一次包装，当数据包接受到了，回调函数被调用，就会resume回到yield出去的点，也就是player.login执行完，接着就像没有发生过等待一样继续往下执行。我对于协程的经验之谈是，协程尽量不要暴露给逻辑，尽可能地隐藏在底层，比如上面的例子，我们可以隐藏在网络层。这样代码看上去就很自然优雅。

5) 游戏世界的高层同步

这个标题是来自《游戏编程精粹7》，想更加深入了解的同学请自行查阅。基本思想是可以配置一些元数据去定义某些玩家的属性自动同步，简化逻辑。
经常都会看到这么设计协议，比如强化某位英雄：

	message TrainHeroRequest {
	    required int32 heroId = 1;
	}

	message TrainHeroResponse {
	    enum ErrCode
	    {
	        Success = 0;
	        NoItem = 1;
	        NotEnoughMoney = 2;
	        NoEmptyTrainRoom = 3;
	        Fail = 4;
	    }
	    required ErrCode returnCode = 1 [default = Success];
	    optional GameCoin consume = 2;
	}

注意到上面的协议中，会告诉客户端你实际花了多少钱，然后你会把钱给扣除，在手游这种简单的设计是很合理的。
但是如果情况比较复杂，最好的做法还是这些数据自动同步，不需要专门写协议告诉客户端去同步。
这样有好有坏，
好处是：节省了很多重复定义的数据同步。完善后不容易出错。
坏处是：这样比较不好跟踪(实际上还是有办法，比如每次属性的变动都增加一个reason，让其自动打log)，而且实现一套自动同步，必须要求你的环境接受服务器推送消息。很多手游都是基于http，做这种同步推送不方便，要另外建立一条长连接。还有就是这样一套同步代码也是要花费不少精力去完成。


