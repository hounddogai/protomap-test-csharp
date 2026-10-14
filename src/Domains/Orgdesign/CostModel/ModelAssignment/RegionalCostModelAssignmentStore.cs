namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional cost model assignments by code.</summary>
public interface IRegionalCostModelAssignmentStore
{
    RegionalCostModelAssignment? Find(string code);
    void Save(RegionalCostModelAssignment item);
    bool Remove(string code);
    List<RegionalCostModelAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalCostModelAssignmentStore(Dictionary<string, RegionalCostModelAssignment> items, List<string> order) : IRegionalCostModelAssignmentStore
{
    public Dictionary<string, RegionalCostModelAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalCostModelAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalCostModelAssignment item)
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

    public List<RegionalCostModelAssignment> List()
    {
        var outValue = new List<RegionalCostModelAssignment>();
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
public sealed class CachedRegionalCostModelAssignmentStore(MemoryRegionalCostModelAssignmentStore inner, RegionalCostModelAssignmentCache cache, int hits, int misses) : IRegionalCostModelAssignmentStore
{
    public MemoryRegionalCostModelAssignmentStore Inner { get; set; } = inner;
    public RegionalCostModelAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalCostModelAssignment? Find(string code)
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

    public void Save(RegionalCostModelAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalCostModelAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional cost model assignments. Empty fields match all.</summary>
public sealed record RegionalCostModelAssignmentQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalCostModelAssignment item)
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

    public List<RegionalCostModelAssignment> Apply(List<RegionalCostModelAssignment> items)
    {
        var matched = Collections.Filter(items, (RegionalCostModelAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional cost model assignment store.</summary>
public static class RegionalCostModelAssignmentStore
{
    public static IRegionalCostModelAssignmentStore NewMemoryRegionalCostModelAssignmentStore()
    {
        return new MemoryRegionalCostModelAssignmentStore(
            new Dictionary<string, RegionalCostModelAssignment>(),
            new List<string>());
    }

    public static IRegionalCostModelAssignmentStore NewCachedRegionalCostModelAssignmentStore(MemoryRegionalCostModelAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalCostModelAssignmentStore(
            inner,
            RegionalCostModelAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalCostModelAssignment> FindRegionalCostModelAssignments(IRegionalCostModelAssignmentStore store, RegionalCostModelAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
