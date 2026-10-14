namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling accrual assignments by code.</summary>
public interface IRollingAccrualAssignmentStore
{
    RollingAccrualAssignment? Find(string code);
    void Save(RollingAccrualAssignment item);
    bool Remove(string code);
    List<RollingAccrualAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccrualAssignmentStore(Dictionary<string, RollingAccrualAssignment> items, List<string> order) : IRollingAccrualAssignmentStore
{
    public Dictionary<string, RollingAccrualAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccrualAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccrualAssignment item)
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

    public List<RollingAccrualAssignment> List()
    {
        var outValue = new List<RollingAccrualAssignment>();
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
public sealed class CachedRollingAccrualAssignmentStore(MemoryRollingAccrualAssignmentStore inner, RollingAccrualAssignmentCache cache, int hits, int misses) : IRollingAccrualAssignmentStore
{
    public MemoryRollingAccrualAssignmentStore Inner { get; set; } = inner;
    public RollingAccrualAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccrualAssignment? Find(string code)
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

    public void Save(RollingAccrualAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccrualAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling accrual assignments. Empty fields match all.</summary>
public sealed record RollingAccrualAssignmentQuery(string Unit, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccrualAssignment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAccrualAssignment> Apply(List<RollingAccrualAssignment> items)
    {
        var matched = Collections.Filter(items, (RollingAccrualAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling accrual assignment store.</summary>
public static class RollingAccrualAssignmentStore
{
    public static IRollingAccrualAssignmentStore NewMemoryRollingAccrualAssignmentStore()
    {
        return new MemoryRollingAccrualAssignmentStore(
            new Dictionary<string, RollingAccrualAssignment>(),
            new List<string>());
    }

    public static IRollingAccrualAssignmentStore NewCachedRollingAccrualAssignmentStore(MemoryRollingAccrualAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccrualAssignmentStore(
            inner,
            RollingAccrualAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingAccrualAssignment> FindRollingAccrualAssignments(IRollingAccrualAssignmentStore store, RollingAccrualAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
