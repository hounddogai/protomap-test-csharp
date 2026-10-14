namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position reservations by code.</summary>
public interface IPositionReservationStore
{
    PositionReservation? Find(string code);
    void Save(PositionReservation item);
    bool Remove(string code);
    List<PositionReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionReservationStore(Dictionary<string, PositionReservation> items, List<string> order) : IPositionReservationStore
{
    public Dictionary<string, PositionReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionReservation item)
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

    public List<PositionReservation> List()
    {
        var outValue = new List<PositionReservation>();
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
public sealed class CachedPositionReservationStore(MemoryPositionReservationStore inner, PositionReservationCache cache, int hits, int misses) : IPositionReservationStore
{
    public MemoryPositionReservationStore Inner { get; set; } = inner;
    public PositionReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionReservation? Find(string code)
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

    public void Save(PositionReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position reservations. Empty fields match all.</summary>
public sealed record PositionReservationQuery(string Locale, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PositionReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionReservation> Apply(List<PositionReservation> items)
    {
        var matched = Collections.Filter(items, (PositionReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position reservation store.</summary>
public static class PositionReservationStore
{
    public static IPositionReservationStore NewMemoryPositionReservationStore()
    {
        return new MemoryPositionReservationStore(new Dictionary<string, PositionReservation>(), new List<string>());
    }

    public static IPositionReservationStore NewCachedPositionReservationStore(MemoryPositionReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPositionReservationStore(inner, PositionReservationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionReservation> FindPositionReservations(IPositionReservationStore store, PositionReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
