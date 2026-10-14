namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted booking variances by code.</summary>
public interface IWeightedBookingVarianceStore
{
    WeightedBookingVariance? Find(string code);
    void Save(WeightedBookingVariance item);
    bool Remove(string code);
    List<WeightedBookingVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBookingVarianceStore(Dictionary<string, WeightedBookingVariance> items, List<string> order) : IWeightedBookingVarianceStore
{
    public Dictionary<string, WeightedBookingVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBookingVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBookingVariance item)
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

    public List<WeightedBookingVariance> List()
    {
        var outValue = new List<WeightedBookingVariance>();
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
public sealed class CachedWeightedBookingVarianceStore(MemoryWeightedBookingVarianceStore inner, WeightedBookingVarianceCache cache, int hits, int misses) : IWeightedBookingVarianceStore
{
    public MemoryWeightedBookingVarianceStore Inner { get; set; } = inner;
    public WeightedBookingVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBookingVariance? Find(string code)
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

    public void Save(WeightedBookingVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBookingVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted booking variances. Empty fields match all.</summary>
public sealed record WeightedBookingVarianceQuery(string Channel, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBookingVariance item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<WeightedBookingVariance> Apply(List<WeightedBookingVariance> items)
    {
        var matched = Collections.Filter(items, (WeightedBookingVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted booking variance store.</summary>
public static class WeightedBookingVarianceStore
{
    public static IWeightedBookingVarianceStore NewMemoryWeightedBookingVarianceStore()
    {
        return new MemoryWeightedBookingVarianceStore(
            new Dictionary<string, WeightedBookingVariance>(),
            new List<string>());
    }

    public static IWeightedBookingVarianceStore NewCachedWeightedBookingVarianceStore(MemoryWeightedBookingVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBookingVarianceStore(
            inner,
            WeightedBookingVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBookingVariance> FindWeightedBookingVariances(IWeightedBookingVarianceStore store, WeightedBookingVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
