namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk reservations by code.</summary>
public interface IDeskReservationStore
{
    DeskReservation? Find(string code);
    void Save(DeskReservation item);
    bool Remove(string code);
    List<DeskReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskReservationStore(Dictionary<string, DeskReservation> items, List<string> order) : IDeskReservationStore
{
    public Dictionary<string, DeskReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskReservation item)
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

    public List<DeskReservation> List()
    {
        var outValue = new List<DeskReservation>();
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
public sealed class CachedDeskReservationStore(MemoryDeskReservationStore inner, DeskReservationCache cache, int hits, int misses) : IDeskReservationStore
{
    public MemoryDeskReservationStore Inner { get; set; } = inner;
    public DeskReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskReservation? Find(string code)
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

    public void Save(DeskReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk reservations. Empty fields match all.</summary>
public sealed record DeskReservationQuery(string Country, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DeskReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskReservation> Apply(List<DeskReservation> items)
    {
        var matched = Collections.Filter(items, (DeskReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk reservation store.</summary>
public static class DeskReservationStore
{
    public static IDeskReservationStore NewMemoryDeskReservationStore()
    {
        return new MemoryDeskReservationStore(new Dictionary<string, DeskReservation>(), new List<string>());
    }

    public static IDeskReservationStore NewCachedDeskReservationStore(MemoryDeskReservationStore inner, int ttl, IClock clock)
    {
        return new CachedDeskReservationStore(inner, DeskReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskReservation> FindDeskReservations(IDeskReservationStore store, DeskReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
