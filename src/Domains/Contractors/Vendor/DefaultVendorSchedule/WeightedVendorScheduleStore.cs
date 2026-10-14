namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted vendor schedules by code.</summary>
public interface IWeightedVendorScheduleStore
{
    WeightedVendorSchedule? Find(string code);
    void Save(WeightedVendorSchedule item);
    bool Remove(string code);
    List<WeightedVendorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedVendorScheduleStore(Dictionary<string, WeightedVendorSchedule> items, List<string> order) : IWeightedVendorScheduleStore
{
    public Dictionary<string, WeightedVendorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedVendorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedVendorSchedule item)
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

    public List<WeightedVendorSchedule> List()
    {
        var outValue = new List<WeightedVendorSchedule>();
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
public sealed class CachedWeightedVendorScheduleStore(MemoryWeightedVendorScheduleStore inner, WeightedVendorScheduleCache cache, int hits, int misses) : IWeightedVendorScheduleStore
{
    public MemoryWeightedVendorScheduleStore Inner { get; set; } = inner;
    public WeightedVendorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedVendorSchedule? Find(string code)
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

    public void Save(WeightedVendorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedVendorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted vendor schedules. Empty fields match all.</summary>
public sealed record WeightedVendorScheduleQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedVendorSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedVendorSchedule> Apply(List<WeightedVendorSchedule> items)
    {
        var matched = Collections.Filter(items, (WeightedVendorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted vendor schedule store.</summary>
public static class WeightedVendorScheduleStore
{
    public static IWeightedVendorScheduleStore NewMemoryWeightedVendorScheduleStore()
    {
        return new MemoryWeightedVendorScheduleStore(
            new Dictionary<string, WeightedVendorSchedule>(),
            new List<string>());
    }

    public static IWeightedVendorScheduleStore NewCachedWeightedVendorScheduleStore(MemoryWeightedVendorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedVendorScheduleStore(
            inner,
            WeightedVendorScheduleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedVendorSchedule> FindWeightedVendorSchedules(IWeightedVendorScheduleStore store, WeightedVendorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
