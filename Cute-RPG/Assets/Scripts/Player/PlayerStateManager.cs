using System.Collections.Generic;
using UnityEngine;

// 绑定Player
[RequireComponent(typeof(Player))] // 这个就是强制要求组件所在物体必须有Player组件
public class PlayerStateManager : EntityStateManager<Player>
{
    [ClassTypeName(typeof(PlayerState))] public string[] states;

    protected override List<EntityState<Player>> GetStateList()
    {
        return PlayerState.CreateListFromStringArray(states);
    }
}