namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback bonus pool milestones by code.</summary>
public interface IFallbackBonusPoolMilestoneStore
{
    FallbackBonusPoolMilestone? Find(string code);
    void Save(FallbackBonusPoolMilestone item);
    bool Remove(string code);
    List<FallbackBonusPoolMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBonusPoolMilestoneStore(Dictionary<string, FallbackBonusPoolMilestone> items, List<string> order) : IFallbackBonusPoolMilestoneStore
{
    public Dictionary<string, FallbackBonusPoolMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBonusPoolMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBonusPoolMilestone item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<FallbackBonusPoolMilestone> List()
    {
        var outValue = new List<FallbackBonusPoolMilestone>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedFallbackBonusPoolMilestoneStore(MemoryFallbackBonusPoolMilestoneStore inner, FallbackBonusPoolMilestoneCache cache, int hits, int misses) : IFallbackBonusPoolMilestoneStore
{
    public MemoryFallbackBonusPoolMilestoneStore Inner { get; set; } = inner;
    public FallbackBonusPoolMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBonusPoolMilestone? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(FallbackBonusPoolMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBonusPoolMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback bonus pool milestones. Empty fields match all.</summary>
public sealed record FallbackBonusPoolMilestoneQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBonusPoolMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackBonusPoolMilestone> Apply(List<FallbackBonusPoolMilestone> items)
    {
        var matched = Collections.Filter(items, (FallbackBonusPoolMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback bonus pool milestone store.</summary>
public static class FallbackBonusPoolMilestoneStore
{
    public static IFallbackBonusPoolMilestoneStore NewMemoryFallbackBonusPoolMilestoneStore()
    {
        return new MemoryFallbackBonusPoolMilestoneStore(
            new Dictionary<string, FallbackBonusPoolMilestone>(),
            new List<string>());
    }

    public static IFallbackBonusPoolMilestoneStore NewCachedFallbackBonusPoolMilestoneStore(MemoryFallbackBonusPoolMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBonusPoolMilestoneStore(
            inner,
            FallbackBonusPoolMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackBonusPoolMilestone> FindFallbackBonusPoolMilestones(IFallbackBonusPoolMilestoneStore store, FallbackBonusPoolMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
