namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request baselines by code.</summary>
public interface ILeaveRequestBaselineStore
{
    LeaveRequestBaseline? Find(string code);
    void Save(LeaveRequestBaseline item);
    bool Remove(string code);
    List<LeaveRequestBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestBaselineStore(Dictionary<string, LeaveRequestBaseline> items, List<string> order) : ILeaveRequestBaselineStore
{
    public Dictionary<string, LeaveRequestBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestBaseline item)
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

    public List<LeaveRequestBaseline> List()
    {
        var outValue = new List<LeaveRequestBaseline>();
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
public sealed class CachedLeaveRequestBaselineStore(MemoryLeaveRequestBaselineStore inner, LeaveRequestBaselineCache cache, int hits, int misses) : ILeaveRequestBaselineStore
{
    public MemoryLeaveRequestBaselineStore Inner { get; set; } = inner;
    public LeaveRequestBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestBaseline? Find(string code)
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

    public void Save(LeaveRequestBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request baselines. Empty fields match all.</summary>
public sealed record LeaveRequestBaselineQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestBaseline> Apply(List<LeaveRequestBaseline> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request baseline store.</summary>
public static class LeaveRequestBaselineStore
{
    public static ILeaveRequestBaselineStore NewMemoryLeaveRequestBaselineStore()
    {
        return new MemoryLeaveRequestBaselineStore(
            new Dictionary<string, LeaveRequestBaseline>(),
            new List<string>());
    }

    public static ILeaveRequestBaselineStore NewCachedLeaveRequestBaselineStore(MemoryLeaveRequestBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestBaselineStore(inner, LeaveRequestBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestBaseline> FindLeaveRequestBaselines(ILeaveRequestBaselineStore store, LeaveRequestBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
