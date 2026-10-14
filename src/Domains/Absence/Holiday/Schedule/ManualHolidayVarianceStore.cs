namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual holiday variances by code.</summary>
public interface IManualHolidayVarianceStore
{
    ManualHolidayVariance? Find(string code);
    void Save(ManualHolidayVariance item);
    bool Remove(string code);
    List<ManualHolidayVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualHolidayVarianceStore(Dictionary<string, ManualHolidayVariance> items, List<string> order) : IManualHolidayVarianceStore
{
    public Dictionary<string, ManualHolidayVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualHolidayVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualHolidayVariance item)
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

    public List<ManualHolidayVariance> List()
    {
        var outValue = new List<ManualHolidayVariance>();
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
public sealed class CachedManualHolidayVarianceStore(MemoryManualHolidayVarianceStore inner, ManualHolidayVarianceCache cache, int hits, int misses) : IManualHolidayVarianceStore
{
    public MemoryManualHolidayVarianceStore Inner { get; set; } = inner;
    public ManualHolidayVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualHolidayVariance? Find(string code)
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

    public void Save(ManualHolidayVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualHolidayVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual holiday variances. Empty fields match all.</summary>
public sealed record ManualHolidayVarianceQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ManualHolidayVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ManualHolidayVariance> Apply(List<ManualHolidayVariance> items)
    {
        var matched = Collections.Filter(items, (ManualHolidayVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual holiday variance store.</summary>
public static class ManualHolidayVarianceStore
{
    public static IManualHolidayVarianceStore NewMemoryManualHolidayVarianceStore()
    {
        return new MemoryManualHolidayVarianceStore(
            new Dictionary<string, ManualHolidayVariance>(),
            new List<string>());
    }

    public static IManualHolidayVarianceStore NewCachedManualHolidayVarianceStore(MemoryManualHolidayVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedManualHolidayVarianceStore(inner, ManualHolidayVarianceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualHolidayVariance> FindManualHolidayVariances(IManualHolidayVarianceStore store, ManualHolidayVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
