namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional policy assignments by code.</summary>
public interface IRegionalPolicyAssignmentStore
{
    RegionalPolicyAssignment? Find(string code);
    void Save(RegionalPolicyAssignment item);
    bool Remove(string code);
    List<RegionalPolicyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPolicyAssignmentStore(Dictionary<string, RegionalPolicyAssignment> items, List<string> order) : IRegionalPolicyAssignmentStore
{
    public Dictionary<string, RegionalPolicyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPolicyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPolicyAssignment item)
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

    public List<RegionalPolicyAssignment> List()
    {
        var outValue = new List<RegionalPolicyAssignment>();
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
public sealed class CachedRegionalPolicyAssignmentStore(MemoryRegionalPolicyAssignmentStore inner, RegionalPolicyAssignmentCache cache, int hits, int misses) : IRegionalPolicyAssignmentStore
{
    public MemoryRegionalPolicyAssignmentStore Inner { get; set; } = inner;
    public RegionalPolicyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPolicyAssignment? Find(string code)
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

    public void Save(RegionalPolicyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPolicyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional policy assignments. Empty fields match all.</summary>
public sealed record RegionalPolicyAssignmentQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPolicyAssignment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPolicyAssignment> Apply(List<RegionalPolicyAssignment> items)
    {
        var matched = Collections.Filter(items, (RegionalPolicyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional policy assignment store.</summary>
public static class RegionalPolicyAssignmentStore
{
    public static IRegionalPolicyAssignmentStore NewMemoryRegionalPolicyAssignmentStore()
    {
        return new MemoryRegionalPolicyAssignmentStore(
            new Dictionary<string, RegionalPolicyAssignment>(),
            new List<string>());
    }

    public static IRegionalPolicyAssignmentStore NewCachedRegionalPolicyAssignmentStore(MemoryRegionalPolicyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPolicyAssignmentStore(
            inner,
            RegionalPolicyAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPolicyAssignment> FindRegionalPolicyAssignments(IRegionalPolicyAssignmentStore store, RegionalPolicyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
