namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling leave request adjustments by code.</summary>
public interface IRollingLeaveRequestAdjustmentStore
{
    RollingLeaveRequestAdjustment? Find(string code);
    void Save(RollingLeaveRequestAdjustment item);
    bool Remove(string code);
    List<RollingLeaveRequestAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLeaveRequestAdjustmentStore(Dictionary<string, RollingLeaveRequestAdjustment> items, List<string> order) : IRollingLeaveRequestAdjustmentStore
{
    public Dictionary<string, RollingLeaveRequestAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLeaveRequestAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLeaveRequestAdjustment item)
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

    public List<RollingLeaveRequestAdjustment> List()
    {
        var outValue = new List<RollingLeaveRequestAdjustment>();
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
public sealed class CachedRollingLeaveRequestAdjustmentStore(MemoryRollingLeaveRequestAdjustmentStore inner, RollingLeaveRequestAdjustmentCache cache, int hits, int misses) : IRollingLeaveRequestAdjustmentStore
{
    public MemoryRollingLeaveRequestAdjustmentStore Inner { get; set; } = inner;
    public RollingLeaveRequestAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLeaveRequestAdjustment? Find(string code)
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

    public void Save(RollingLeaveRequestAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLeaveRequestAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling leave request adjustments. Empty fields match all.</summary>
public sealed record RollingLeaveRequestAdjustmentQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLeaveRequestAdjustment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingLeaveRequestAdjustment> Apply(List<RollingLeaveRequestAdjustment> items)
    {
        var matched = Collections.Filter(items, (RollingLeaveRequestAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling leave request adjustment store.</summary>
public static class RollingLeaveRequestAdjustmentStore
{
    public static IRollingLeaveRequestAdjustmentStore NewMemoryRollingLeaveRequestAdjustmentStore()
    {
        return new MemoryRollingLeaveRequestAdjustmentStore(
            new Dictionary<string, RollingLeaveRequestAdjustment>(),
            new List<string>());
    }

    public static IRollingLeaveRequestAdjustmentStore NewCachedRollingLeaveRequestAdjustmentStore(MemoryRollingLeaveRequestAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLeaveRequestAdjustmentStore(
            inner,
            RollingLeaveRequestAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingLeaveRequestAdjustment> FindRollingLeaveRequestAdjustments(IRollingLeaveRequestAdjustmentStore store, RollingLeaveRequestAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
