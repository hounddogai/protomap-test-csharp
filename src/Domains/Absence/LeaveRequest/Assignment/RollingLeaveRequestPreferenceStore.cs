namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling leave request preferences by code.</summary>
public interface IRollingLeaveRequestPreferenceStore
{
    RollingLeaveRequestPreference? Find(string code);
    void Save(RollingLeaveRequestPreference item);
    bool Remove(string code);
    List<RollingLeaveRequestPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLeaveRequestPreferenceStore(Dictionary<string, RollingLeaveRequestPreference> items, List<string> order) : IRollingLeaveRequestPreferenceStore
{
    public Dictionary<string, RollingLeaveRequestPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLeaveRequestPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLeaveRequestPreference item)
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

    public List<RollingLeaveRequestPreference> List()
    {
        var outValue = new List<RollingLeaveRequestPreference>();
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
public sealed class CachedRollingLeaveRequestPreferenceStore(MemoryRollingLeaveRequestPreferenceStore inner, RollingLeaveRequestPreferenceCache cache, int hits, int misses) : IRollingLeaveRequestPreferenceStore
{
    public MemoryRollingLeaveRequestPreferenceStore Inner { get; set; } = inner;
    public RollingLeaveRequestPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLeaveRequestPreference? Find(string code)
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

    public void Save(RollingLeaveRequestPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLeaveRequestPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling leave request preferences. Empty fields match all.</summary>
public sealed record RollingLeaveRequestPreferenceQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLeaveRequestPreference item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<RollingLeaveRequestPreference> Apply(List<RollingLeaveRequestPreference> items)
    {
        var matched = Collections.Filter(items, (RollingLeaveRequestPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling leave request preference store.</summary>
public static class RollingLeaveRequestPreferenceStore
{
    public static IRollingLeaveRequestPreferenceStore NewMemoryRollingLeaveRequestPreferenceStore()
    {
        return new MemoryRollingLeaveRequestPreferenceStore(
            new Dictionary<string, RollingLeaveRequestPreference>(),
            new List<string>());
    }

    public static IRollingLeaveRequestPreferenceStore NewCachedRollingLeaveRequestPreferenceStore(MemoryRollingLeaveRequestPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLeaveRequestPreferenceStore(
            inner,
            RollingLeaveRequestPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingLeaveRequestPreference> FindRollingLeaveRequestPreferences(IRollingLeaveRequestPreferenceStore store, RollingLeaveRequestPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
