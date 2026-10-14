namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment windows by code.</summary>
public interface IAssignmentWindowStore
{
    AssignmentWindow? Find(string code);
    void Save(AssignmentWindow item);
    bool Remove(string code);
    List<AssignmentWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentWindowStore(Dictionary<string, AssignmentWindow> items, List<string> order) : IAssignmentWindowStore
{
    public Dictionary<string, AssignmentWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentWindow item)
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

    public List<AssignmentWindow> List()
    {
        var outValue = new List<AssignmentWindow>();
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
public sealed class CachedAssignmentWindowStore(MemoryAssignmentWindowStore inner, AssignmentWindowCache cache, int hits, int misses) : IAssignmentWindowStore
{
    public MemoryAssignmentWindowStore Inner { get; set; } = inner;
    public AssignmentWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentWindow? Find(string code)
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

    public void Save(AssignmentWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment windows. Empty fields match all.</summary>
public sealed record AssignmentWindowQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentWindow item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AssignmentWindow> Apply(List<AssignmentWindow> items)
    {
        var matched = Collections.Filter(items, (AssignmentWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment window store.</summary>
public static class AssignmentWindowStore
{
    public static IAssignmentWindowStore NewMemoryAssignmentWindowStore()
    {
        return new MemoryAssignmentWindowStore(new Dictionary<string, AssignmentWindow>(), new List<string>());
    }

    public static IAssignmentWindowStore NewCachedAssignmentWindowStore(MemoryAssignmentWindowStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentWindowStore(inner, AssignmentWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentWindow> FindAssignmentWindows(IAssignmentWindowStore store, AssignmentWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
