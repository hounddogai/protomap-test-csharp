namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled cost model overrides by code.</summary>
public interface IScheduledCostModelOverrideStore
{
    ScheduledCostModelOverride? Find(string code);
    void Save(ScheduledCostModelOverride item);
    bool Remove(string code);
    List<ScheduledCostModelOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledCostModelOverrideStore(Dictionary<string, ScheduledCostModelOverride> items, List<string> order) : IScheduledCostModelOverrideStore
{
    public Dictionary<string, ScheduledCostModelOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledCostModelOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledCostModelOverride item)
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

    public List<ScheduledCostModelOverride> List()
    {
        var outValue = new List<ScheduledCostModelOverride>();
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
public sealed class CachedScheduledCostModelOverrideStore(MemoryScheduledCostModelOverrideStore inner, ScheduledCostModelOverrideCache cache, int hits, int misses) : IScheduledCostModelOverrideStore
{
    public MemoryScheduledCostModelOverrideStore Inner { get; set; } = inner;
    public ScheduledCostModelOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledCostModelOverride? Find(string code)
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

    public void Save(ScheduledCostModelOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledCostModelOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled cost model overrides. Empty fields match all.</summary>
public sealed record ScheduledCostModelOverrideQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledCostModelOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledCostModelOverride> Apply(List<ScheduledCostModelOverride> items)
    {
        var matched = Collections.Filter(items, (ScheduledCostModelOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled cost model override store.</summary>
public static class ScheduledCostModelOverrideStore
{
    public static IScheduledCostModelOverrideStore NewMemoryScheduledCostModelOverrideStore()
    {
        return new MemoryScheduledCostModelOverrideStore(
            new Dictionary<string, ScheduledCostModelOverride>(),
            new List<string>());
    }

    public static IScheduledCostModelOverrideStore NewCachedScheduledCostModelOverrideStore(MemoryScheduledCostModelOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledCostModelOverrideStore(
            inner,
            ScheduledCostModelOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledCostModelOverride> FindScheduledCostModelOverrides(IScheduledCostModelOverrideStore store, ScheduledCostModelOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
