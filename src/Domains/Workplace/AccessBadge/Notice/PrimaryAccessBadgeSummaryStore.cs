namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary access badge summaries by code.</summary>
public interface IPrimaryAccessBadgeSummaryStore
{
    PrimaryAccessBadgeSummary? Find(string code);
    void Save(PrimaryAccessBadgeSummary item);
    bool Remove(string code);
    List<PrimaryAccessBadgeSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccessBadgeSummaryStore(Dictionary<string, PrimaryAccessBadgeSummary> items, List<string> order) : IPrimaryAccessBadgeSummaryStore
{
    public Dictionary<string, PrimaryAccessBadgeSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccessBadgeSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccessBadgeSummary item)
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

    public List<PrimaryAccessBadgeSummary> List()
    {
        var outValue = new List<PrimaryAccessBadgeSummary>();
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
public sealed class CachedPrimaryAccessBadgeSummaryStore(MemoryPrimaryAccessBadgeSummaryStore inner, PrimaryAccessBadgeSummaryCache cache, int hits, int misses) : IPrimaryAccessBadgeSummaryStore
{
    public MemoryPrimaryAccessBadgeSummaryStore Inner { get; set; } = inner;
    public PrimaryAccessBadgeSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccessBadgeSummary? Find(string code)
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

    public void Save(PrimaryAccessBadgeSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccessBadgeSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary access badge summaries. Empty fields match all.</summary>
public sealed record PrimaryAccessBadgeSummaryQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccessBadgeSummary item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PrimaryAccessBadgeSummary> Apply(List<PrimaryAccessBadgeSummary> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccessBadgeSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary access badge summary store.</summary>
public static class PrimaryAccessBadgeSummaryStore
{
    public static IPrimaryAccessBadgeSummaryStore NewMemoryPrimaryAccessBadgeSummaryStore()
    {
        return new MemoryPrimaryAccessBadgeSummaryStore(
            new Dictionary<string, PrimaryAccessBadgeSummary>(),
            new List<string>());
    }

    public static IPrimaryAccessBadgeSummaryStore NewCachedPrimaryAccessBadgeSummaryStore(MemoryPrimaryAccessBadgeSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccessBadgeSummaryStore(
            inner,
            PrimaryAccessBadgeSummaryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAccessBadgeSummary> FindPrimaryAccessBadgeSummaries(IPrimaryAccessBadgeSummaryStore store, PrimaryAccessBadgeSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
