namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request entries by code.</summary>
public interface ILeaveRequestEntryStore
{
    LeaveRequestEntry? Find(string code);
    void Save(LeaveRequestEntry item);
    bool Remove(string code);
    List<LeaveRequestEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestEntryStore(Dictionary<string, LeaveRequestEntry> items, List<string> order) : ILeaveRequestEntryStore
{
    public Dictionary<string, LeaveRequestEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestEntry item)
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

    public List<LeaveRequestEntry> List()
    {
        var outValue = new List<LeaveRequestEntry>();
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
public sealed class CachedLeaveRequestEntryStore(MemoryLeaveRequestEntryStore inner, LeaveRequestEntryCache cache, int hits, int misses) : ILeaveRequestEntryStore
{
    public MemoryLeaveRequestEntryStore Inner { get; set; } = inner;
    public LeaveRequestEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestEntry? Find(string code)
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

    public void Save(LeaveRequestEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request entries. Empty fields match all.</summary>
public sealed record LeaveRequestEntryQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestEntry item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestEntry> Apply(List<LeaveRequestEntry> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request entry store.</summary>
public static class LeaveRequestEntryStore
{
    public static ILeaveRequestEntryStore NewMemoryLeaveRequestEntryStore()
    {
        return new MemoryLeaveRequestEntryStore(new Dictionary<string, LeaveRequestEntry>(), new List<string>());
    }

    public static ILeaveRequestEntryStore NewCachedLeaveRequestEntryStore(MemoryLeaveRequestEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestEntryStore(inner, LeaveRequestEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestEntry> FindLeaveRequestEntries(ILeaveRequestEntryStore store, LeaveRequestEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
