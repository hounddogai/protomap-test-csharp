namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary engagement summaries by code.</summary>
public interface IPrimaryEngagementSummaryStore
{
    PrimaryEngagementSummary? Find(string code);
    void Save(PrimaryEngagementSummary item);
    bool Remove(string code);
    List<PrimaryEngagementSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEngagementSummaryStore(Dictionary<string, PrimaryEngagementSummary> items, List<string> order) : IPrimaryEngagementSummaryStore
{
    public Dictionary<string, PrimaryEngagementSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEngagementSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEngagementSummary item)
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

    public List<PrimaryEngagementSummary> List()
    {
        var outValue = new List<PrimaryEngagementSummary>();
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
public sealed class CachedPrimaryEngagementSummaryStore(MemoryPrimaryEngagementSummaryStore inner, PrimaryEngagementSummaryCache cache, int hits, int misses) : IPrimaryEngagementSummaryStore
{
    public MemoryPrimaryEngagementSummaryStore Inner { get; set; } = inner;
    public PrimaryEngagementSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEngagementSummary? Find(string code)
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

    public void Save(PrimaryEngagementSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEngagementSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary engagement summaries. Empty fields match all.</summary>
public sealed record PrimaryEngagementSummaryQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEngagementSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryEngagementSummary> Apply(List<PrimaryEngagementSummary> items)
    {
        var matched = Collections.Filter(items, (PrimaryEngagementSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary engagement summary store.</summary>
public static class PrimaryEngagementSummaryStore
{
    public static IPrimaryEngagementSummaryStore NewMemoryPrimaryEngagementSummaryStore()
    {
        return new MemoryPrimaryEngagementSummaryStore(
            new Dictionary<string, PrimaryEngagementSummary>(),
            new List<string>());
    }

    public static IPrimaryEngagementSummaryStore NewCachedPrimaryEngagementSummaryStore(MemoryPrimaryEngagementSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEngagementSummaryStore(
            inner,
            PrimaryEngagementSummaryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEngagementSummary> FindPrimaryEngagementSummaries(IPrimaryEngagementSummaryStore store, PrimaryEngagementSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
