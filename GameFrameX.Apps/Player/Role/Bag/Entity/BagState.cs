using GameFrameX.DataBase.Mongo;

namespace GameFrameX.Apps.Player.Role.Bag.Entity;

public class BagState : CacheState
{
    public Dictionary<int, long> ItemMap = new();
}