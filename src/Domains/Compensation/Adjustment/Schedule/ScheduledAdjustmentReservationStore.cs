namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled adjustment reservations by code.</summary>
public interface IScheduledAdjustmentReservationStore
{
    ScheduledAdjustmentReservation? Find(string code);
    void Save(ScheduledAdjustmentReservation item);
    bool Remove(string code);
    List<ScheduledAdjustmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAdjustmentReservationStore(Dictionary<string, ScheduledAdjustmentReservation> items, List<string> order) : IScheduledAdjustmentReservationStore
{
    public Dictionary<string, ScheduledAdjustmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAdjustmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAdjustmentReservation item)
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

    public List<ScheduledAdjustmentReservation> List()
    {
        var outValue = new List<ScheduledAdjustmentReservation>();
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
public sealed class CachedScheduledAdjustmentReservationStore(MemoryScheduledAdjustmentReservationStore inner, ScheduledAdjustmentReservationCache cache, int hits, int misses) : IScheduledAdjustmentReservationStore
{
    public MemoryScheduledAdjustmentReservationStore Inner { get; set; } = inner;
    public ScheduledAdjustmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAdjustmentReservation? Find(string code)
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

    public void Save(ScheduledAdjustmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAdjustmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled adjustment reservations. Empty fields match all.</summary>
public sealed record ScheduledAdjustmentReservationQuery(string Channel, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAdjustmentReservation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<ScheduledAdjustmentReservation> Apply(List<ScheduledAdjustmentReservation> items)
    {
        var matched = Collections.Filter(items, (ScheduledAdjustmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled adjustment reservation store.</summary>
public static class ScheduledAdjustmentReservationStore
{
    public static IScheduledAdjustmentReservationStore NewMemoryScheduledAdjustmentReservationStore()
    {
        return new MemoryScheduledAdjustmentReservationStore(
            new Dictionary<string, ScheduledAdjustmentReservation>(),
            new List<string>());
    }

    public static IScheduledAdjustmentReservationStore NewCachedScheduledAdjustmentReservationStore(MemoryScheduledAdjustmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAdjustmentReservationStore(
            inner,
            ScheduledAdjustmentReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAdjustmentReservation> FindScheduledAdjustmentReservations(IScheduledAdjustmentReservationStore store, ScheduledAdjustmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
