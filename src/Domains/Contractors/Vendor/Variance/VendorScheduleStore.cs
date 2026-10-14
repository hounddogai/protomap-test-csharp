namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor schedules by code.</summary>
public interface IVendorScheduleStore
{
    VendorSchedule? Find(string code);
    void Save(VendorSchedule item);
    bool Remove(string code);
    List<VendorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorScheduleStore(Dictionary<string, VendorSchedule> items, List<string> order) : IVendorScheduleStore
{
    public Dictionary<string, VendorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorSchedule item)
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

    public List<VendorSchedule> List()
    {
        var outValue = new List<VendorSchedule>();
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
public sealed class CachedVendorScheduleStore(MemoryVendorScheduleStore inner, VendorScheduleCache cache, int hits, int misses) : IVendorScheduleStore
{
    public MemoryVendorScheduleStore Inner { get; set; } = inner;
    public VendorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorSchedule? Find(string code)
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

    public void Save(VendorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor schedules. Empty fields match all.</summary>
public sealed record VendorScheduleQuery(string Country, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(VendorSchedule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorSchedule> Apply(List<VendorSchedule> items)
    {
        var matched = Collections.Filter(items, (VendorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor schedule store.</summary>
public static class VendorScheduleStore
{
    public static IVendorScheduleStore NewMemoryVendorScheduleStore()
    {
        return new MemoryVendorScheduleStore(new Dictionary<string, VendorSchedule>(), new List<string>());
    }

    public static IVendorScheduleStore NewCachedVendorScheduleStore(MemoryVendorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedVendorScheduleStore(inner, VendorScheduleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorSchedule> FindVendorSchedules(IVendorScheduleStore store, VendorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
