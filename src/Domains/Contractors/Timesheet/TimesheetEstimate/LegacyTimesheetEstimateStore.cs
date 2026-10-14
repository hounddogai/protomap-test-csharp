namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy timesheet estimates by code.</summary>
public interface ILegacyTimesheetEstimateStore
{
    LegacyTimesheetEstimate? Find(string code);
    void Save(LegacyTimesheetEstimate item);
    bool Remove(string code);
    List<LegacyTimesheetEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTimesheetEstimateStore(Dictionary<string, LegacyTimesheetEstimate> items, List<string> order) : ILegacyTimesheetEstimateStore
{
    public Dictionary<string, LegacyTimesheetEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTimesheetEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTimesheetEstimate item)
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

    public List<LegacyTimesheetEstimate> List()
    {
        var outValue = new List<LegacyTimesheetEstimate>();
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
public sealed class CachedLegacyTimesheetEstimateStore(MemoryLegacyTimesheetEstimateStore inner, LegacyTimesheetEstimateCache cache, int hits, int misses) : ILegacyTimesheetEstimateStore
{
    public MemoryLegacyTimesheetEstimateStore Inner { get; set; } = inner;
    public LegacyTimesheetEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTimesheetEstimate? Find(string code)
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

    public void Save(LegacyTimesheetEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTimesheetEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy timesheet estimates. Empty fields match all.</summary>
public sealed record LegacyTimesheetEstimateQuery(string Country, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTimesheetEstimate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<LegacyTimesheetEstimate> Apply(List<LegacyTimesheetEstimate> items)
    {
        var matched = Collections.Filter(items, (LegacyTimesheetEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy timesheet estimate store.</summary>
public static class LegacyTimesheetEstimateStore
{
    public static ILegacyTimesheetEstimateStore NewMemoryLegacyTimesheetEstimateStore()
    {
        return new MemoryLegacyTimesheetEstimateStore(
            new Dictionary<string, LegacyTimesheetEstimate>(),
            new List<string>());
    }

    public static ILegacyTimesheetEstimateStore NewCachedLegacyTimesheetEstimateStore(MemoryLegacyTimesheetEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTimesheetEstimateStore(
            inner,
            LegacyTimesheetEstimateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTimesheetEstimate> FindLegacyTimesheetEstimates(ILegacyTimesheetEstimateStore store, LegacyTimesheetEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
