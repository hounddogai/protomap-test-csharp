namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped engagement summaries by code.</summary>
public interface ICappedEngagementSummaryStore
{
    CappedEngagementSummary? Find(string code);
    void Save(CappedEngagementSummary item);
    bool Remove(string code);
    List<CappedEngagementSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedEngagementSummaryStore(Dictionary<string, CappedEngagementSummary> items, List<string> order) : ICappedEngagementSummaryStore
{
    public Dictionary<string, CappedEngagementSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedEngagementSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedEngagementSummary item)
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

    public List<CappedEngagementSummary> List()
    {
        var outValue = new List<CappedEngagementSummary>();
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
public sealed class CachedCappedEngagementSummaryStore(MemoryCappedEngagementSummaryStore inner, CappedEngagementSummaryCache cache, int hits, int misses) : ICappedEngagementSummaryStore
{
    public MemoryCappedEngagementSummaryStore Inner { get; set; } = inner;
    public CappedEngagementSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedEngagementSummary? Find(string code)
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

    public void Save(CappedEngagementSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedEngagementSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped engagement summaries. Empty fields match all.</summary>
public sealed record CappedEngagementSummaryQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CappedEngagementSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedEngagementSummary> Apply(List<CappedEngagementSummary> items)
    {
        var matched = Collections.Filter(items, (CappedEngagementSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped engagement summary store.</summary>
public static class CappedEngagementSummaryStore
{
    public static ICappedEngagementSummaryStore NewMemoryCappedEngagementSummaryStore()
    {
        return new MemoryCappedEngagementSummaryStore(
            new Dictionary<string, CappedEngagementSummary>(),
            new List<string>());
    }

    public static ICappedEngagementSummaryStore NewCachedCappedEngagementSummaryStore(MemoryCappedEngagementSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedCappedEngagementSummaryStore(
            inner,
            CappedEngagementSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedEngagementSummary> FindCappedEngagementSummaries(ICappedEngagementSummaryStore store, CappedEngagementSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
