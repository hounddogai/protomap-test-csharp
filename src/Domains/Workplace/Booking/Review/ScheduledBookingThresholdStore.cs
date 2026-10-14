namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled booking thresholds by code.</summary>
public interface IScheduledBookingThresholdStore
{
    ScheduledBookingThreshold? Find(string code);
    void Save(ScheduledBookingThreshold item);
    bool Remove(string code);
    List<ScheduledBookingThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBookingThresholdStore(Dictionary<string, ScheduledBookingThreshold> items, List<string> order) : IScheduledBookingThresholdStore
{
    public Dictionary<string, ScheduledBookingThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBookingThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBookingThreshold item)
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

    public List<ScheduledBookingThreshold> List()
    {
        var outValue = new List<ScheduledBookingThreshold>();
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
public sealed class CachedScheduledBookingThresholdStore(MemoryScheduledBookingThresholdStore inner, ScheduledBookingThresholdCache cache, int hits, int misses) : IScheduledBookingThresholdStore
{
    public MemoryScheduledBookingThresholdStore Inner { get; set; } = inner;
    public ScheduledBookingThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBookingThreshold? Find(string code)
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

    public void Save(ScheduledBookingThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBookingThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled booking thresholds. Empty fields match all.</summary>
public sealed record ScheduledBookingThresholdQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBookingThreshold item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBookingThreshold> Apply(List<ScheduledBookingThreshold> items)
    {
        var matched = Collections.Filter(items, (ScheduledBookingThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled booking threshold store.</summary>
public static class ScheduledBookingThresholdStore
{
    public static IScheduledBookingThresholdStore NewMemoryScheduledBookingThresholdStore()
    {
        return new MemoryScheduledBookingThresholdStore(
            new Dictionary<string, ScheduledBookingThreshold>(),
            new List<string>());
    }

    public static IScheduledBookingThresholdStore NewCachedScheduledBookingThresholdStore(MemoryScheduledBookingThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBookingThresholdStore(
            inner,
            ScheduledBookingThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBookingThreshold> FindScheduledBookingThresholds(IScheduledBookingThresholdStore store, ScheduledBookingThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
