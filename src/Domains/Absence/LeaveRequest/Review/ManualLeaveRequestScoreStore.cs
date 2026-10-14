namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual leave request scores by code.</summary>
public interface IManualLeaveRequestScoreStore
{
    ManualLeaveRequestScore? Find(string code);
    void Save(ManualLeaveRequestScore item);
    bool Remove(string code);
    List<ManualLeaveRequestScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualLeaveRequestScoreStore(Dictionary<string, ManualLeaveRequestScore> items, List<string> order) : IManualLeaveRequestScoreStore
{
    public Dictionary<string, ManualLeaveRequestScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualLeaveRequestScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualLeaveRequestScore item)
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

    public List<ManualLeaveRequestScore> List()
    {
        var outValue = new List<ManualLeaveRequestScore>();
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
public sealed class CachedManualLeaveRequestScoreStore(MemoryManualLeaveRequestScoreStore inner, ManualLeaveRequestScoreCache cache, int hits, int misses) : IManualLeaveRequestScoreStore
{
    public MemoryManualLeaveRequestScoreStore Inner { get; set; } = inner;
    public ManualLeaveRequestScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualLeaveRequestScore? Find(string code)
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

    public void Save(ManualLeaveRequestScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualLeaveRequestScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual leave request scores. Empty fields match all.</summary>
public sealed record ManualLeaveRequestScoreQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualLeaveRequestScore item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ManualLeaveRequestScore> Apply(List<ManualLeaveRequestScore> items)
    {
        var matched = Collections.Filter(items, (ManualLeaveRequestScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual leave request score store.</summary>
public static class ManualLeaveRequestScoreStore
{
    public static IManualLeaveRequestScoreStore NewMemoryManualLeaveRequestScoreStore()
    {
        return new MemoryManualLeaveRequestScoreStore(
            new Dictionary<string, ManualLeaveRequestScore>(),
            new List<string>());
    }

    public static IManualLeaveRequestScoreStore NewCachedManualLeaveRequestScoreStore(MemoryManualLeaveRequestScoreStore inner, int ttl, IClock clock)
    {
        return new CachedManualLeaveRequestScoreStore(
            inner,
            ManualLeaveRequestScoreCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualLeaveRequestScore> FindManualLeaveRequestScores(IManualLeaveRequestScoreStore store, ManualLeaveRequestScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
