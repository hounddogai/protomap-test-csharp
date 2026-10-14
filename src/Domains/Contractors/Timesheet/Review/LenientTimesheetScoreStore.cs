namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient timesheet scores by code.</summary>
public interface ILenientTimesheetScoreStore
{
    LenientTimesheetScore? Find(string code);
    void Save(LenientTimesheetScore item);
    bool Remove(string code);
    List<LenientTimesheetScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTimesheetScoreStore(Dictionary<string, LenientTimesheetScore> items, List<string> order) : ILenientTimesheetScoreStore
{
    public Dictionary<string, LenientTimesheetScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTimesheetScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTimesheetScore item)
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

    public List<LenientTimesheetScore> List()
    {
        var outValue = new List<LenientTimesheetScore>();
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
public sealed class CachedLenientTimesheetScoreStore(MemoryLenientTimesheetScoreStore inner, LenientTimesheetScoreCache cache, int hits, int misses) : ILenientTimesheetScoreStore
{
    public MemoryLenientTimesheetScoreStore Inner { get; set; } = inner;
    public LenientTimesheetScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTimesheetScore? Find(string code)
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

    public void Save(LenientTimesheetScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTimesheetScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient timesheet scores. Empty fields match all.</summary>
public sealed record LenientTimesheetScoreQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTimesheetScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTimesheetScore> Apply(List<LenientTimesheetScore> items)
    {
        var matched = Collections.Filter(items, (LenientTimesheetScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient timesheet score store.</summary>
public static class LenientTimesheetScoreStore
{
    public static ILenientTimesheetScoreStore NewMemoryLenientTimesheetScoreStore()
    {
        return new MemoryLenientTimesheetScoreStore(
            new Dictionary<string, LenientTimesheetScore>(),
            new List<string>());
    }

    public static ILenientTimesheetScoreStore NewCachedLenientTimesheetScoreStore(MemoryLenientTimesheetScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTimesheetScoreStore(
            inner,
            LenientTimesheetScoreCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientTimesheetScore> FindLenientTimesheetScores(ILenientTimesheetScoreStore store, LenientTimesheetScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
