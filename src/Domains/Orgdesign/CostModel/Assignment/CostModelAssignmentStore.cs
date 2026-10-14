namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model assignments by code.</summary>
public interface ICostModelAssignmentStore
{
    CostModelAssignment? Find(string code);
    void Save(CostModelAssignment item);
    bool Remove(string code);
    List<CostModelAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelAssignmentStore(Dictionary<string, CostModelAssignment> items, List<string> order) : ICostModelAssignmentStore
{
    public Dictionary<string, CostModelAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelAssignment item)
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

    public List<CostModelAssignment> List()
    {
        var outValue = new List<CostModelAssignment>();
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
public sealed class CachedCostModelAssignmentStore(MemoryCostModelAssignmentStore inner, CostModelAssignmentCache cache, int hits, int misses) : ICostModelAssignmentStore
{
    public MemoryCostModelAssignmentStore Inner { get; set; } = inner;
    public CostModelAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelAssignment? Find(string code)
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

    public void Save(CostModelAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model assignments. Empty fields match all.</summary>
public sealed record CostModelAssignmentQuery(string Team, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelAssignment> Apply(List<CostModelAssignment> items)
    {
        var matched = Collections.Filter(items, (CostModelAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model assignment store.</summary>
public static class CostModelAssignmentStore
{
    public static ICostModelAssignmentStore NewMemoryCostModelAssignmentStore()
    {
        return new MemoryCostModelAssignmentStore(new Dictionary<string, CostModelAssignment>(), new List<string>());
    }

    public static ICostModelAssignmentStore NewCachedCostModelAssignmentStore(MemoryCostModelAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelAssignmentStore(inner, CostModelAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CostModelAssignment> FindCostModelAssignments(ICostModelAssignmentStore store, CostModelAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
