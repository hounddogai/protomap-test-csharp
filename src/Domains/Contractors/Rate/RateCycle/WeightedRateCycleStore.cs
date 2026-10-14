namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate cycles by code.</summary>
public interface IWeightedRateCycleStore
{
    WeightedRateCycle? Find(string code);
    void Save(WeightedRateCycle item);
    bool Remove(string code);
    List<WeightedRateCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRateCycleStore(Dictionary<string, WeightedRateCycle> items, List<string> order) : IWeightedRateCycleStore
{
    public Dictionary<string, WeightedRateCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRateCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRateCycle item)
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

    public List<WeightedRateCycle> List()
    {
        var outValue = new List<WeightedRateCycle>();
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
public sealed class CachedWeightedRateCycleStore(MemoryWeightedRateCycleStore inner, WeightedRateCycleCache cache, int hits, int misses) : IWeightedRateCycleStore
{
    public MemoryWeightedRateCycleStore Inner { get; set; } = inner;
    public WeightedRateCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRateCycle? Find(string code)
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

    public void Save(WeightedRateCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRateCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate cycles. Empty fields match all.</summary>
public sealed record WeightedRateCycleQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRateCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRateCycle> Apply(List<WeightedRateCycle> items)
    {
        var matched = Collections.Filter(items, (WeightedRateCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate cycle store.</summary>
public static class WeightedRateCycleStore
{
    public static IWeightedRateCycleStore NewMemoryWeightedRateCycleStore()
    {
        return new MemoryWeightedRateCycleStore(new Dictionary<string, WeightedRateCycle>(), new List<string>());
    }

    public static IWeightedRateCycleStore NewCachedWeightedRateCycleStore(MemoryWeightedRateCycleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRateCycleStore(inner, WeightedRateCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedRateCycle> FindWeightedRateCycles(IWeightedRateCycleStore store, WeightedRateCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
