using UnityEngine;

// 挂载的那个对象叫做脚本对象（一般就是通过这种方式创建出来的）
[CreateAssetMenu(
    fileName = "NewPlayerStats",   // 这个定义创建好后显示的默认名称
    menuName = "CreateStats/New Player Stats"  // 这个定义右击菜单后显示的路径
)]
public class PlayerStats : EntityStats<PlayerStats>
{
    [Header("General Stats")]
    public float snapForce = 15f;          // 将角色贴合到地面的吸附力
    public float rotationSpeed = 970f;     // 玩家角色旋转速度（度/秒）
    public float gravity = 38f;            // 普通重力加速度
    public float fallGravity = 65f;        // 下落时额外重力加速度
    public float gravityTopSpeed = 50f;    // 重力作用下的最大下落速度
    
    [Header("Motion Stats")]
    public float acceleration = 13f;       // 加速度
    public float deceleration = 28f;       // 减速度
    public float friction = 28f;           // 地面摩擦力
    public float slopeFriction = 18f;      // 坡面摩擦力
    public float topSpeed = 6f;            // 最高速度
    public float turningDrag = 28f;        // 转向时的阻力
    public float brakeThreshold = -0.8f;   // 刹车判定阈值


    [Header("Jump Stats")] 
    public int multiJumps = 1;
    public float coyoteJumpThreshold = 0.15f;
    public float maxJumpHeight = 17f;
    public float minJumpHeight = 13f;
    
}