namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment items by code.</summary>
public interface IAssignmentItemStore
{
    AssignmentItem? Find(string code);
    void Save(AssignmentItem item);
    bool Remove(string code);
    List<AssignmentItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentItemStore(Dictionary<string, AssignmentItem> items, List<string> order) : IAssignmentItemStore
{
    public Dictionary<string, AssignmentItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentItem item)
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

    public List<AssignmentItem> List()
    {
        var outValue = new List<AssignmentItem>();
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
public sealed class CachedAssignmentItemStore(MemoryAssignmentItemStore inner, AssignmentItemCache cache, int hits, int misses) : IAssignmentItemStore
{
    public MemoryAssignmentItemStore Inner { get; set; } = inner;
    public AssignmentItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentItem? Find(string code)
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

    public void Save(AssignmentItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment items. Empty fields match all.</summary>
public sealed record AssignmentItemQuery(string Category, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentItem item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentItem> Apply(List<AssignmentItem> items)
    {
        var matched = Collections.Filter(items, (AssignmentItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment item store.</summary>
public static class AssignmentItemStore
{
    public static IAssignmentItemStore NewMemoryAssignmentItemStore()
    {
        return new MemoryAssignmentItemStore(new Dictionary<string, AssignmentItem>(), new List<string>());
    }

    public static IAssignmentItemStore NewCachedAssignmentItemStore(MemoryAssignmentItemStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentItemStore(inner, AssignmentItemCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentItem> FindAssignmentItems(IAssignmentItemStore store, AssignmentItemQuery query)
    {
        return query.Apply(store.List());
    }
}
