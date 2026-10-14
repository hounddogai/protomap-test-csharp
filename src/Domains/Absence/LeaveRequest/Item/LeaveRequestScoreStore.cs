namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request scores by code.</summary>
public interface ILeaveRequestScoreStore
{
    LeaveRequestScore? Find(string code);
    void Save(LeaveRequestScore item);
    bool Remove(string code);
    List<LeaveRequestScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestScoreStore(Dictionary<string, LeaveRequestScore> items, List<string> order) : ILeaveRequestScoreStore
{
    public Dictionary<string, LeaveRequestScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestScore item)
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

    public List<LeaveRequestScore> List()
    {
        var outValue = new List<LeaveRequestScore>();
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
public sealed class CachedLeaveRequestScoreStore(MemoryLeaveRequestScoreStore inner, LeaveRequestScoreCache cache, int hits, int misses) : ILeaveRequestScoreStore
{
    public MemoryLeaveRequestScoreStore Inner { get; set; } = inner;
    public LeaveRequestScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestScore? Find(string code)
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

    public void Save(LeaveRequestScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request scores. Empty fields match all.</summary>
public sealed record LeaveRequestScoreQuery(string Source, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestScore item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<LeaveRequestScore> Apply(List<LeaveRequestScore> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request score store.</summary>
public static class LeaveRequestScoreStore
{
    public static ILeaveRequestScoreStore NewMemoryLeaveRequestScoreStore()
    {
        return new MemoryLeaveRequestScoreStore(new Dictionary<string, LeaveRequestScore>(), new List<string>());
    }

    public static ILeaveRequestScoreStore NewCachedLeaveRequestScoreStore(MemoryLeaveRequestScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestScoreStore(inner, LeaveRequestScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestScore> FindLeaveRequestScores(ILeaveRequestScoreStore store, LeaveRequestScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
