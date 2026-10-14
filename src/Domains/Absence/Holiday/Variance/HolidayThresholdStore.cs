namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday thresholds by code.</summary>
public interface IHolidayThresholdStore
{
    HolidayThreshold? Find(string code);
    void Save(HolidayThreshold item);
    bool Remove(string code);
    List<HolidayThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayThresholdStore(Dictionary<string, HolidayThreshold> items, List<string> order) : IHolidayThresholdStore
{
    public Dictionary<string, HolidayThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayThreshold item)
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

    public List<HolidayThreshold> List()
    {
        var outValue = new List<HolidayThreshold>();
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
public sealed class CachedHolidayThresholdStore(MemoryHolidayThresholdStore inner, HolidayThresholdCache cache, int hits, int misses) : IHolidayThresholdStore
{
    public MemoryHolidayThresholdStore Inner { get; set; } = inner;
    public HolidayThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayThreshold? Find(string code)
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

    public void Save(HolidayThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday thresholds. Empty fields match all.</summary>
public sealed record HolidayThresholdQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayThreshold item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayThreshold> Apply(List<HolidayThreshold> items)
    {
        var matched = Collections.Filter(items, (HolidayThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday threshold store.</summary>
public static class HolidayThresholdStore
{
    public static IHolidayThresholdStore NewMemoryHolidayThresholdStore()
    {
        return new MemoryHolidayThresholdStore(new Dictionary<string, HolidayThreshold>(), new List<string>());
    }

    public static IHolidayThresholdStore NewCachedHolidayThresholdStore(MemoryHolidayThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayThresholdStore(inner, HolidayThresholdCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HolidayThreshold> FindHolidayThresholds(IHolidayThresholdStore store, HolidayThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
