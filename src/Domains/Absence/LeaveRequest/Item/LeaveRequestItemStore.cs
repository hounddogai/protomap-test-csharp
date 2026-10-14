namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request items by code.</summary>
public interface ILeaveRequestItemStore
{
    LeaveRequestItem? Find(string code);
    void Save(LeaveRequestItem item);
    bool Remove(string code);
    List<LeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestItemStore(Dictionary<string, LeaveRequestItem> items, List<string> order) : ILeaveRequestItemStore
{
    public Dictionary<string, LeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestItem item)
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

    public List<LeaveRequestItem> List()
    {
        var outValue = new List<LeaveRequestItem>();
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
public sealed class CachedLeaveRequestItemStore(MemoryLeaveRequestItemStore inner, LeaveRequestItemCache cache, int hits, int misses) : ILeaveRequestItemStore
{
    public MemoryLeaveRequestItemStore Inner { get; set; } = inner;
    public LeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestItem? Find(string code)
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

    public void Save(LeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request items. Empty fields match all.</summary>
public sealed record LeaveRequestItemQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestItem item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestItem> Apply(List<LeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request item store.</summary>
public static class LeaveRequestItemStore
{
    public static ILeaveRequestItemStore NewMemoryLeaveRequestItemStore()
    {
        return new MemoryLeaveRequestItemStore(new Dictionary<string, LeaveRequestItem>(), new List<string>());
    }

    public static ILeaveRequestItemStore NewCachedLeaveRequestItemStore(MemoryLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestItemStore(inner, LeaveRequestItemCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestItem> FindLeaveRequestItems(ILeaveRequestItemStore store, LeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
