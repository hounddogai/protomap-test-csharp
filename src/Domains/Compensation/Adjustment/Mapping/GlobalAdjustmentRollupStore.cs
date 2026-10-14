namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global adjustment rollups by code.</summary>
public interface IGlobalAdjustmentRollupStore
{
    GlobalAdjustmentRollup? Find(string code);
    void Save(GlobalAdjustmentRollup item);
    bool Remove(string code);
    List<GlobalAdjustmentRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAdjustmentRollupStore(Dictionary<string, GlobalAdjustmentRollup> items, List<string> order) : IGlobalAdjustmentRollupStore
{
    public Dictionary<string, GlobalAdjustmentRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAdjustmentRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAdjustmentRollup item)
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

    public List<GlobalAdjustmentRollup> List()
    {
        var outValue = new List<GlobalAdjustmentRollup>();
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
public sealed class CachedGlobalAdjustmentRollupStore(MemoryGlobalAdjustmentRollupStore inner, GlobalAdjustmentRollupCache cache, int hits, int misses) : IGlobalAdjustmentRollupStore
{
    public MemoryGlobalAdjustmentRollupStore Inner { get; set; } = inner;
    public GlobalAdjustmentRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAdjustmentRollup? Find(string code)
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

    public void Save(GlobalAdjustmentRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAdjustmentRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global adjustment rollups. Empty fields match all.</summary>
public sealed record GlobalAdjustmentRollupQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAdjustmentRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAdjustmentRollup> Apply(List<GlobalAdjustmentRollup> items)
    {
        var matched = Collections.Filter(items, (GlobalAdjustmentRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global adjustment rollup store.</summary>
public static class GlobalAdjustmentRollupStore
{
    public static IGlobalAdjustmentRollupStore NewMemoryGlobalAdjustmentRollupStore()
    {
        return new MemoryGlobalAdjustmentRollupStore(
            new Dictionary<string, GlobalAdjustmentRollup>(),
            new List<string>());
    }

    public static IGlobalAdjustmentRollupStore NewCachedGlobalAdjustmentRollupStore(MemoryGlobalAdjustmentRollupStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAdjustmentRollupStore(
            inner,
            GlobalAdjustmentRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAdjustmentRollup> FindGlobalAdjustmentRollups(IGlobalAdjustmentRollupStore store, GlobalAdjustmentRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
