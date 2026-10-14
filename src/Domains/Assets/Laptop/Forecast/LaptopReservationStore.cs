namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop reservations by code.</summary>
public interface ILaptopReservationStore
{
    LaptopReservation? Find(string code);
    void Save(LaptopReservation item);
    bool Remove(string code);
    List<LaptopReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopReservationStore(Dictionary<string, LaptopReservation> items, List<string> order) : ILaptopReservationStore
{
    public Dictionary<string, LaptopReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopReservation item)
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

    public List<LaptopReservation> List()
    {
        var outValue = new List<LaptopReservation>();
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
public sealed class CachedLaptopReservationStore(MemoryLaptopReservationStore inner, LaptopReservationCache cache, int hits, int misses) : ILaptopReservationStore
{
    public MemoryLaptopReservationStore Inner { get; set; } = inner;
    public LaptopReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopReservation? Find(string code)
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

    public void Save(LaptopReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop reservations. Empty fields match all.</summary>
public sealed record LaptopReservationQuery(string Department, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopReservation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopReservation> Apply(List<LaptopReservation> items)
    {
        var matched = Collections.Filter(items, (LaptopReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop reservation store.</summary>
public static class LaptopReservationStore
{
    public static ILaptopReservationStore NewMemoryLaptopReservationStore()
    {
        return new MemoryLaptopReservationStore(new Dictionary<string, LaptopReservation>(), new List<string>());
    }

    public static ILaptopReservationStore NewCachedLaptopReservationStore(MemoryLaptopReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopReservationStore(inner, LaptopReservationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopReservation> FindLaptopReservations(ILaptopReservationStore store, LaptopReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
