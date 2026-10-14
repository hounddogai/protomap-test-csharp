namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped mobility reservations by code.</summary>
public interface ICappedMobilityReservationStore
{
    CappedMobilityReservation? Find(string code);
    void Save(CappedMobilityReservation item);
    bool Remove(string code);
    List<CappedMobilityReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedMobilityReservationStore(Dictionary<string, CappedMobilityReservation> items, List<string> order) : ICappedMobilityReservationStore
{
    public Dictionary<string, CappedMobilityReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedMobilityReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedMobilityReservation item)
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

    public List<CappedMobilityReservation> List()
    {
        var outValue = new List<CappedMobilityReservation>();
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
public sealed class CachedCappedMobilityReservationStore(MemoryCappedMobilityReservationStore inner, CappedMobilityReservationCache cache, int hits, int misses) : ICappedMobilityReservationStore
{
    public MemoryCappedMobilityReservationStore Inner { get; set; } = inner;
    public CappedMobilityReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedMobilityReservation? Find(string code)
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

    public void Save(CappedMobilityReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedMobilityReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped mobility reservations. Empty fields match all.</summary>
public sealed record CappedMobilityReservationQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedMobilityReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<CappedMobilityReservation> Apply(List<CappedMobilityReservation> items)
    {
        var matched = Collections.Filter(items, (CappedMobilityReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped mobility reservation store.</summary>
public static class CappedMobilityReservationStore
{
    public static ICappedMobilityReservationStore NewMemoryCappedMobilityReservationStore()
    {
        return new MemoryCappedMobilityReservationStore(
            new Dictionary<string, CappedMobilityReservation>(),
            new List<string>());
    }

    public static ICappedMobilityReservationStore NewCachedCappedMobilityReservationStore(MemoryCappedMobilityReservationStore inner, int ttl, IClock clock)
    {
        return new CachedCappedMobilityReservationStore(
            inner,
            CappedMobilityReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedMobilityReservation> FindCappedMobilityReservations(ICappedMobilityReservationStore store, CappedMobilityReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
