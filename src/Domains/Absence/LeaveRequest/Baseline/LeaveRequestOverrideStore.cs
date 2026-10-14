namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request overrides by code.</summary>
public interface ILeaveRequestOverrideStore
{
    LeaveRequestOverride? Find(string code);
    void Save(LeaveRequestOverride item);
    bool Remove(string code);
    List<LeaveRequestOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestOverrideStore(Dictionary<string, LeaveRequestOverride> items, List<string> order) : ILeaveRequestOverrideStore
{
    public Dictionary<string, LeaveRequestOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestOverride item)
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

    public List<LeaveRequestOverride> List()
    {
        var outValue = new List<LeaveRequestOverride>();
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
public sealed class CachedLeaveRequestOverrideStore(MemoryLeaveRequestOverrideStore inner, LeaveRequestOverrideCache cache, int hits, int misses) : ILeaveRequestOverrideStore
{
    public MemoryLeaveRequestOverrideStore Inner { get; set; } = inner;
    public LeaveRequestOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestOverride? Find(string code)
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

    public void Save(LeaveRequestOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request overrides. Empty fields match all.</summary>
public sealed record LeaveRequestOverrideQuery(string Channel, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestOverride item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestOverride> Apply(List<LeaveRequestOverride> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request override store.</summary>
public static class LeaveRequestOverrideStore
{
    public static ILeaveRequestOverrideStore NewMemoryLeaveRequestOverrideStore()
    {
        return new MemoryLeaveRequestOverrideStore(
            new Dictionary<string, LeaveRequestOverride>(),
            new List<string>());
    }

    public static ILeaveRequestOverrideStore NewCachedLeaveRequestOverrideStore(MemoryLeaveRequestOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestOverrideStore(inner, LeaveRequestOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestOverride> FindLeaveRequestOverrides(ILeaveRequestOverrideStore store, LeaveRequestOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
