using UnityEngine;

public abstract class EntityStatsManager<T> : MonoBehaviour where T : EntityStats<T>
{
    public T[] stats;
    
    public T current { get; protected set; }
    
    public virtual void Change(int to)
    {
        // 确保索引合法
        if (to >= 0 && to < stats.Length)
        {
            // 如果切换的不是当前属性，则进行切换
            if (current != stats[to])
            {
                current = stats[to];
            }
        }
    }

    protected virtual void Start()
    {
        if (stats.Length > 0)
        {
            current = stats[0];
        }
    }
}