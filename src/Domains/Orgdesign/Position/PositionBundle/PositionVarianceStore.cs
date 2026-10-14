namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position variances by code.</summary>
public interface IPositionVarianceStore
{
    PositionVariance? Find(string code);
    void Save(PositionVariance item);
    bool Remove(string code);
    List<PositionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionVarianceStore(Dictionary<string, PositionVariance> items, List<string> order) : IPositionVarianceStore
{
    public Dictionary<string, PositionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionVariance item)
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

    public List<PositionVariance> List()
    {
        var outValue = new List<PositionVariance>();
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
public sealed class CachedPositionVarianceStore(MemoryPositionVarianceStore inner, PositionVarianceCache cache, int hits, int misses) : IPositionVarianceStore
{
    public MemoryPositionVarianceStore Inner { get; set; } = inner;
    public PositionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionVariance? Find(string code)
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

    public void Save(PositionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position variances. Empty fields match all.</summary>
public sealed record PositionVarianceQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PositionVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionVariance> Apply(List<PositionVariance> items)
    {
        var matched = Collections.Filter(items, (PositionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position variance store.</summary>
public static class PositionVarianceStore
{
    public static IPositionVarianceStore NewMemoryPositionVarianceStore()
    {
        return new MemoryPositionVarianceStore(new Dictionary<string, PositionVariance>(), new List<string>());
    }

    public static IPositionVarianceStore NewCachedPositionVarianceStore(MemoryPositionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPositionVarianceStore(inner, PositionVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionVariance> FindPositionVariances(IPositionVarianceStore store, PositionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
