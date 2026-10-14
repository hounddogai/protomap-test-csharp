namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk summaries by code.</summary>
public interface IDeskSummaryStore
{
    DeskSummary? Find(string code);
    void Save(DeskSummary item);
    bool Remove(string code);
    List<DeskSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskSummaryStore(Dictionary<string, DeskSummary> items, List<string> order) : IDeskSummaryStore
{
    public Dictionary<string, DeskSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskSummary item)
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

    public List<DeskSummary> List()
    {
        var outValue = new List<DeskSummary>();
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
public sealed class CachedDeskSummaryStore(MemoryDeskSummaryStore inner, DeskSummaryCache cache, int hits, int misses) : IDeskSummaryStore
{
    public MemoryDeskSummaryStore Inner { get; set; } = inner;
    public DeskSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskSummary? Find(string code)
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

    public void Save(DeskSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk summaries. Empty fields match all.</summary>
public sealed record DeskSummaryQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeskSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskSummary> Apply(List<DeskSummary> items)
    {
        var matched = Collections.Filter(items, (DeskSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk summary store.</summary>
public static class DeskSummaryStore
{
    public static IDeskSummaryStore NewMemoryDeskSummaryStore()
    {
        return new MemoryDeskSummaryStore(new Dictionary<string, DeskSummary>(), new List<string>());
    }

    public static IDeskSummaryStore NewCachedDeskSummaryStore(MemoryDeskSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedDeskSummaryStore(inner, DeskSummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeskSummary> FindDeskSummaries(IDeskSummaryStore store, DeskSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
