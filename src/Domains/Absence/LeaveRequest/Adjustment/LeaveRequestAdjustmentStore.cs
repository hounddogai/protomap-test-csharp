namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request adjustments by code.</summary>
public interface ILeaveRequestAdjustmentStore
{
    LeaveRequestAdjustment? Find(string code);
    void Save(LeaveRequestAdjustment item);
    bool Remove(string code);
    List<LeaveRequestAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestAdjustmentStore(Dictionary<string, LeaveRequestAdjustment> items, List<string> order) : ILeaveRequestAdjustmentStore
{
    public Dictionary<string, LeaveRequestAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestAdjustment item)
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

    public List<LeaveRequestAdjustment> List()
    {
        var outValue = new List<LeaveRequestAdjustment>();
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
public sealed class CachedLeaveRequestAdjustmentStore(MemoryLeaveRequestAdjustmentStore inner, LeaveRequestAdjustmentCache cache, int hits, int misses) : ILeaveRequestAdjustmentStore
{
    public MemoryLeaveRequestAdjustmentStore Inner { get; set; } = inner;
    public LeaveRequestAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestAdjustment? Find(string code)
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

    public void Save(LeaveRequestAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request adjustments. Empty fields match all.</summary>
public sealed record LeaveRequestAdjustmentQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestAdjustment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestAdjustment> Apply(List<LeaveRequestAdjustment> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request adjustment store.</summary>
public static class LeaveRequestAdjustmentStore
{
    public static ILeaveRequestAdjustmentStore NewMemoryLeaveRequestAdjustmentStore()
    {
        return new MemoryLeaveRequestAdjustmentStore(
            new Dictionary<string, LeaveRequestAdjustment>(),
            new List<string>());
    }

    public static ILeaveRequestAdjustmentStore NewCachedLeaveRequestAdjustmentStore(MemoryLeaveRequestAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestAdjustmentStore(
            inner,
            LeaveRequestAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LeaveRequestAdjustment> FindLeaveRequestAdjustments(ILeaveRequestAdjustmentStore store, LeaveRequestAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
