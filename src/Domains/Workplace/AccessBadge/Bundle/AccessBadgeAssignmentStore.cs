namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge assignments by code.</summary>
public interface IAccessBadgeAssignmentStore
{
    AccessBadgeAssignment? Find(string code);
    void Save(AccessBadgeAssignment item);
    bool Remove(string code);
    List<AccessBadgeAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeAssignmentStore(Dictionary<string, AccessBadgeAssignment> items, List<string> order) : IAccessBadgeAssignmentStore
{
    public Dictionary<string, AccessBadgeAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeAssignment item)
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

    public List<AccessBadgeAssignment> List()
    {
        var outValue = new List<AccessBadgeAssignment>();
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
public sealed class CachedAccessBadgeAssignmentStore(MemoryAccessBadgeAssignmentStore inner, AccessBadgeAssignmentCache cache, int hits, int misses) : IAccessBadgeAssignmentStore
{
    public MemoryAccessBadgeAssignmentStore Inner { get; set; } = inner;
    public AccessBadgeAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeAssignment? Find(string code)
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

    public void Save(AccessBadgeAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge assignments. Empty fields match all.</summary>
public sealed record AccessBadgeAssignmentQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeAssignment> Apply(List<AccessBadgeAssignment> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge assignment store.</summary>
public static class AccessBadgeAssignmentStore
{
    public static IAccessBadgeAssignmentStore NewMemoryAccessBadgeAssignmentStore()
    {
        return new MemoryAccessBadgeAssignmentStore(
            new Dictionary<string, AccessBadgeAssignment>(),
            new List<string>());
    }

    public static IAccessBadgeAssignmentStore NewCachedAccessBadgeAssignmentStore(MemoryAccessBadgeAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeAssignmentStore(inner, AccessBadgeAssignmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeAssignment> FindAccessBadgeAssignments(IAccessBadgeAssignmentStore store, AccessBadgeAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
