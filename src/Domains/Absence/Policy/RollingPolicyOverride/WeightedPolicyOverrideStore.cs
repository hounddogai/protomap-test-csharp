namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted policy overrides by code.</summary>
public interface IWeightedPolicyOverrideStore
{
    WeightedPolicyOverride? Find(string code);
    void Save(WeightedPolicyOverride item);
    bool Remove(string code);
    List<WeightedPolicyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPolicyOverrideStore(Dictionary<string, WeightedPolicyOverride> items, List<string> order) : IWeightedPolicyOverrideStore
{
    public Dictionary<string, WeightedPolicyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPolicyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPolicyOverride item)
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

    public List<WeightedPolicyOverride> List()
    {
        var outValue = new List<WeightedPolicyOverride>();
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
public sealed class CachedWeightedPolicyOverrideStore(MemoryWeightedPolicyOverrideStore inner, WeightedPolicyOverrideCache cache, int hits, int misses) : IWeightedPolicyOverrideStore
{
    public MemoryWeightedPolicyOverrideStore Inner { get; set; } = inner;
    public WeightedPolicyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPolicyOverride? Find(string code)
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

    public void Save(WeightedPolicyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPolicyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted policy overrides. Empty fields match all.</summary>
public sealed record WeightedPolicyOverrideQuery(string Name, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPolicyOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPolicyOverride> Apply(List<WeightedPolicyOverride> items)
    {
        var matched = Collections.Filter(items, (WeightedPolicyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted policy override store.</summary>
public static class WeightedPolicyOverrideStore
{
    public static IWeightedPolicyOverrideStore NewMemoryWeightedPolicyOverrideStore()
    {
        return new MemoryWeightedPolicyOverrideStore(
            new Dictionary<string, WeightedPolicyOverride>(),
            new List<string>());
    }

    public static IWeightedPolicyOverrideStore NewCachedWeightedPolicyOverrideStore(MemoryWeightedPolicyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPolicyOverrideStore(
            inner,
            WeightedPolicyOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPolicyOverride> FindWeightedPolicyOverrides(IWeightedPolicyOverrideStore store, WeightedPolicyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
