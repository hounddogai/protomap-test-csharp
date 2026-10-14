namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request preferences by code.</summary>
public interface ILeaveRequestPreferenceStore
{
    LeaveRequestPreference? Find(string code);
    void Save(LeaveRequestPreference item);
    bool Remove(string code);
    List<LeaveRequestPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestPreferenceStore(Dictionary<string, LeaveRequestPreference> items, List<string> order) : ILeaveRequestPreferenceStore
{
    public Dictionary<string, LeaveRequestPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestPreference item)
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

    public List<LeaveRequestPreference> List()
    {
        var outValue = new List<LeaveRequestPreference>();
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
public sealed class CachedLeaveRequestPreferenceStore(MemoryLeaveRequestPreferenceStore inner, LeaveRequestPreferenceCache cache, int hits, int misses) : ILeaveRequestPreferenceStore
{
    public MemoryLeaveRequestPreferenceStore Inner { get; set; } = inner;
    public LeaveRequestPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestPreference? Find(string code)
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

    public void Save(LeaveRequestPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request preferences. Empty fields match all.</summary>
public sealed record LeaveRequestPreferenceQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestPreference> Apply(List<LeaveRequestPreference> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request preference store.</summary>
public static class LeaveRequestPreferenceStore
{
    public static ILeaveRequestPreferenceStore NewMemoryLeaveRequestPreferenceStore()
    {
        return new MemoryLeaveRequestPreferenceStore(
            new Dictionary<string, LeaveRequestPreference>(),
            new List<string>());
    }

    public static ILeaveRequestPreferenceStore NewCachedLeaveRequestPreferenceStore(MemoryLeaveRequestPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestPreferenceStore(
            inner,
            LeaveRequestPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LeaveRequestPreference> FindLeaveRequestPreferences(ILeaveRequestPreferenceStore store, LeaveRequestPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
