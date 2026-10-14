namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge summaries by code.</summary>
public interface IAccessBadgeSummaryStore
{
    AccessBadgeSummary? Find(string code);
    void Save(AccessBadgeSummary item);
    bool Remove(string code);
    List<AccessBadgeSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeSummaryStore(Dictionary<string, AccessBadgeSummary> items, List<string> order) : IAccessBadgeSummaryStore
{
    public Dictionary<string, AccessBadgeSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeSummary item)
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

    public List<AccessBadgeSummary> List()
    {
        var outValue = new List<AccessBadgeSummary>();
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
public sealed class CachedAccessBadgeSummaryStore(MemoryAccessBadgeSummaryStore inner, AccessBadgeSummaryCache cache, int hits, int misses) : IAccessBadgeSummaryStore
{
    public MemoryAccessBadgeSummaryStore Inner { get; set; } = inner;
    public AccessBadgeSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeSummary? Find(string code)
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

    public void Save(AccessBadgeSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge summaries. Empty fields match all.</summary>
public sealed record AccessBadgeSummaryQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AccessBadgeSummary> Apply(List<AccessBadgeSummary> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge summary store.</summary>
public static class AccessBadgeSummaryStore
{
    public static IAccessBadgeSummaryStore NewMemoryAccessBadgeSummaryStore()
    {
        return new MemoryAccessBadgeSummaryStore(new Dictionary<string, AccessBadgeSummary>(), new List<string>());
    }

    public static IAccessBadgeSummaryStore NewCachedAccessBadgeSummaryStore(MemoryAccessBadgeSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeSummaryStore(inner, AccessBadgeSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeSummary> FindAccessBadgeSummaries(IAccessBadgeSummaryStore store, AccessBadgeSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
