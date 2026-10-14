namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual visitor summaries by code.</summary>
public interface IManualVisitorSummaryStore
{
    ManualVisitorSummary? Find(string code);
    void Save(ManualVisitorSummary item);
    bool Remove(string code);
    List<ManualVisitorSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualVisitorSummaryStore(Dictionary<string, ManualVisitorSummary> items, List<string> order) : IManualVisitorSummaryStore
{
    public Dictionary<string, ManualVisitorSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualVisitorSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualVisitorSummary item)
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

    public List<ManualVisitorSummary> List()
    {
        var outValue = new List<ManualVisitorSummary>();
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
public sealed class CachedManualVisitorSummaryStore(MemoryManualVisitorSummaryStore inner, ManualVisitorSummaryCache cache, int hits, int misses) : IManualVisitorSummaryStore
{
    public MemoryManualVisitorSummaryStore Inner { get; set; } = inner;
    public ManualVisitorSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualVisitorSummary? Find(string code)
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

    public void Save(ManualVisitorSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualVisitorSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual visitor summaries. Empty fields match all.</summary>
public sealed record ManualVisitorSummaryQuery(string Channel, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ManualVisitorSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualVisitorSummary> Apply(List<ManualVisitorSummary> items)
    {
        var matched = Collections.Filter(items, (ManualVisitorSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual visitor summary store.</summary>
public static class ManualVisitorSummaryStore
{
    public static IManualVisitorSummaryStore NewMemoryManualVisitorSummaryStore()
    {
        return new MemoryManualVisitorSummaryStore(
            new Dictionary<string, ManualVisitorSummary>(),
            new List<string>());
    }

    public static IManualVisitorSummaryStore NewCachedManualVisitorSummaryStore(MemoryManualVisitorSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedManualVisitorSummaryStore(inner, ManualVisitorSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualVisitorSummary> FindManualVisitorSummaries(IManualVisitorSummaryStore store, ManualVisitorSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
