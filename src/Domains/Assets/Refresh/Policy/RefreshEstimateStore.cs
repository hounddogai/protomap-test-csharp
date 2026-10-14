namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh estimates by code.</summary>
public interface IRefreshEstimateStore
{
    RefreshEstimate? Find(string code);
    void Save(RefreshEstimate item);
    bool Remove(string code);
    List<RefreshEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshEstimateStore(Dictionary<string, RefreshEstimate> items, List<string> order) : IRefreshEstimateStore
{
    public Dictionary<string, RefreshEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshEstimate item)
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

    public List<RefreshEstimate> List()
    {
        var outValue = new List<RefreshEstimate>();
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
public sealed class CachedRefreshEstimateStore(MemoryRefreshEstimateStore inner, RefreshEstimateCache cache, int hits, int misses) : IRefreshEstimateStore
{
    public MemoryRefreshEstimateStore Inner { get; set; } = inner;
    public RefreshEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshEstimate? Find(string code)
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

    public void Save(RefreshEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh estimates. Empty fields match all.</summary>
public sealed record RefreshEstimateQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshEstimate> Apply(List<RefreshEstimate> items)
    {
        var matched = Collections.Filter(items, (RefreshEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh estimate store.</summary>
public static class RefreshEstimateStore
{
    public static IRefreshEstimateStore NewMemoryRefreshEstimateStore()
    {
        return new MemoryRefreshEstimateStore(new Dictionary<string, RefreshEstimate>(), new List<string>());
    }

    public static IRefreshEstimateStore NewCachedRefreshEstimateStore(MemoryRefreshEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshEstimateStore(inner, RefreshEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshEstimate> FindRefreshEstimates(IRefreshEstimateStore store, RefreshEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
