namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled nomination reservations by code.</summary>
public interface IScheduledNominationReservationStore
{
    ScheduledNominationReservation? Find(string code);
    void Save(ScheduledNominationReservation item);
    bool Remove(string code);
    List<ScheduledNominationReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledNominationReservationStore(Dictionary<string, ScheduledNominationReservation> items, List<string> order) : IScheduledNominationReservationStore
{
    public Dictionary<string, ScheduledNominationReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledNominationReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledNominationReservation item)
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

    public List<ScheduledNominationReservation> List()
    {
        var outValue = new List<ScheduledNominationReservation>();
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
public sealed class CachedScheduledNominationReservationStore(MemoryScheduledNominationReservationStore inner, ScheduledNominationReservationCache cache, int hits, int misses) : IScheduledNominationReservationStore
{
    public MemoryScheduledNominationReservationStore Inner { get; set; } = inner;
    public ScheduledNominationReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledNominationReservation? Find(string code)
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

    public void Save(ScheduledNominationReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledNominationReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled nomination reservations. Empty fields match all.</summary>
public sealed record ScheduledNominationReservationQuery(string Category, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledNominationReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ScheduledNominationReservation> Apply(List<ScheduledNominationReservation> items)
    {
        var matched = Collections.Filter(items, (ScheduledNominationReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled nomination reservation store.</summary>
public static class ScheduledNominationReservationStore
{
    public static IScheduledNominationReservationStore NewMemoryScheduledNominationReservationStore()
    {
        return new MemoryScheduledNominationReservationStore(
            new Dictionary<string, ScheduledNominationReservation>(),
            new List<string>());
    }

    public static IScheduledNominationReservationStore NewCachedScheduledNominationReservationStore(MemoryScheduledNominationReservationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledNominationReservationStore(
            inner,
            ScheduledNominationReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledNominationReservation> FindScheduledNominationReservations(IScheduledNominationReservationStore store, ScheduledNominationReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
