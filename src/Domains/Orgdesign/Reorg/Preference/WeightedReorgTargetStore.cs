namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted reorg targets by code.</summary>
public interface IWeightedReorgTargetStore
{
    WeightedReorgTarget? Find(string code);
    void Save(WeightedReorgTarget item);
    bool Remove(string code);
    List<WeightedReorgTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedReorgTargetStore(Dictionary<string, WeightedReorgTarget> items, List<string> order) : IWeightedReorgTargetStore
{
    public Dictionary<string, WeightedReorgTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedReorgTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedReorgTarget item)
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

    public List<WeightedReorgTarget> List()
    {
        var outValue = new List<WeightedReorgTarget>();
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
public sealed class CachedWeightedReorgTargetStore(MemoryWeightedReorgTargetStore inner, WeightedReorgTargetCache cache, int hits, int misses) : IWeightedReorgTargetStore
{
    public MemoryWeightedReorgTargetStore Inner { get; set; } = inner;
    public WeightedReorgTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedReorgTarget? Find(string code)
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

    public void Save(WeightedReorgTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedReorgTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted reorg targets. Empty fields match all.</summary>
public sealed record WeightedReorgTargetQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedReorgTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedReorgTarget> Apply(List<WeightedReorgTarget> items)
    {
        var matched = Collections.Filter(items, (WeightedReorgTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted reorg target store.</summary>
public static class WeightedReorgTargetStore
{
    public static IWeightedReorgTargetStore NewMemoryWeightedReorgTargetStore()
    {
        return new MemoryWeightedReorgTargetStore(new Dictionary<string, WeightedReorgTarget>(), new List<string>());
    }

    public static IWeightedReorgTargetStore NewCachedWeightedReorgTargetStore(MemoryWeightedReorgTargetStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedReorgTargetStore(inner, WeightedReorgTargetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WeightedReorgTarget> FindWeightedReorgTargets(IWeightedReorgTargetStore store, WeightedReorgTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
