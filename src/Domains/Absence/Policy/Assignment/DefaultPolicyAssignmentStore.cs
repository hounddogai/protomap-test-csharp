namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default policy assignments by code.</summary>
public interface IDefaultPolicyAssignmentStore
{
    DefaultPolicyAssignment? Find(string code);
    void Save(DefaultPolicyAssignment item);
    bool Remove(string code);
    List<DefaultPolicyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPolicyAssignmentStore(Dictionary<string, DefaultPolicyAssignment> items, List<string> order) : IDefaultPolicyAssignmentStore
{
    public Dictionary<string, DefaultPolicyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPolicyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPolicyAssignment item)
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

    public List<DefaultPolicyAssignment> List()
    {
        var outValue = new List<DefaultPolicyAssignment>();
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
public sealed class CachedDefaultPolicyAssignmentStore(MemoryDefaultPolicyAssignmentStore inner, DefaultPolicyAssignmentCache cache, int hits, int misses) : IDefaultPolicyAssignmentStore
{
    public MemoryDefaultPolicyAssignmentStore Inner { get; set; } = inner;
    public DefaultPolicyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPolicyAssignment? Find(string code)
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

    public void Save(DefaultPolicyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPolicyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default policy assignments. Empty fields match all.</summary>
public sealed record DefaultPolicyAssignmentQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPolicyAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<DefaultPolicyAssignment> Apply(List<DefaultPolicyAssignment> items)
    {
        var matched = Collections.Filter(items, (DefaultPolicyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default policy assignment store.</summary>
public static class DefaultPolicyAssignmentStore
{
    public static IDefaultPolicyAssignmentStore NewMemoryDefaultPolicyAssignmentStore()
    {
        return new MemoryDefaultPolicyAssignmentStore(
            new Dictionary<string, DefaultPolicyAssignment>(),
            new List<string>());
    }

    public static IDefaultPolicyAssignmentStore NewCachedDefaultPolicyAssignmentStore(MemoryDefaultPolicyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPolicyAssignmentStore(
            inner,
            DefaultPolicyAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultPolicyAssignment> FindDefaultPolicyAssignments(IDefaultPolicyAssignmentStore store, DefaultPolicyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
