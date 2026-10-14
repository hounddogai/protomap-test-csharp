namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped cost model assignments by code.</summary>
public interface ICappedCostModelAssignmentStore
{
    CappedCostModelAssignment? Find(string code);
    void Save(CappedCostModelAssignment item);
    bool Remove(string code);
    List<CappedCostModelAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCostModelAssignmentStore(Dictionary<string, CappedCostModelAssignment> items, List<string> order) : ICappedCostModelAssignmentStore
{
    public Dictionary<string, CappedCostModelAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCostModelAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCostModelAssignment item)
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

    public List<CappedCostModelAssignment> List()
    {
        var outValue = new List<CappedCostModelAssignment>();
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
public sealed class CachedCappedCostModelAssignmentStore(MemoryCappedCostModelAssignmentStore inner, CappedCostModelAssignmentCache cache, int hits, int misses) : ICappedCostModelAssignmentStore
{
    public MemoryCappedCostModelAssignmentStore Inner { get; set; } = inner;
    public CappedCostModelAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCostModelAssignment? Find(string code)
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

    public void Save(CappedCostModelAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCostModelAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped cost model assignments. Empty fields match all.</summary>
public sealed record CappedCostModelAssignmentQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCostModelAssignment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedCostModelAssignment> Apply(List<CappedCostModelAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedCostModelAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped cost model assignment store.</summary>
public static class CappedCostModelAssignmentStore
{
    public static ICappedCostModelAssignmentStore NewMemoryCappedCostModelAssignmentStore()
    {
        return new MemoryCappedCostModelAssignmentStore(
            new Dictionary<string, CappedCostModelAssignment>(),
            new List<string>());
    }

    public static ICappedCostModelAssignmentStore NewCachedCappedCostModelAssignmentStore(MemoryCappedCostModelAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCostModelAssignmentStore(
            inner,
            CappedCostModelAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedCostModelAssignment> FindCappedCostModelAssignments(ICappedCostModelAssignmentStore store, CappedCostModelAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
