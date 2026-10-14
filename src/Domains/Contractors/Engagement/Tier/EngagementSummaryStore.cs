namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement summaries by code.</summary>
public interface IEngagementSummaryStore
{
    EngagementSummary? Find(string code);
    void Save(EngagementSummary item);
    bool Remove(string code);
    List<EngagementSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementSummaryStore(Dictionary<string, EngagementSummary> items, List<string> order) : IEngagementSummaryStore
{
    public Dictionary<string, EngagementSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementSummary item)
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

    public List<EngagementSummary> List()
    {
        var outValue = new List<EngagementSummary>();
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
public sealed class CachedEngagementSummaryStore(MemoryEngagementSummaryStore inner, EngagementSummaryCache cache, int hits, int misses) : IEngagementSummaryStore
{
    public MemoryEngagementSummaryStore Inner { get; set; } = inner;
    public EngagementSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementSummary? Find(string code)
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

    public void Save(EngagementSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement summaries. Empty fields match all.</summary>
public sealed record EngagementSummaryQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementSummary> Apply(List<EngagementSummary> items)
    {
        var matched = Collections.Filter(items, (EngagementSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement summary store.</summary>
public static class EngagementSummaryStore
{
    public static IEngagementSummaryStore NewMemoryEngagementSummaryStore()
    {
        return new MemoryEngagementSummaryStore(new Dictionary<string, EngagementSummary>(), new List<string>());
    }

    public static IEngagementSummaryStore NewCachedEngagementSummaryStore(MemoryEngagementSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementSummaryStore(inner, EngagementSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementSummary> FindEngagementSummaries(IEngagementSummaryStore store, EngagementSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
