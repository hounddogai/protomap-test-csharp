namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary refresh variances by code.</summary>
public interface IPrimaryRefreshVarianceStore
{
    PrimaryRefreshVariance? Find(string code);
    void Save(PrimaryRefreshVariance item);
    bool Remove(string code);
    List<PrimaryRefreshVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRefreshVarianceStore(Dictionary<string, PrimaryRefreshVariance> items, List<string> order) : IPrimaryRefreshVarianceStore
{
    public Dictionary<string, PrimaryRefreshVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRefreshVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRefreshVariance item)
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

    public List<PrimaryRefreshVariance> List()
    {
        var outValue = new List<PrimaryRefreshVariance>();
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
public sealed class CachedPrimaryRefreshVarianceStore(MemoryPrimaryRefreshVarianceStore inner, PrimaryRefreshVarianceCache cache, int hits, int misses) : IPrimaryRefreshVarianceStore
{
    public MemoryPrimaryRefreshVarianceStore Inner { get; set; } = inner;
    public PrimaryRefreshVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRefreshVariance? Find(string code)
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

    public void Save(PrimaryRefreshVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRefreshVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary refresh variances. Empty fields match all.</summary>
public sealed record PrimaryRefreshVarianceQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRefreshVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PrimaryRefreshVariance> Apply(List<PrimaryRefreshVariance> items)
    {
        var matched = Collections.Filter(items, (PrimaryRefreshVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary refresh variance store.</summary>
public static class PrimaryRefreshVarianceStore
{
    public static IPrimaryRefreshVarianceStore NewMemoryPrimaryRefreshVarianceStore()
    {
        return new MemoryPrimaryRefreshVarianceStore(
            new Dictionary<string, PrimaryRefreshVariance>(),
            new List<string>());
    }

    public static IPrimaryRefreshVarianceStore NewCachedPrimaryRefreshVarianceStore(MemoryPrimaryRefreshVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRefreshVarianceStore(
            inner,
            PrimaryRefreshVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryRefreshVariance> FindPrimaryRefreshVariances(IPrimaryRefreshVarianceStore store, PrimaryRefreshVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
