namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending position estimates by code.</summary>
public interface IPendingPositionEstimateStore
{
    PendingPositionEstimate? Find(string code);
    void Save(PendingPositionEstimate item);
    bool Remove(string code);
    List<PendingPositionEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPositionEstimateStore(Dictionary<string, PendingPositionEstimate> items, List<string> order) : IPendingPositionEstimateStore
{
    public Dictionary<string, PendingPositionEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPositionEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPositionEstimate item)
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

    public List<PendingPositionEstimate> List()
    {
        var outValue = new List<PendingPositionEstimate>();
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
public sealed class CachedPendingPositionEstimateStore(MemoryPendingPositionEstimateStore inner, PendingPositionEstimateCache cache, int hits, int misses) : IPendingPositionEstimateStore
{
    public MemoryPendingPositionEstimateStore Inner { get; set; } = inner;
    public PendingPositionEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPositionEstimate? Find(string code)
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

    public void Save(PendingPositionEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPositionEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending position estimates. Empty fields match all.</summary>
public sealed record PendingPositionEstimateQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPositionEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPositionEstimate> Apply(List<PendingPositionEstimate> items)
    {
        var matched = Collections.Filter(items, (PendingPositionEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending position estimate store.</summary>
public static class PendingPositionEstimateStore
{
    public static IPendingPositionEstimateStore NewMemoryPendingPositionEstimateStore()
    {
        return new MemoryPendingPositionEstimateStore(
            new Dictionary<string, PendingPositionEstimate>(),
            new List<string>());
    }

    public static IPendingPositionEstimateStore NewCachedPendingPositionEstimateStore(MemoryPendingPositionEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPositionEstimateStore(
            inner,
            PendingPositionEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingPositionEstimate> FindPendingPositionEstimates(IPendingPositionEstimateStore store, PendingPositionEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
