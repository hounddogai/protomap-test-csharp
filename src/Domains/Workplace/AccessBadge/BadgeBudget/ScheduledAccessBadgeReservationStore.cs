namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled access badge reservations by code.</summary>
public interface IScheduledAccessBadgeReservationStore
{
    ScheduledAccessBadgeReservation? Find(string code);
    void Save(ScheduledAccessBadgeReservation item);
    bool Remove(string code);
    List<ScheduledAccessBadgeReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccessBadgeReservationStore(Dictionary<string, ScheduledAccessBadgeReservation> items, List<string> order) : IScheduledAccessBadgeReservationStore
{
    public Dictionary<string, ScheduledAccessBadgeReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccessBadgeReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccessBadgeReservation item)
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

    public List<ScheduledAccessBadgeReservation> List()
    {
        var outValue = new List<ScheduledAccessBadgeReservation>();
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
public sealed class CachedScheduledAccessBadgeReservationStore(MemoryScheduledAccessBadgeReservationStore inner, ScheduledAccessBadgeReservationCache cache, int hits, int misses) : IScheduledAccessBadgeReservationStore
{
    public MemoryScheduledAccessBadgeReservationStore Inner { get; set; } = inner;
    public ScheduledAccessBadgeReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccessBadgeReservation? Find(string code)
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

    public void Save(ScheduledAccessBadgeReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccessBadgeReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled access badge reservations. Empty fields match all.</summary>
public sealed record ScheduledAccessBadgeReservationQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccessBadgeReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccessBadgeReservation> Apply(List<ScheduledAccessBadgeReservation> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccessBadgeReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled access badge reservation store.</summary>
public static class ScheduledAccessBadgeReservationStore
{
    public static IScheduledAccessBadgeReservationStore NewMemoryScheduledAccessBadgeReservationStore()
    {
        return new MemoryScheduledAccessBadgeReservationStore(
            new Dictionary<string, ScheduledAccessBadgeReservation>(),
            new List<string>());
    }

    public static IScheduledAccessBadgeReservationStore NewCachedScheduledAccessBadgeReservationStore(MemoryScheduledAccessBadgeReservationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccessBadgeReservationStore(
            inner,
            ScheduledAccessBadgeReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccessBadgeReservation> FindScheduledAccessBadgeReservations(IScheduledAccessBadgeReservationStore store, ScheduledAccessBadgeReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
