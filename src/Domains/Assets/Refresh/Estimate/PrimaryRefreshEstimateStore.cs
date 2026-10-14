namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary refresh estimates by code.</summary>
public interface IPrimaryRefreshEstimateStore
{
    PrimaryRefreshEstimate? Find(string code);
    void Save(PrimaryRefreshEstimate item);
    bool Remove(string code);
    List<PrimaryRefreshEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRefreshEstimateStore(Dictionary<string, PrimaryRefreshEstimate> items, List<string> order) : IPrimaryRefreshEstimateStore
{
    public Dictionary<string, PrimaryRefreshEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRefreshEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRefreshEstimate item)
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

    public List<PrimaryRefreshEstimate> List()
    {
        var outValue = new List<PrimaryRefreshEstimate>();
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
public sealed class CachedPrimaryRefreshEstimateStore(MemoryPrimaryRefreshEstimateStore inner, PrimaryRefreshEstimateCache cache, int hits, int misses) : IPrimaryRefreshEstimateStore
{
    public MemoryPrimaryRefreshEstimateStore Inner { get; set; } = inner;
    public PrimaryRefreshEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRefreshEstimate? Find(string code)
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

    public void Save(PrimaryRefreshEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRefreshEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary refresh estimates. Empty fields match all.</summary>
public sealed record PrimaryRefreshEstimateQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRefreshEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRefreshEstimate> Apply(List<PrimaryRefreshEstimate> items)
    {
        var matched = Collections.Filter(items, (PrimaryRefreshEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary refresh estimate store.</summary>
public static class PrimaryRefreshEstimateStore
{
    public static IPrimaryRefreshEstimateStore NewMemoryPrimaryRefreshEstimateStore()
    {
        return new MemoryPrimaryRefreshEstimateStore(
            new Dictionary<string, PrimaryRefreshEstimate>(),
            new List<string>());
    }

    public static IPrimaryRefreshEstimateStore NewCachedPrimaryRefreshEstimateStore(MemoryPrimaryRefreshEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRefreshEstimateStore(
            inner,
            PrimaryRefreshEstimateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryRefreshEstimate> FindPrimaryRefreshEstimates(IPrimaryRefreshEstimateStore store, PrimaryRefreshEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
