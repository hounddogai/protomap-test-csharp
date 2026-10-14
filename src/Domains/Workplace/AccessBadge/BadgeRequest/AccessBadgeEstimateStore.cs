namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge estimates by code.</summary>
public interface IAccessBadgeEstimateStore
{
    AccessBadgeEstimate? Find(string code);
    void Save(AccessBadgeEstimate item);
    bool Remove(string code);
    List<AccessBadgeEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeEstimateStore(Dictionary<string, AccessBadgeEstimate> items, List<string> order) : IAccessBadgeEstimateStore
{
    public Dictionary<string, AccessBadgeEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeEstimate item)
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

    public List<AccessBadgeEstimate> List()
    {
        var outValue = new List<AccessBadgeEstimate>();
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
public sealed class CachedAccessBadgeEstimateStore(MemoryAccessBadgeEstimateStore inner, AccessBadgeEstimateCache cache, int hits, int misses) : IAccessBadgeEstimateStore
{
    public MemoryAccessBadgeEstimateStore Inner { get; set; } = inner;
    public AccessBadgeEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeEstimate? Find(string code)
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

    public void Save(AccessBadgeEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge estimates. Empty fields match all.</summary>
public sealed record AccessBadgeEstimateQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AccessBadgeEstimate> Apply(List<AccessBadgeEstimate> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge estimate store.</summary>
public static class AccessBadgeEstimateStore
{
    public static IAccessBadgeEstimateStore NewMemoryAccessBadgeEstimateStore()
    {
        return new MemoryAccessBadgeEstimateStore(new Dictionary<string, AccessBadgeEstimate>(), new List<string>());
    }

    public static IAccessBadgeEstimateStore NewCachedAccessBadgeEstimateStore(MemoryAccessBadgeEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeEstimateStore(inner, AccessBadgeEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeEstimate> FindAccessBadgeEstimates(IAccessBadgeEstimateStore store, AccessBadgeEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
