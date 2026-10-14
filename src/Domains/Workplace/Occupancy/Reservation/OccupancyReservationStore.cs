namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy reservations by code.</summary>
public interface IOccupancyReservationStore
{
    OccupancyReservation? Find(string code);
    void Save(OccupancyReservation item);
    bool Remove(string code);
    List<OccupancyReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyReservationStore(Dictionary<string, OccupancyReservation> items, List<string> order) : IOccupancyReservationStore
{
    public Dictionary<string, OccupancyReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyReservation item)
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

    public List<OccupancyReservation> List()
    {
        var outValue = new List<OccupancyReservation>();
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
public sealed class CachedOccupancyReservationStore(MemoryOccupancyReservationStore inner, OccupancyReservationCache cache, int hits, int misses) : IOccupancyReservationStore
{
    public MemoryOccupancyReservationStore Inner { get; set; } = inner;
    public OccupancyReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyReservation? Find(string code)
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

    public void Save(OccupancyReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy reservations. Empty fields match all.</summary>
public sealed record OccupancyReservationQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<OccupancyReservation> Apply(List<OccupancyReservation> items)
    {
        var matched = Collections.Filter(items, (OccupancyReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy reservation store.</summary>
public static class OccupancyReservationStore
{
    public static IOccupancyReservationStore NewMemoryOccupancyReservationStore()
    {
        return new MemoryOccupancyReservationStore(
            new Dictionary<string, OccupancyReservation>(),
            new List<string>());
    }

    public static IOccupancyReservationStore NewCachedOccupancyReservationStore(MemoryOccupancyReservationStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyReservationStore(inner, OccupancyReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OccupancyReservation> FindOccupancyReservations(IOccupancyReservationStore store, OccupancyReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
