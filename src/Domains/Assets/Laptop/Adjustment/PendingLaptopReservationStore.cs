namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending laptop reservations by code.</summary>
public interface IPendingLaptopReservationStore
{
    PendingLaptopReservation? Find(string code);
    void Save(PendingLaptopReservation item);
    bool Remove(string code);
    List<PendingLaptopReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLaptopReservationStore(Dictionary<string, PendingLaptopReservation> items, List<string> order) : IPendingLaptopReservationStore
{
    public Dictionary<string, PendingLaptopReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLaptopReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLaptopReservation item)
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

    public List<PendingLaptopReservation> List()
    {
        var outValue = new List<PendingLaptopReservation>();
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
public sealed class CachedPendingLaptopReservationStore(MemoryPendingLaptopReservationStore inner, PendingLaptopReservationCache cache, int hits, int misses) : IPendingLaptopReservationStore
{
    public MemoryPendingLaptopReservationStore Inner { get; set; } = inner;
    public PendingLaptopReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLaptopReservation? Find(string code)
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

    public void Save(PendingLaptopReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLaptopReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending laptop reservations. Empty fields match all.</summary>
public sealed record PendingLaptopReservationQuery(string Country, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLaptopReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<PendingLaptopReservation> Apply(List<PendingLaptopReservation> items)
    {
        var matched = Collections.Filter(items, (PendingLaptopReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending laptop reservation store.</summary>
public static class PendingLaptopReservationStore
{
    public static IPendingLaptopReservationStore NewMemoryPendingLaptopReservationStore()
    {
        return new MemoryPendingLaptopReservationStore(
            new Dictionary<string, PendingLaptopReservation>(),
            new List<string>());
    }

    public static IPendingLaptopReservationStore NewCachedPendingLaptopReservationStore(MemoryPendingLaptopReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLaptopReservationStore(
            inner,
            PendingLaptopReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingLaptopReservation> FindPendingLaptopReservations(IPendingLaptopReservationStore store, PendingLaptopReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
