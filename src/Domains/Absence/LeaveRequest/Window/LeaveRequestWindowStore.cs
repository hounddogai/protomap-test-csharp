namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request windows by code.</summary>
public interface ILeaveRequestWindowStore
{
    LeaveRequestWindow? Find(string code);
    void Save(LeaveRequestWindow item);
    bool Remove(string code);
    List<LeaveRequestWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestWindowStore(Dictionary<string, LeaveRequestWindow> items, List<string> order) : ILeaveRequestWindowStore
{
    public Dictionary<string, LeaveRequestWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestWindow item)
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

    public List<LeaveRequestWindow> List()
    {
        var outValue = new List<LeaveRequestWindow>();
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
public sealed class CachedLeaveRequestWindowStore(MemoryLeaveRequestWindowStore inner, LeaveRequestWindowCache cache, int hits, int misses) : ILeaveRequestWindowStore
{
    public MemoryLeaveRequestWindowStore Inner { get; set; } = inner;
    public LeaveRequestWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestWindow? Find(string code)
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

    public void Save(LeaveRequestWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request windows. Empty fields match all.</summary>
public sealed record LeaveRequestWindowQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestWindow item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestWindow> Apply(List<LeaveRequestWindow> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request window store.</summary>
public static class LeaveRequestWindowStore
{
    public static ILeaveRequestWindowStore NewMemoryLeaveRequestWindowStore()
    {
        return new MemoryLeaveRequestWindowStore(new Dictionary<string, LeaveRequestWindow>(), new List<string>());
    }

    public static ILeaveRequestWindowStore NewCachedLeaveRequestWindowStore(MemoryLeaveRequestWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestWindowStore(inner, LeaveRequestWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestWindow> FindLeaveRequestWindows(ILeaveRequestWindowStore store, LeaveRequestWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
