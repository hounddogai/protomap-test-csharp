namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback warranty summaries by code.</summary>
public interface IFallbackWarrantySummaryStore
{
    FallbackWarrantySummary? Find(string code);
    void Save(FallbackWarrantySummary item);
    bool Remove(string code);
    List<FallbackWarrantySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackWarrantySummaryStore(Dictionary<string, FallbackWarrantySummary> items, List<string> order) : IFallbackWarrantySummaryStore
{
    public Dictionary<string, FallbackWarrantySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackWarrantySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackWarrantySummary item)
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

    public List<FallbackWarrantySummary> List()
    {
        var outValue = new List<FallbackWarrantySummary>();
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
public sealed class CachedFallbackWarrantySummaryStore(MemoryFallbackWarrantySummaryStore inner, FallbackWarrantySummaryCache cache, int hits, int misses) : IFallbackWarrantySummaryStore
{
    public MemoryFallbackWarrantySummaryStore Inner { get; set; } = inner;
    public FallbackWarrantySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackWarrantySummary? Find(string code)
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

    public void Save(FallbackWarrantySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackWarrantySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback warranty summaries. Empty fields match all.</summary>
public sealed record FallbackWarrantySummaryQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackWarrantySummary item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackWarrantySummary> Apply(List<FallbackWarrantySummary> items)
    {
        var matched = Collections.Filter(items, (FallbackWarrantySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback warranty summary store.</summary>
public static class FallbackWarrantySummaryStore
{
    public static IFallbackWarrantySummaryStore NewMemoryFallbackWarrantySummaryStore()
    {
        return new MemoryFallbackWarrantySummaryStore(
            new Dictionary<string, FallbackWarrantySummary>(),
            new List<string>());
    }

    public static IFallbackWarrantySummaryStore NewCachedFallbackWarrantySummaryStore(MemoryFallbackWarrantySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackWarrantySummaryStore(
            inner,
            FallbackWarrantySummaryCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackWarrantySummary> FindFallbackWarrantySummaries(IFallbackWarrantySummaryStore store, FallbackWarrantySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
