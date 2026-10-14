namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted merit cycle tiers by code.</summary>
public interface IWeightedMeritCycleTierStore
{
    WeightedMeritCycleTier? Find(string code);
    void Save(WeightedMeritCycleTier item);
    bool Remove(string code);
    List<WeightedMeritCycleTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedMeritCycleTierStore(Dictionary<string, WeightedMeritCycleTier> items, List<string> order) : IWeightedMeritCycleTierStore
{
    public Dictionary<string, WeightedMeritCycleTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedMeritCycleTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedMeritCycleTier item)
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

    public List<WeightedMeritCycleTier> List()
    {
        var outValue = new List<WeightedMeritCycleTier>();
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
public sealed class CachedWeightedMeritCycleTierStore(MemoryWeightedMeritCycleTierStore inner, WeightedMeritCycleTierCache cache, int hits, int misses) : IWeightedMeritCycleTierStore
{
    public MemoryWeightedMeritCycleTierStore Inner { get; set; } = inner;
    public WeightedMeritCycleTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedMeritCycleTier? Find(string code)
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

    public void Save(WeightedMeritCycleTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedMeritCycleTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted merit cycle tiers. Empty fields match all.</summary>
public sealed record WeightedMeritCycleTierQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedMeritCycleTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedMeritCycleTier> Apply(List<WeightedMeritCycleTier> items)
    {
        var matched = Collections.Filter(items, (WeightedMeritCycleTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted merit cycle tier store.</summary>
public static class WeightedMeritCycleTierStore
{
    public static IWeightedMeritCycleTierStore NewMemoryWeightedMeritCycleTierStore()
    {
        return new MemoryWeightedMeritCycleTierStore(
            new Dictionary<string, WeightedMeritCycleTier>(),
            new List<string>());
    }

    public static IWeightedMeritCycleTierStore NewCachedWeightedMeritCycleTierStore(MemoryWeightedMeritCycleTierStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedMeritCycleTierStore(
            inner,
            WeightedMeritCycleTierCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedMeritCycleTier> FindWeightedMeritCycleTiers(IWeightedMeritCycleTierStore store, WeightedMeritCycleTierQuery query)
    {
        return query.Apply(store.List());
    }
}
