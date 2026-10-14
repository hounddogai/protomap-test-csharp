namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global engagement assignments by code.</summary>
public interface IGlobalEngagementAssignmentStore
{
    GlobalEngagementAssignment? Find(string code);
    void Save(GlobalEngagementAssignment item);
    bool Remove(string code);
    List<GlobalEngagementAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEngagementAssignmentStore(Dictionary<string, GlobalEngagementAssignment> items, List<string> order) : IGlobalEngagementAssignmentStore
{
    public Dictionary<string, GlobalEngagementAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEngagementAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEngagementAssignment item)
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

    public List<GlobalEngagementAssignment> List()
    {
        var outValue = new List<GlobalEngagementAssignment>();
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
public sealed class CachedGlobalEngagementAssignmentStore(MemoryGlobalEngagementAssignmentStore inner, GlobalEngagementAssignmentCache cache, int hits, int misses) : IGlobalEngagementAssignmentStore
{
    public MemoryGlobalEngagementAssignmentStore Inner { get; set; } = inner;
    public GlobalEngagementAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEngagementAssignment? Find(string code)
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

    public void Save(GlobalEngagementAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEngagementAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global engagement assignments. Empty fields match all.</summary>
public sealed record GlobalEngagementAssignmentQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEngagementAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalEngagementAssignment> Apply(List<GlobalEngagementAssignment> items)
    {
        var matched = Collections.Filter(items, (GlobalEngagementAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global engagement assignment store.</summary>
public static class GlobalEngagementAssignmentStore
{
    public static IGlobalEngagementAssignmentStore NewMemoryGlobalEngagementAssignmentStore()
    {
        return new MemoryGlobalEngagementAssignmentStore(
            new Dictionary<string, GlobalEngagementAssignment>(),
            new List<string>());
    }

    public static IGlobalEngagementAssignmentStore NewCachedGlobalEngagementAssignmentStore(MemoryGlobalEngagementAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEngagementAssignmentStore(
            inner,
            GlobalEngagementAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEngagementAssignment> FindGlobalEngagementAssignments(IGlobalEngagementAssignmentStore store, GlobalEngagementAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
