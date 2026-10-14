namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday variances by code.</summary>
public interface IHolidayVarianceStore
{
    HolidayVariance? Find(string code);
    void Save(HolidayVariance item);
    bool Remove(string code);
    List<HolidayVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayVarianceStore(Dictionary<string, HolidayVariance> items, List<string> order) : IHolidayVarianceStore
{
    public Dictionary<string, HolidayVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayVariance item)
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

    public List<HolidayVariance> List()
    {
        var outValue = new List<HolidayVariance>();
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
public sealed class CachedHolidayVarianceStore(MemoryHolidayVarianceStore inner, HolidayVarianceCache cache, int hits, int misses) : IHolidayVarianceStore
{
    public MemoryHolidayVarianceStore Inner { get; set; } = inner;
    public HolidayVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayVariance? Find(string code)
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

    public void Save(HolidayVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday variances. Empty fields match all.</summary>
public sealed record HolidayVarianceQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayVariance> Apply(List<HolidayVariance> items)
    {
        var matched = Collections.Filter(items, (HolidayVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday variance store.</summary>
public static class HolidayVarianceStore
{
    public static IHolidayVarianceStore NewMemoryHolidayVarianceStore()
    {
        return new MemoryHolidayVarianceStore(new Dictionary<string, HolidayVariance>(), new List<string>());
    }

    public static IHolidayVarianceStore NewCachedHolidayVarianceStore(MemoryHolidayVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayVarianceStore(inner, HolidayVarianceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayVariance> FindHolidayVariances(IHolidayVarianceStore store, HolidayVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
