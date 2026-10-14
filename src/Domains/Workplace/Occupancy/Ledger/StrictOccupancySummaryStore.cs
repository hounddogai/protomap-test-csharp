namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict occupancy summaries by code.</summary>
public interface IStrictOccupancySummaryStore
{
    StrictOccupancySummary? Find(string code);
    void Save(StrictOccupancySummary item);
    bool Remove(string code);
    List<StrictOccupancySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOccupancySummaryStore(Dictionary<string, StrictOccupancySummary> items, List<string> order) : IStrictOccupancySummaryStore
{
    public Dictionary<string, StrictOccupancySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOccupancySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOccupancySummary item)
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

    public List<StrictOccupancySummary> List()
    {
        var outValue = new List<StrictOccupancySummary>();
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
public sealed class CachedStrictOccupancySummaryStore(MemoryStrictOccupancySummaryStore inner, StrictOccupancySummaryCache cache, int hits, int misses) : IStrictOccupancySummaryStore
{
    public MemoryStrictOccupancySummaryStore Inner { get; set; } = inner;
    public StrictOccupancySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOccupancySummary? Find(string code)
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

    public void Save(StrictOccupancySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOccupancySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict occupancy summaries. Empty fields match all.</summary>
public sealed record StrictOccupancySummaryQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOccupancySummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOccupancySummary> Apply(List<StrictOccupancySummary> items)
    {
        var matched = Collections.Filter(items, (StrictOccupancySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict occupancy summary store.</summary>
public static class StrictOccupancySummaryStore
{
    public static IStrictOccupancySummaryStore NewMemoryStrictOccupancySummaryStore()
    {
        return new MemoryStrictOccupancySummaryStore(
            new Dictionary<string, StrictOccupancySummary>(),
            new List<string>());
    }

    public static IStrictOccupancySummaryStore NewCachedStrictOccupancySummaryStore(MemoryStrictOccupancySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOccupancySummaryStore(
            inner,
            StrictOccupancySummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictOccupancySummary> FindStrictOccupancySummaries(IStrictOccupancySummaryStore store, StrictOccupancySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
