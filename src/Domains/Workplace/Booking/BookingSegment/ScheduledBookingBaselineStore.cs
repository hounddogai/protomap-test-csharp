namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled booking baselines by code.</summary>
public interface IScheduledBookingBaselineStore
{
    ScheduledBookingBaseline? Find(string code);
    void Save(ScheduledBookingBaseline item);
    bool Remove(string code);
    List<ScheduledBookingBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBookingBaselineStore(Dictionary<string, ScheduledBookingBaseline> items, List<string> order) : IScheduledBookingBaselineStore
{
    public Dictionary<string, ScheduledBookingBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBookingBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBookingBaseline item)
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

    public List<ScheduledBookingBaseline> List()
    {
        var outValue = new List<ScheduledBookingBaseline>();
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
public sealed class CachedScheduledBookingBaselineStore(MemoryScheduledBookingBaselineStore inner, ScheduledBookingBaselineCache cache, int hits, int misses) : IScheduledBookingBaselineStore
{
    public MemoryScheduledBookingBaselineStore Inner { get; set; } = inner;
    public ScheduledBookingBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBookingBaseline? Find(string code)
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

    public void Save(ScheduledBookingBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBookingBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled booking baselines. Empty fields match all.</summary>
public sealed record ScheduledBookingBaselineQuery(string Team, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBookingBaseline item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBookingBaseline> Apply(List<ScheduledBookingBaseline> items)
    {
        var matched = Collections.Filter(items, (ScheduledBookingBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled booking baseline store.</summary>
public static class ScheduledBookingBaselineStore
{
    public static IScheduledBookingBaselineStore NewMemoryScheduledBookingBaselineStore()
    {
        return new MemoryScheduledBookingBaselineStore(
            new Dictionary<string, ScheduledBookingBaseline>(),
            new List<string>());
    }

    public static IScheduledBookingBaselineStore NewCachedScheduledBookingBaselineStore(MemoryScheduledBookingBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBookingBaselineStore(
            inner,
            ScheduledBookingBaselineCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBookingBaseline> FindScheduledBookingBaselines(IScheduledBookingBaselineStore store, ScheduledBookingBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
