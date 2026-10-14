namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor summaries by code.</summary>
public interface IVisitorSummaryStore
{
    VisitorSummary? Find(string code);
    void Save(VisitorSummary item);
    bool Remove(string code);
    List<VisitorSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorSummaryStore(Dictionary<string, VisitorSummary> items, List<string> order) : IVisitorSummaryStore
{
    public Dictionary<string, VisitorSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorSummary item)
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

    public List<VisitorSummary> List()
    {
        var outValue = new List<VisitorSummary>();
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
public sealed class CachedVisitorSummaryStore(MemoryVisitorSummaryStore inner, VisitorSummaryCache cache, int hits, int misses) : IVisitorSummaryStore
{
    public MemoryVisitorSummaryStore Inner { get; set; } = inner;
    public VisitorSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorSummary? Find(string code)
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

    public void Save(VisitorSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor summaries. Empty fields match all.</summary>
public sealed record VisitorSummaryQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorSummary item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorSummary> Apply(List<VisitorSummary> items)
    {
        var matched = Collections.Filter(items, (VisitorSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor summary store.</summary>
public static class VisitorSummaryStore
{
    public static IVisitorSummaryStore NewMemoryVisitorSummaryStore()
    {
        return new MemoryVisitorSummaryStore(new Dictionary<string, VisitorSummary>(), new List<string>());
    }

    public static IVisitorSummaryStore NewCachedVisitorSummaryStore(MemoryVisitorSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorSummaryStore(inner, VisitorSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorSummary> FindVisitorSummaries(IVisitorSummaryStore store, VisitorSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
