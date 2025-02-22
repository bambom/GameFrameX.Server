using GameFrameX.Apps.Player.Player.Entity;
using GameFrameX.Monitor.Player;

namespace GameFrameX.Hotfix.Logic.Http;

/// <summary>
/// 角色创建
/// </summary>
[HttpMessageMapping(typeof(ReqPlayerCreateHttpHandler))]
[Description("角色创建,仅限前端调用")]
public sealed class ReqPlayerCreateHttpHandler : BaseHttpHandler
{
    public override async Task<MessageObject> Action(string ip, string url, Dictionary<string, object> paramMap, MessageObject messageObject)
    {
        var reqPlayerCreate = messageObject as ReqPlayerCreate;

        var playerState = await OnPlayerCreate(reqPlayerCreate);
        var respPlayerCreate = new RespPlayerCreate
        {
            UniqueId = reqPlayerCreate.UniqueId,
            PlayerInfo = new PlayerInfo
            {
                Id = playerState.Id,
                Name = playerState.Name,
                Level = playerState.Level,
                State = playerState.State,
                Avatar = playerState.Avatar,
            },
        };
        return respPlayerCreate;
    }

    private async Task<PlayerState> OnPlayerCreate(ReqPlayerCreate reqPlayerCreate)
    {
        var playerState = new PlayerState
        {
            Id = ActorIdGenerator.GetActorId(GlobalConst.ActorTypePlayer),
            AccountId = reqPlayerCreate.Id,
            Name = reqPlayerCreate.Name,
            Level = (uint)Utility.Random.Next(1, 50),
            State = 0,
            Avatar = (uint)Utility.Random.Next(1, 50),
        };
        MetricsPlayerRegister.CreateCounterOptions.Inc();
        await GameDb.SaveOneAsync(playerState);
        return playerState;
    }
}