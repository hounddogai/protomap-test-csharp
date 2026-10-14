namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient retro pay summaries by code.</summary>
public interface ILenientRetroPaySummaryStore
{
    LenientRetroPaySummary? Find(string code);
    void Save(LenientRetroPaySummary item);
    bool Remove(string code);
    List<LenientRetroPaySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRetroPaySummaryStore(Dictionary<string, LenientRetroPaySummary> items, List<string> order) : ILenientRetroPaySummaryStore
{
    public Dictionary<string, LenientRetroPaySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRetroPaySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRetroPaySummary item)
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

    public List<LenientRetroPaySummary> List()
    {
        var outValue = new List<LenientRetroPaySummary>();
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
public sealed class CachedLenientRetroPaySummaryStore(MemoryLenientRetroPaySummaryStore inner, LenientRetroPaySummaryCache cache, int hits, int misses) : ILenientRetroPaySummaryStore
{
    public MemoryLenientRetroPaySummaryStore Inner { get; set; } = inner;
    public LenientRetroPaySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRetroPaySummary? Find(string code)
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

    public void Save(LenientRetroPaySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRetroPaySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient retro pay summaries. Empty fields match all.</summary>
public sealed record LenientRetroPaySummaryQuery(string Country, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRetroPaySummary item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientRetroPaySummary> Apply(List<LenientRetroPaySummary> items)
    {
        var matched = Collections.Filter(items, (LenientRetroPaySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient retro pay summary store.</summary>
public static class LenientRetroPaySummaryStore
{
    public static ILenientRetroPaySummaryStore NewMemoryLenientRetroPaySummaryStore()
    {
        return new MemoryLenientRetroPaySummaryStore(
            new Dictionary<string, LenientRetroPaySummary>(),
            new List<string>());
    }

    public static ILenientRetroPaySummaryStore NewCachedLenientRetroPaySummaryStore(MemoryLenientRetroPaySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRetroPaySummaryStore(
            inner,
            LenientRetroPaySummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientRetroPaySummary> FindLenientRetroPaySummaries(ILenientRetroPaySummaryStore store, LenientRetroPaySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
