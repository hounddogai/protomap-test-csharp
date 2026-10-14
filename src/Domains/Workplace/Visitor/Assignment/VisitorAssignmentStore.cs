namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor assignments by code.</summary>
public interface IVisitorAssignmentStore
{
    VisitorAssignment? Find(string code);
    void Save(VisitorAssignment item);
    bool Remove(string code);
    List<VisitorAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorAssignmentStore(Dictionary<string, VisitorAssignment> items, List<string> order) : IVisitorAssignmentStore
{
    public Dictionary<string, VisitorAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorAssignment item)
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

    public List<VisitorAssignment> List()
    {
        var outValue = new List<VisitorAssignment>();
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
public sealed class CachedVisitorAssignmentStore(MemoryVisitorAssignmentStore inner, VisitorAssignmentCache cache, int hits, int misses) : IVisitorAssignmentStore
{
    public MemoryVisitorAssignmentStore Inner { get; set; } = inner;
    public VisitorAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorAssignment? Find(string code)
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

    public void Save(VisitorAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor assignments. Empty fields match all.</summary>
public sealed record VisitorAssignmentQuery(string Category, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorAssignment> Apply(List<VisitorAssignment> items)
    {
        var matched = Collections.Filter(items, (VisitorAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor assignment store.</summary>
public static class VisitorAssignmentStore
{
    public static IVisitorAssignmentStore NewMemoryVisitorAssignmentStore()
    {
        return new MemoryVisitorAssignmentStore(new Dictionary<string, VisitorAssignment>(), new List<string>());
    }

    public static IVisitorAssignmentStore NewCachedVisitorAssignmentStore(MemoryVisitorAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorAssignmentStore(inner, VisitorAssignmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VisitorAssignment> FindVisitorAssignments(IVisitorAssignmentStore store, VisitorAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
