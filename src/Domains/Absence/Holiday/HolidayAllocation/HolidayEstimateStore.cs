namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday estimates by code.</summary>
public interface IHolidayEstimateStore
{
    HolidayEstimate? Find(string code);
    void Save(HolidayEstimate item);
    bool Remove(string code);
    List<HolidayEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayEstimateStore(Dictionary<string, HolidayEstimate> items, List<string> order) : IHolidayEstimateStore
{
    public Dictionary<string, HolidayEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayEstimate item)
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

    public List<HolidayEstimate> List()
    {
        var outValue = new List<HolidayEstimate>();
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
public sealed class CachedHolidayEstimateStore(MemoryHolidayEstimateStore inner, HolidayEstimateCache cache, int hits, int misses) : IHolidayEstimateStore
{
    public MemoryHolidayEstimateStore Inner { get; set; } = inner;
    public HolidayEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayEstimate? Find(string code)
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

    public void Save(HolidayEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday estimates. Empty fields match all.</summary>
public sealed record HolidayEstimateQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayEstimate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayEstimate> Apply(List<HolidayEstimate> items)
    {
        var matched = Collections.Filter(items, (HolidayEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday estimate store.</summary>
public static class HolidayEstimateStore
{
    public static IHolidayEstimateStore NewMemoryHolidayEstimateStore()
    {
        return new MemoryHolidayEstimateStore(new Dictionary<string, HolidayEstimate>(), new List<string>());
    }

    public static IHolidayEstimateStore NewCachedHolidayEstimateStore(MemoryHolidayEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayEstimateStore(inner, HolidayEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayEstimate> FindHolidayEstimates(IHolidayEstimateStore store, HolidayEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
