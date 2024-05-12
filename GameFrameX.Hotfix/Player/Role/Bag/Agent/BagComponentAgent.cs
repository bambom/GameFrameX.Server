﻿using GameFrameX.Apps.Player.Role.Bag.Component;
using GameFrameX.Apps.Player.Role.Bag.Entity;
using GameFrameX.Hotfix.Common.Events;
using GameFrameX.Hotfix.Player.Role.Role.Agent;

namespace GameFrameX.Hotfix.Player.Role.Bag.Agent
{
    public class BagComponentAgent : StateComponentAgent<BagComponent, BagState>
    {
        public override void Active()
        {
            if (State.ItemMap.Count <= 0)
            {
                State.ItemMap.Add(101, 1);
                State.ItemMap.Add(103, 100);
            }
        }

        private RespBagInfo BuildInfoMsg()
        {
            var res = new RespBagInfo();
            foreach (var kv in State.ItemMap)
                res.ItemDic[kv.Key] = kv.Value;
            return res;
        }

        public async Task GetBagInfo(ReqBagInfo reqMsg)
        {
            var ret = BuildInfoMsg();
            ret.UniqueId = reqMsg.UniqueId;
            // await this.NotifyClient(ret, reqMsg.UniId);
        }

        /// <summary>
        /// 宠物合成
        /// </summary>
        /// <returns></returns>
        public async Task ComposePet(ReqComposePet reqMsg)
        {
            //宠物碎片合成相关逻辑
            //.....
            //.....

            //合成成功后分发一个获得宠物的事件(在PetCompAgent中监听此事件)
            this.Dispatch(EventId.GotNewPet, new OneParam<int>(1000));

            var res = new RespComposePet();
            res.PetId = 1000;
            // await this.NotifyClient(res, reqMsg.UniId);
        }
    }
}