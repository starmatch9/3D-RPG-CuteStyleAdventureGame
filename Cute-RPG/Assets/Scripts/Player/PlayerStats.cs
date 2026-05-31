using UnityEngine;

// 挂载的那个对象叫做脚本对象（一般就是通过这种方式创建出来的）
[CreateAssetMenu(
    fileName = "NewPlayerStats",   // 这个定义创建好后显示的默认名称
    menuName = "CreateStats/New Player Stats"  // 这个定义右击菜单后显示的路径
)]
public class PlayerStats : EntityStats<PlayerStats>
{
    //==============================【运动属性】==============================//
    [Header("Motion Stats")]
    public bool applySlopeFactor = true;   // 是否考虑坡度因子
    public float acceleration = 13f;       // 加速度
    public float deceleration = 28f;       // 减速度
    public float friction = 28f;           // 地面摩擦力
    public float slopeFriction = 18f;      // 坡面摩擦力
    public float topSpeed = 6f;            // 最高速度
    public float turningDrag = 28f;        // 转向时的阻力
    public float airAcceleration = 32f;    // 空中加速度
    public float brakeThreshold = -0.8f;   // 刹车判定阈值
    public float slopeUpwardForce = 25f;   // 上坡时的额外推力
    public float slopeDownwardForce = 28f; // 下坡时的额外推力
}