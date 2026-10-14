namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict access badge assignments by code.</summary>
public interface IStrictAccessBadgeAssignmentStore
{
    StrictAccessBadgeAssignment? Find(string code);
    void Save(StrictAccessBadgeAssignment item);
    bool Remove(string code);
    List<StrictAccessBadgeAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAccessBadgeAssignmentStore(Dictionary<string, StrictAccessBadgeAssignment> items, List<string> order) : IStrictAccessBadgeAssignmentStore
{
    public Dictionary<string, StrictAccessBadgeAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAccessBadgeAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAccessBadgeAssignment item)
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

    public List<StrictAccessBadgeAssignment> List()
    {
        var outValue = new List<StrictAccessBadgeAssignment>();
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
public sealed class CachedStrictAccessBadgeAssignmentStore(MemoryStrictAccessBadgeAssignmentStore inner, StrictAccessBadgeAssignmentCache cache, int hits, int misses) : IStrictAccessBadgeAssignmentStore
{
    public MemoryStrictAccessBadgeAssignmentStore Inner { get; set; } = inner;
    public StrictAccessBadgeAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAccessBadgeAssignment? Find(string code)
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

    public void Save(StrictAccessBadgeAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAccessBadgeAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict access badge assignments. Empty fields match all.</summary>
public sealed record StrictAccessBadgeAssignmentQuery(string Team, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAccessBadgeAssignment item)
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

    public List<StrictAccessBadgeAssignment> Apply(List<StrictAccessBadgeAssignment> items)
    {
        var matched = Collections.Filter(items, (StrictAccessBadgeAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict access badge assignment store.</summary>
public static class StrictAccessBadgeAssignmentStore
{
    public static IStrictAccessBadgeAssignmentStore NewMemoryStrictAccessBadgeAssignmentStore()
    {
        return new MemoryStrictAccessBadgeAssignmentStore(
            new Dictionary<string, StrictAccessBadgeAssignment>(),
            new List<string>());
    }

    public static IStrictAccessBadgeAssignmentStore NewCachedStrictAccessBadgeAssignmentStore(MemoryStrictAccessBadgeAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAccessBadgeAssignmentStore(
            inner,
            StrictAccessBadgeAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictAccessBadgeAssignment> FindStrictAccessBadgeAssignments(IStrictAccessBadgeAssignmentStore store, StrictAccessBadgeAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
