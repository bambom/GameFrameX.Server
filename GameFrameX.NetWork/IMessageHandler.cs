using GameFrameX.NetWork.Messages;

namespace GameFrameX.NetWork;

public interface IMessageHandler
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    Task Init();

    /// <summary>
    /// 内部执行
    /// </summary>
    /// <returns></returns>
    Task InnerAction();

    /// <summary>
    /// 消息对象
    /// </summary>
    MessageObject Message { get; set; }

    /// <summary>
    /// 网络频道对象
    /// </summary>
    INetChannel Channel { get; set; }
}