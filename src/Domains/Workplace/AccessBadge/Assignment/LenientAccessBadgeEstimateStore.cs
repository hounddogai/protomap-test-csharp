namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient access badge estimates by code.</summary>
public interface ILenientAccessBadgeEstimateStore
{
    LenientAccessBadgeEstimate? Find(string code);
    void Save(LenientAccessBadgeEstimate item);
    bool Remove(string code);
    List<LenientAccessBadgeEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAccessBadgeEstimateStore(Dictionary<string, LenientAccessBadgeEstimate> items, List<string> order) : ILenientAccessBadgeEstimateStore
{
    public Dictionary<string, LenientAccessBadgeEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAccessBadgeEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAccessBadgeEstimate item)
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

    public List<LenientAccessBadgeEstimate> List()
    {
        var outValue = new List<LenientAccessBadgeEstimate>();
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
public sealed class CachedLenientAccessBadgeEstimateStore(MemoryLenientAccessBadgeEstimateStore inner, LenientAccessBadgeEstimateCache cache, int hits, int misses) : ILenientAccessBadgeEstimateStore
{
    public MemoryLenientAccessBadgeEstimateStore Inner { get; set; } = inner;
    public LenientAccessBadgeEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAccessBadgeEstimate? Find(string code)
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

    public void Save(LenientAccessBadgeEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAccessBadgeEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient access badge estimates. Empty fields match all.</summary>
public sealed record LenientAccessBadgeEstimateQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAccessBadgeEstimate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAccessBadgeEstimate> Apply(List<LenientAccessBadgeEstimate> items)
    {
        var matched = Collections.Filter(items, (LenientAccessBadgeEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient access badge estimate store.</summary>
public static class LenientAccessBadgeEstimateStore
{
    public static ILenientAccessBadgeEstimateStore NewMemoryLenientAccessBadgeEstimateStore()
    {
        return new MemoryLenientAccessBadgeEstimateStore(
            new Dictionary<string, LenientAccessBadgeEstimate>(),
            new List<string>());
    }

    public static ILenientAccessBadgeEstimateStore NewCachedLenientAccessBadgeEstimateStore(MemoryLenientAccessBadgeEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAccessBadgeEstimateStore(
            inner,
            LenientAccessBadgeEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientAccessBadgeEstimate> FindLenientAccessBadgeEstimates(ILenientAccessBadgeEstimateStore store, LenientAccessBadgeEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
