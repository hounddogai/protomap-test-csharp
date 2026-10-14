namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary balance metrics by code.</summary>
public interface IPrimaryBalanceMetricStore
{
    PrimaryBalanceMetric? Find(string code);
    void Save(PrimaryBalanceMetric item);
    bool Remove(string code);
    List<PrimaryBalanceMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryBalanceMetricStore(Dictionary<string, PrimaryBalanceMetric> items, List<string> order) : IPrimaryBalanceMetricStore
{
    public Dictionary<string, PrimaryBalanceMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryBalanceMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryBalanceMetric item)
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

    public List<PrimaryBalanceMetric> List()
    {
        var outValue = new List<PrimaryBalanceMetric>();
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
public sealed class CachedPrimaryBalanceMetricStore(MemoryPrimaryBalanceMetricStore inner, PrimaryBalanceMetricCache cache, int hits, int misses) : IPrimaryBalanceMetricStore
{
    public MemoryPrimaryBalanceMetricStore Inner { get; set; } = inner;
    public PrimaryBalanceMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryBalanceMetric? Find(string code)
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

    public void Save(PrimaryBalanceMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryBalanceMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary balance metrics. Empty fields match all.</summary>
public sealed record PrimaryBalanceMetricQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryBalanceMetric item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<PrimaryBalanceMetric> Apply(List<PrimaryBalanceMetric> items)
    {
        var matched = Collections.Filter(items, (PrimaryBalanceMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary balance metric store.</summary>
public static class PrimaryBalanceMetricStore
{
    public static IPrimaryBalanceMetricStore NewMemoryPrimaryBalanceMetricStore()
    {
        return new MemoryPrimaryBalanceMetricStore(
            new Dictionary<string, PrimaryBalanceMetric>(),
            new List<string>());
    }

    public static IPrimaryBalanceMetricStore NewCachedPrimaryBalanceMetricStore(MemoryPrimaryBalanceMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryBalanceMetricStore(inner, PrimaryBalanceMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryBalanceMetric> FindPrimaryBalanceMetrics(IPrimaryBalanceMetricStore store, PrimaryBalanceMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
