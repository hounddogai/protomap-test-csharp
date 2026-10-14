namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered mobility reservations by code.</summary>
public interface ITieredMobilityReservationStore
{
    TieredMobilityReservation? Find(string code);
    void Save(TieredMobilityReservation item);
    bool Remove(string code);
    List<TieredMobilityReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredMobilityReservationStore(Dictionary<string, TieredMobilityReservation> items, List<string> order) : ITieredMobilityReservationStore
{
    public Dictionary<string, TieredMobilityReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredMobilityReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredMobilityReservation item)
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

    public List<TieredMobilityReservation> List()
    {
        var outValue = new List<TieredMobilityReservation>();
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
public sealed class CachedTieredMobilityReservationStore(MemoryTieredMobilityReservationStore inner, TieredMobilityReservationCache cache, int hits, int misses) : ITieredMobilityReservationStore
{
    public MemoryTieredMobilityReservationStore Inner { get; set; } = inner;
    public TieredMobilityReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredMobilityReservation? Find(string code)
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

    public void Save(TieredMobilityReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredMobilityReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered mobility reservations. Empty fields match all.</summary>
public sealed record TieredMobilityReservationQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TieredMobilityReservation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredMobilityReservation> Apply(List<TieredMobilityReservation> items)
    {
        var matched = Collections.Filter(items, (TieredMobilityReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered mobility reservation store.</summary>
public static class TieredMobilityReservationStore
{
    public static ITieredMobilityReservationStore NewMemoryTieredMobilityReservationStore()
    {
        return new MemoryTieredMobilityReservationStore(
            new Dictionary<string, TieredMobilityReservation>(),
            new List<string>());
    }

    public static ITieredMobilityReservationStore NewCachedTieredMobilityReservationStore(MemoryTieredMobilityReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTieredMobilityReservationStore(
            inner,
            TieredMobilityReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredMobilityReservation> FindTieredMobilityReservations(ITieredMobilityReservationStore store, TieredMobilityReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
