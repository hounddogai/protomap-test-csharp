namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant variances by code.</summary>
public interface IEquityGrantVarianceStore
{
    EquityGrantVariance? Find(string code);
    void Save(EquityGrantVariance item);
    bool Remove(string code);
    List<EquityGrantVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantVarianceStore(Dictionary<string, EquityGrantVariance> items, List<string> order) : IEquityGrantVarianceStore
{
    public Dictionary<string, EquityGrantVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantVariance item)
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

    public List<EquityGrantVariance> List()
    {
        var outValue = new List<EquityGrantVariance>();
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
public sealed class CachedEquityGrantVarianceStore(MemoryEquityGrantVarianceStore inner, EquityGrantVarianceCache cache, int hits, int misses) : IEquityGrantVarianceStore
{
    public MemoryEquityGrantVarianceStore Inner { get; set; } = inner;
    public EquityGrantVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantVariance? Find(string code)
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

    public void Save(EquityGrantVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant variances. Empty fields match all.</summary>
public sealed record EquityGrantVarianceQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantVariance item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantVariance> Apply(List<EquityGrantVariance> items)
    {
        var matched = Collections.Filter(items, (EquityGrantVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant variance store.</summary>
public static class EquityGrantVarianceStore
{
    public static IEquityGrantVarianceStore NewMemoryEquityGrantVarianceStore()
    {
        return new MemoryEquityGrantVarianceStore(new Dictionary<string, EquityGrantVariance>(), new List<string>());
    }

    public static IEquityGrantVarianceStore NewCachedEquityGrantVarianceStore(MemoryEquityGrantVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantVarianceStore(inner, EquityGrantVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantVariance> FindEquityGrantVariances(IEquityGrantVarianceStore store, EquityGrantVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
