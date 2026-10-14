namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient equity grant summaries by code.</summary>
public interface ILenientEquityGrantSummaryStore
{
    LenientEquityGrantSummary? Find(string code);
    void Save(LenientEquityGrantSummary item);
    bool Remove(string code);
    List<LenientEquityGrantSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientEquityGrantSummaryStore(Dictionary<string, LenientEquityGrantSummary> items, List<string> order) : ILenientEquityGrantSummaryStore
{
    public Dictionary<string, LenientEquityGrantSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientEquityGrantSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientEquityGrantSummary item)
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

    public List<LenientEquityGrantSummary> List()
    {
        var outValue = new List<LenientEquityGrantSummary>();
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
public sealed class CachedLenientEquityGrantSummaryStore(MemoryLenientEquityGrantSummaryStore inner, LenientEquityGrantSummaryCache cache, int hits, int misses) : ILenientEquityGrantSummaryStore
{
    public MemoryLenientEquityGrantSummaryStore Inner { get; set; } = inner;
    public LenientEquityGrantSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientEquityGrantSummary? Find(string code)
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

    public void Save(LenientEquityGrantSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientEquityGrantSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient equity grant summaries. Empty fields match all.</summary>
public sealed record LenientEquityGrantSummaryQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LenientEquityGrantSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientEquityGrantSummary> Apply(List<LenientEquityGrantSummary> items)
    {
        var matched = Collections.Filter(items, (LenientEquityGrantSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient equity grant summary store.</summary>
public static class LenientEquityGrantSummaryStore
{
    public static ILenientEquityGrantSummaryStore NewMemoryLenientEquityGrantSummaryStore()
    {
        return new MemoryLenientEquityGrantSummaryStore(
            new Dictionary<string, LenientEquityGrantSummary>(),
            new List<string>());
    }

    public static ILenientEquityGrantSummaryStore NewCachedLenientEquityGrantSummaryStore(MemoryLenientEquityGrantSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientEquityGrantSummaryStore(
            inner,
            LenientEquityGrantSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientEquityGrantSummary> FindLenientEquityGrantSummaries(ILenientEquityGrantSummaryStore store, LenientEquityGrantSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
