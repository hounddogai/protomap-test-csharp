namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback access badge assignments by code.</summary>
public interface IFallbackAccessBadgeAssignmentStore
{
    FallbackAccessBadgeAssignment? Find(string code);
    void Save(FallbackAccessBadgeAssignment item);
    bool Remove(string code);
    List<FallbackAccessBadgeAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAccessBadgeAssignmentStore(Dictionary<string, FallbackAccessBadgeAssignment> items, List<string> order) : IFallbackAccessBadgeAssignmentStore
{
    public Dictionary<string, FallbackAccessBadgeAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAccessBadgeAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAccessBadgeAssignment item)
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

    public List<FallbackAccessBadgeAssignment> List()
    {
        var outValue = new List<FallbackAccessBadgeAssignment>();
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
public sealed class CachedFallbackAccessBadgeAssignmentStore(MemoryFallbackAccessBadgeAssignmentStore inner, FallbackAccessBadgeAssignmentCache cache, int hits, int misses) : IFallbackAccessBadgeAssignmentStore
{
    public MemoryFallbackAccessBadgeAssignmentStore Inner { get; set; } = inner;
    public FallbackAccessBadgeAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAccessBadgeAssignment? Find(string code)
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

    public void Save(FallbackAccessBadgeAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAccessBadgeAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback access badge assignments. Empty fields match all.</summary>
public sealed record FallbackAccessBadgeAssignmentQuery(string Region, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAccessBadgeAssignment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<FallbackAccessBadgeAssignment> Apply(List<FallbackAccessBadgeAssignment> items)
    {
        var matched = Collections.Filter(items, (FallbackAccessBadgeAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback access badge assignment store.</summary>
public static class FallbackAccessBadgeAssignmentStore
{
    public static IFallbackAccessBadgeAssignmentStore NewMemoryFallbackAccessBadgeAssignmentStore()
    {
        return new MemoryFallbackAccessBadgeAssignmentStore(
            new Dictionary<string, FallbackAccessBadgeAssignment>(),
            new List<string>());
    }

    public static IFallbackAccessBadgeAssignmentStore NewCachedFallbackAccessBadgeAssignmentStore(MemoryFallbackAccessBadgeAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAccessBadgeAssignmentStore(
            inner,
            FallbackAccessBadgeAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAccessBadgeAssignment> FindFallbackAccessBadgeAssignments(IFallbackAccessBadgeAssignmentStore store, FallbackAccessBadgeAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
