namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped timesheet thresholds by code.</summary>
public interface ICappedTimesheetThresholdStore
{
    CappedTimesheetThreshold? Find(string code);
    void Save(CappedTimesheetThreshold item);
    bool Remove(string code);
    List<CappedTimesheetThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedTimesheetThresholdStore(Dictionary<string, CappedTimesheetThreshold> items, List<string> order) : ICappedTimesheetThresholdStore
{
    public Dictionary<string, CappedTimesheetThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedTimesheetThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedTimesheetThreshold item)
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

    public List<CappedTimesheetThreshold> List()
    {
        var outValue = new List<CappedTimesheetThreshold>();
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
public sealed class CachedCappedTimesheetThresholdStore(MemoryCappedTimesheetThresholdStore inner, CappedTimesheetThresholdCache cache, int hits, int misses) : ICappedTimesheetThresholdStore
{
    public MemoryCappedTimesheetThresholdStore Inner { get; set; } = inner;
    public CappedTimesheetThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedTimesheetThreshold? Find(string code)
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

    public void Save(CappedTimesheetThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedTimesheetThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped timesheet thresholds. Empty fields match all.</summary>
public sealed record CappedTimesheetThresholdQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CappedTimesheetThreshold item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedTimesheetThreshold> Apply(List<CappedTimesheetThreshold> items)
    {
        var matched = Collections.Filter(items, (CappedTimesheetThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped timesheet threshold store.</summary>
public static class CappedTimesheetThresholdStore
{
    public static ICappedTimesheetThresholdStore NewMemoryCappedTimesheetThresholdStore()
    {
        return new MemoryCappedTimesheetThresholdStore(
            new Dictionary<string, CappedTimesheetThreshold>(),
            new List<string>());
    }

    public static ICappedTimesheetThresholdStore NewCachedCappedTimesheetThresholdStore(MemoryCappedTimesheetThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCappedTimesheetThresholdStore(
            inner,
            CappedTimesheetThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedTimesheetThreshold> FindCappedTimesheetThresholds(ICappedTimesheetThresholdStore store, CappedTimesheetThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
