namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive balance summaries by code.</summary>
public interface IAdaptiveBalanceSummaryStore
{
    AdaptiveBalanceSummary? Find(string code);
    void Save(AdaptiveBalanceSummary item);
    bool Remove(string code);
    List<AdaptiveBalanceSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBalanceSummaryStore(Dictionary<string, AdaptiveBalanceSummary> items, List<string> order) : IAdaptiveBalanceSummaryStore
{
    public Dictionary<string, AdaptiveBalanceSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBalanceSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBalanceSummary item)
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

    public List<AdaptiveBalanceSummary> List()
    {
        var outValue = new List<AdaptiveBalanceSummary>();
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
public sealed class CachedAdaptiveBalanceSummaryStore(MemoryAdaptiveBalanceSummaryStore inner, AdaptiveBalanceSummaryCache cache, int hits, int misses) : IAdaptiveBalanceSummaryStore
{
    public MemoryAdaptiveBalanceSummaryStore Inner { get; set; } = inner;
    public AdaptiveBalanceSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBalanceSummary? Find(string code)
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

    public void Save(AdaptiveBalanceSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBalanceSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive balance summaries. Empty fields match all.</summary>
public sealed record AdaptiveBalanceSummaryQuery(string Team, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBalanceSummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AdaptiveBalanceSummary> Apply(List<AdaptiveBalanceSummary> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBalanceSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive balance summary store.</summary>
public static class AdaptiveBalanceSummaryStore
{
    public static IAdaptiveBalanceSummaryStore NewMemoryAdaptiveBalanceSummaryStore()
    {
        return new MemoryAdaptiveBalanceSummaryStore(
            new Dictionary<string, AdaptiveBalanceSummary>(),
            new List<string>());
    }

    public static IAdaptiveBalanceSummaryStore NewCachedAdaptiveBalanceSummaryStore(MemoryAdaptiveBalanceSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBalanceSummaryStore(
            inner,
            AdaptiveBalanceSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBalanceSummary> FindAdaptiveBalanceSummaries(IAdaptiveBalanceSummaryStore store, AdaptiveBalanceSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
