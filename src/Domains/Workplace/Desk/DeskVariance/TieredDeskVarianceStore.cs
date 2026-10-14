namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered desk variances by code.</summary>
public interface ITieredDeskVarianceStore
{
    TieredDeskVariance? Find(string code);
    void Save(TieredDeskVariance item);
    bool Remove(string code);
    List<TieredDeskVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredDeskVarianceStore(Dictionary<string, TieredDeskVariance> items, List<string> order) : ITieredDeskVarianceStore
{
    public Dictionary<string, TieredDeskVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredDeskVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredDeskVariance item)
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

    public List<TieredDeskVariance> List()
    {
        var outValue = new List<TieredDeskVariance>();
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
public sealed class CachedTieredDeskVarianceStore(MemoryTieredDeskVarianceStore inner, TieredDeskVarianceCache cache, int hits, int misses) : ITieredDeskVarianceStore
{
    public MemoryTieredDeskVarianceStore Inner { get; set; } = inner;
    public TieredDeskVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredDeskVariance? Find(string code)
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

    public void Save(TieredDeskVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredDeskVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered desk variances. Empty fields match all.</summary>
public sealed record TieredDeskVarianceQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(TieredDeskVariance item)
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

    public List<TieredDeskVariance> Apply(List<TieredDeskVariance> items)
    {
        var matched = Collections.Filter(items, (TieredDeskVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered desk variance store.</summary>
public static class TieredDeskVarianceStore
{
    public static ITieredDeskVarianceStore NewMemoryTieredDeskVarianceStore()
    {
        return new MemoryTieredDeskVarianceStore(new Dictionary<string, TieredDeskVariance>(), new List<string>());
    }

    public static ITieredDeskVarianceStore NewCachedTieredDeskVarianceStore(MemoryTieredDeskVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedTieredDeskVarianceStore(inner, TieredDeskVarianceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredDeskVariance> FindTieredDeskVariances(ITieredDeskVarianceStore store, TieredDeskVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
