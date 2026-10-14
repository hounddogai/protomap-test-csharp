namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request groups by code.</summary>
public interface ILeaveRequestGroupStore
{
    LeaveRequestGroup? Find(string code);
    void Save(LeaveRequestGroup item);
    bool Remove(string code);
    List<LeaveRequestGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestGroupStore(Dictionary<string, LeaveRequestGroup> items, List<string> order) : ILeaveRequestGroupStore
{
    public Dictionary<string, LeaveRequestGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestGroup item)
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

    public List<LeaveRequestGroup> List()
    {
        var outValue = new List<LeaveRequestGroup>();
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
public sealed class CachedLeaveRequestGroupStore(MemoryLeaveRequestGroupStore inner, LeaveRequestGroupCache cache, int hits, int misses) : ILeaveRequestGroupStore
{
    public MemoryLeaveRequestGroupStore Inner { get; set; } = inner;
    public LeaveRequestGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestGroup? Find(string code)
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

    public void Save(LeaveRequestGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request groups. Empty fields match all.</summary>
public sealed record LeaveRequestGroupQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestGroup> Apply(List<LeaveRequestGroup> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request group store.</summary>
public static class LeaveRequestGroupStore
{
    public static ILeaveRequestGroupStore NewMemoryLeaveRequestGroupStore()
    {
        return new MemoryLeaveRequestGroupStore(new Dictionary<string, LeaveRequestGroup>(), new List<string>());
    }

    public static ILeaveRequestGroupStore NewCachedLeaveRequestGroupStore(MemoryLeaveRequestGroupStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestGroupStore(inner, LeaveRequestGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestGroup> FindLeaveRequestGroups(ILeaveRequestGroupStore store, LeaveRequestGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
