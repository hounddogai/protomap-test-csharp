namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict timesheet estimates by code.</summary>
public interface IStrictTimesheetEstimateStore
{
    StrictTimesheetEstimate? Find(string code);
    void Save(StrictTimesheetEstimate item);
    bool Remove(string code);
    List<StrictTimesheetEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictTimesheetEstimateStore(Dictionary<string, StrictTimesheetEstimate> items, List<string> order) : IStrictTimesheetEstimateStore
{
    public Dictionary<string, StrictTimesheetEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictTimesheetEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictTimesheetEstimate item)
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

    public List<StrictTimesheetEstimate> List()
    {
        var outValue = new List<StrictTimesheetEstimate>();
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
public sealed class CachedStrictTimesheetEstimateStore(MemoryStrictTimesheetEstimateStore inner, StrictTimesheetEstimateCache cache, int hits, int misses) : IStrictTimesheetEstimateStore
{
    public MemoryStrictTimesheetEstimateStore Inner { get; set; } = inner;
    public StrictTimesheetEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictTimesheetEstimate? Find(string code)
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

    public void Save(StrictTimesheetEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictTimesheetEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict timesheet estimates. Empty fields match all.</summary>
public sealed record StrictTimesheetEstimateQuery(string Region, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(StrictTimesheetEstimate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictTimesheetEstimate> Apply(List<StrictTimesheetEstimate> items)
    {
        var matched = Collections.Filter(items, (StrictTimesheetEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict timesheet estimate store.</summary>
public static class StrictTimesheetEstimateStore
{
    public static IStrictTimesheetEstimateStore NewMemoryStrictTimesheetEstimateStore()
    {
        return new MemoryStrictTimesheetEstimateStore(
            new Dictionary<string, StrictTimesheetEstimate>(),
            new List<string>());
    }

    public static IStrictTimesheetEstimateStore NewCachedStrictTimesheetEstimateStore(MemoryStrictTimesheetEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictTimesheetEstimateStore(
            inner,
            StrictTimesheetEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictTimesheetEstimate> FindStrictTimesheetEstimates(IStrictTimesheetEstimateStore store, StrictTimesheetEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
