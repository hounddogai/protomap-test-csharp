namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global position variances by code.</summary>
public interface IGlobalPositionVarianceStore
{
    GlobalPositionVariance? Find(string code);
    void Save(GlobalPositionVariance item);
    bool Remove(string code);
    List<GlobalPositionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPositionVarianceStore(Dictionary<string, GlobalPositionVariance> items, List<string> order) : IGlobalPositionVarianceStore
{
    public Dictionary<string, GlobalPositionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPositionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPositionVariance item)
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

    public List<GlobalPositionVariance> List()
    {
        var outValue = new List<GlobalPositionVariance>();
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
public sealed class CachedGlobalPositionVarianceStore(MemoryGlobalPositionVarianceStore inner, GlobalPositionVarianceCache cache, int hits, int misses) : IGlobalPositionVarianceStore
{
    public MemoryGlobalPositionVarianceStore Inner { get; set; } = inner;
    public GlobalPositionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPositionVariance? Find(string code)
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

    public void Save(GlobalPositionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPositionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global position variances. Empty fields match all.</summary>
public sealed record GlobalPositionVarianceQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPositionVariance item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPositionVariance> Apply(List<GlobalPositionVariance> items)
    {
        var matched = Collections.Filter(items, (GlobalPositionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global position variance store.</summary>
public static class GlobalPositionVarianceStore
{
    public static IGlobalPositionVarianceStore NewMemoryGlobalPositionVarianceStore()
    {
        return new MemoryGlobalPositionVarianceStore(
            new Dictionary<string, GlobalPositionVariance>(),
            new List<string>());
    }

    public static IGlobalPositionVarianceStore NewCachedGlobalPositionVarianceStore(MemoryGlobalPositionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPositionVarianceStore(
            inner,
            GlobalPositionVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPositionVariance> FindGlobalPositionVariances(IGlobalPositionVarianceStore store, GlobalPositionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
