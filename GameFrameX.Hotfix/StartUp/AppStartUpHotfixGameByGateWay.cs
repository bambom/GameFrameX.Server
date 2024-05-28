﻿using System.Net;
using System.Timers;
using GameFrameX.NetWork.Messages;
using GameFrameX.Proto.BuiltIn;
using SuperSocket.ClientEngine;
using Timer = System.Timers.Timer;

namespace GameFrameX.Hotfix.StartUp;

internal partial class AppStartUpHotfixGame
{
    AsyncTcpSession _gatewayClient;

    private Timer _gateWayReconnectionTimer;
    private Timer _gateWayHeartBeatTimer;
    private ReqActorHeartBeat _reqGatewayActorHeartBeat;

    private void SendToGatewayMessage(long messageUniqueId, IMessage message)
    {
        if (!_gatewayClient.IsConnected)
        {
            return;
        }

        var span = AppStartUpHotfixGame.messageEncoderHandler.Handler(message);
        if (Setting.IsDebug && Setting.IsDebugSend)
        {
            LogHelper.Debug(message.ToSendMessageString(Setting.ServerType, ServerType.Gateway));
        }

        _gatewayClient.TrySend(span);
    }

    private void StartGatewayClient()
    {
        _gateWayReconnectionTimer = new Timer
        {
            Interval = 5000
        };
        _gateWayReconnectionTimer.Elapsed += GateWayReconnectionTimerOnElapsed;
        _gateWayReconnectionTimer.Start();
        _gateWayHeartBeatTimer = new Timer
        {
            Interval = 5000
        };
        _gateWayHeartBeatTimer.Elapsed += GateWayHeartBeatTimerOnElapsed;
        _gateWayHeartBeatTimer.Start();
        _reqGatewayActorHeartBeat = new ReqActorHeartBeat();
        _gatewayClient = new AsyncTcpSession();
        _gatewayClient.Closed += GateWayClientOnClosed;
        _gatewayClient.DataReceived += GateWayClientOnDataReceived;
        _gatewayClient.Connected += GateWayClientOnConnected;
        _gatewayClient.Error += GateWayClientOnError;
    }

    private void GateWayHeartBeatTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _reqGatewayActorHeartBeat.Timestamp = TimeHelper.UnixTimeSeconds();
        _reqGatewayActorHeartBeat.UpdateUniqueId();
        SendToGatewayMessage(_reqGatewayActorHeartBeat.UniqueId, _reqGatewayActorHeartBeat);
    }

    private void GateWayReconnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        ConnectToGateWay();
    }

    private void GateWayClientOnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs errorEventArgs)
    {
        LogHelper.Info("和网关服务器链接链接发生错误!" + errorEventArgs);
        GateWayClientOnClosed(sender, errorEventArgs);
    }

    private void GateWayClientOnConnected(object sender, EventArgs e)
    {
        // 和网关服务器链接成功，关闭重连
        _gateWayReconnectionTimer.Stop();
        _gateWayHeartBeatTimer.Start();
        LogHelper.Info("和网关服务器链接链接成功!");
    }

    private void GateWayClientOnDataReceived(object o, DataEventArgs dataEventArgs)
    {
        var messageData = dataEventArgs.Data.ReadBytes(dataEventArgs.Offset, dataEventArgs.Length);
        // var message = messageRouterDecoderHandler.RpcHandler(messageData);
        // if (message is IActorResponseMessage actorResponseMessage)
        // {
        //     rpcSession.Reply(actorResponseMessage);
        // }


        // var messageObject = (BaseMessageObject)message;
        // LogHelper.Info($"收到发现中心服务器消息：{dataEventArgs.Data}: {messageObject}");
    }

    private void GateWayClientOnClosed(object sender, EventArgs eventArgs)
    {
        LogHelper.Info("和网关服务器链接链接断开!开启重连");
        // 和网关服务器链接断开，开启重连
        _gateWayReconnectionTimer.Start();
        _gateWayHeartBeatTimer.Stop();
    }

    private void ConnectToGateWay()
    {
        if (ConnectTargetServer == null)
        {
            return;
        }

        var endPoint = new IPEndPoint(IPAddress.Parse((string)ConnectTargetServer.TargetIP), ConnectTargetServer.TargetPort);
        _gatewayClient.Connect(endPoint);
    }

    private void DisconnectToGateWay()
    {
        _gatewayClient?.Close();
    }
}