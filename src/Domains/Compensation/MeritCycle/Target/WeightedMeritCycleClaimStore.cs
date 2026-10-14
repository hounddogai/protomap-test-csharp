namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted merit cycle claims by code.</summary>
public interface IWeightedMeritCycleClaimStore
{
    WeightedMeritCycleClaim? Find(string code);
    void Save(WeightedMeritCycleClaim item);
    bool Remove(string code);
    List<WeightedMeritCycleClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedMeritCycleClaimStore(Dictionary<string, WeightedMeritCycleClaim> items, List<string> order) : IWeightedMeritCycleClaimStore
{
    public Dictionary<string, WeightedMeritCycleClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedMeritCycleClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedMeritCycleClaim item)
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

    public List<WeightedMeritCycleClaim> List()
    {
        var outValue = new List<WeightedMeritCycleClaim>();
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
public sealed class CachedWeightedMeritCycleClaimStore(MemoryWeightedMeritCycleClaimStore inner, WeightedMeritCycleClaimCache cache, int hits, int misses) : IWeightedMeritCycleClaimStore
{
    public MemoryWeightedMeritCycleClaimStore Inner { get; set; } = inner;
    public WeightedMeritCycleClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedMeritCycleClaim? Find(string code)
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

    public void Save(WeightedMeritCycleClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedMeritCycleClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted merit cycle claims. Empty fields match all.</summary>
public sealed record WeightedMeritCycleClaimQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedMeritCycleClaim item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedMeritCycleClaim> Apply(List<WeightedMeritCycleClaim> items)
    {
        var matched = Collections.Filter(items, (WeightedMeritCycleClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted merit cycle claim store.</summary>
public static class WeightedMeritCycleClaimStore
{
    public static IWeightedMeritCycleClaimStore NewMemoryWeightedMeritCycleClaimStore()
    {
        return new MemoryWeightedMeritCycleClaimStore(
            new Dictionary<string, WeightedMeritCycleClaim>(),
            new List<string>());
    }

    public static IWeightedMeritCycleClaimStore NewCachedWeightedMeritCycleClaimStore(MemoryWeightedMeritCycleClaimStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedMeritCycleClaimStore(
            inner,
            WeightedMeritCycleClaimCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedMeritCycleClaim> FindWeightedMeritCycleClaims(IWeightedMeritCycleClaimStore store, WeightedMeritCycleClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
