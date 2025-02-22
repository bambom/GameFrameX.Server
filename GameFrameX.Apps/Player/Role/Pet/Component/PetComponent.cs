﻿using GameFrameX.Apps.Player.Role.Pet.Entity;
using GameFrameX.Core.Abstractions.Attribute;
using GameFrameX.Core.Components;
using GameFrameX.Utility.Setting;

namespace GameFrameX.Apps.Player.Role.Pet.Component;

[ComponentType(GlobalConst.ActorTypePlayer)]
public class PetComponent : StateComponent<PetState>
{
}