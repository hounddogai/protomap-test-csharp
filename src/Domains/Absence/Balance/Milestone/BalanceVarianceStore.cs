namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance variances by code.</summary>
public interface IBalanceVarianceStore
{
    BalanceVariance? Find(string code);
    void Save(BalanceVariance item);
    bool Remove(string code);
    List<BalanceVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceVarianceStore(Dictionary<string, BalanceVariance> items, List<string> order) : IBalanceVarianceStore
{
    public Dictionary<string, BalanceVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceVariance item)
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

    public List<BalanceVariance> List()
    {
        var outValue = new List<BalanceVariance>();
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
public sealed class CachedBalanceVarianceStore(MemoryBalanceVarianceStore inner, BalanceVarianceCache cache, int hits, int misses) : IBalanceVarianceStore
{
    public MemoryBalanceVarianceStore Inner { get; set; } = inner;
    public BalanceVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceVariance? Find(string code)
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

    public void Save(BalanceVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance variances. Empty fields match all.</summary>
public sealed record BalanceVarianceQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceVariance item)
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

    public List<BalanceVariance> Apply(List<BalanceVariance> items)
    {
        var matched = Collections.Filter(items, (BalanceVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance variance store.</summary>
public static class BalanceVarianceStore
{
    public static IBalanceVarianceStore NewMemoryBalanceVarianceStore()
    {
        return new MemoryBalanceVarianceStore(new Dictionary<string, BalanceVariance>(), new List<string>());
    }

    public static IBalanceVarianceStore NewCachedBalanceVarianceStore(MemoryBalanceVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceVarianceStore(inner, BalanceVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BalanceVariance> FindBalanceVariances(IBalanceVarianceStore store, BalanceVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
