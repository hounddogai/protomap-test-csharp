namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg summaries by code.</summary>
public interface IReorgSummaryStore
{
    ReorgSummary? Find(string code);
    void Save(ReorgSummary item);
    bool Remove(string code);
    List<ReorgSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgSummaryStore(Dictionary<string, ReorgSummary> items, List<string> order) : IReorgSummaryStore
{
    public Dictionary<string, ReorgSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgSummary item)
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

    public List<ReorgSummary> List()
    {
        var outValue = new List<ReorgSummary>();
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
public sealed class CachedReorgSummaryStore(MemoryReorgSummaryStore inner, ReorgSummaryCache cache, int hits, int misses) : IReorgSummaryStore
{
    public MemoryReorgSummaryStore Inner { get; set; } = inner;
    public ReorgSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgSummary? Find(string code)
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

    public void Save(ReorgSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg summaries. Empty fields match all.</summary>
public sealed record ReorgSummaryQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgSummary> Apply(List<ReorgSummary> items)
    {
        var matched = Collections.Filter(items, (ReorgSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg summary store.</summary>
public static class ReorgSummaryStore
{
    public static IReorgSummaryStore NewMemoryReorgSummaryStore()
    {
        return new MemoryReorgSummaryStore(new Dictionary<string, ReorgSummary>(), new List<string>());
    }

    public static IReorgSummaryStore NewCachedReorgSummaryStore(MemoryReorgSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedReorgSummaryStore(inner, ReorgSummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReorgSummary> FindReorgSummaries(IReorgSummaryStore store, ReorgSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
