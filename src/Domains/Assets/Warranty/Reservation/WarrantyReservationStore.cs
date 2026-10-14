namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty reservations by code.</summary>
public interface IWarrantyReservationStore
{
    WarrantyReservation? Find(string code);
    void Save(WarrantyReservation item);
    bool Remove(string code);
    List<WarrantyReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyReservationStore(Dictionary<string, WarrantyReservation> items, List<string> order) : IWarrantyReservationStore
{
    public Dictionary<string, WarrantyReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyReservation item)
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

    public List<WarrantyReservation> List()
    {
        var outValue = new List<WarrantyReservation>();
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
public sealed class CachedWarrantyReservationStore(MemoryWarrantyReservationStore inner, WarrantyReservationCache cache, int hits, int misses) : IWarrantyReservationStore
{
    public MemoryWarrantyReservationStore Inner { get; set; } = inner;
    public WarrantyReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyReservation? Find(string code)
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

    public void Save(WarrantyReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty reservations. Empty fields match all.</summary>
public sealed record WarrantyReservationQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyReservation> Apply(List<WarrantyReservation> items)
    {
        var matched = Collections.Filter(items, (WarrantyReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty reservation store.</summary>
public static class WarrantyReservationStore
{
    public static IWarrantyReservationStore NewMemoryWarrantyReservationStore()
    {
        return new MemoryWarrantyReservationStore(new Dictionary<string, WarrantyReservation>(), new List<string>());
    }

    public static IWarrantyReservationStore NewCachedWarrantyReservationStore(MemoryWarrantyReservationStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyReservationStore(inner, WarrantyReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyReservation> FindWarrantyReservations(IWarrantyReservationStore store, WarrantyReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
