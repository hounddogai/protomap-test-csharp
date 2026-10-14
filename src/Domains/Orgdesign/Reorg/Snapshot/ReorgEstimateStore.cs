namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg estimates by code.</summary>
public interface IReorgEstimateStore
{
    ReorgEstimate? Find(string code);
    void Save(ReorgEstimate item);
    bool Remove(string code);
    List<ReorgEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgEstimateStore(Dictionary<string, ReorgEstimate> items, List<string> order) : IReorgEstimateStore
{
    public Dictionary<string, ReorgEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgEstimate item)
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

    public List<ReorgEstimate> List()
    {
        var outValue = new List<ReorgEstimate>();
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
public sealed class CachedReorgEstimateStore(MemoryReorgEstimateStore inner, ReorgEstimateCache cache, int hits, int misses) : IReorgEstimateStore
{
    public MemoryReorgEstimateStore Inner { get; set; } = inner;
    public ReorgEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgEstimate? Find(string code)
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

    public void Save(ReorgEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg estimates. Empty fields match all.</summary>
public sealed record ReorgEstimateQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgEstimate> Apply(List<ReorgEstimate> items)
    {
        var matched = Collections.Filter(items, (ReorgEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg estimate store.</summary>
public static class ReorgEstimateStore
{
    public static IReorgEstimateStore NewMemoryReorgEstimateStore()
    {
        return new MemoryReorgEstimateStore(new Dictionary<string, ReorgEstimate>(), new List<string>());
    }

    public static IReorgEstimateStore NewCachedReorgEstimateStore(MemoryReorgEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedReorgEstimateStore(inner, ReorgEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgEstimate> FindReorgEstimates(IReorgEstimateStore store, ReorgEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
