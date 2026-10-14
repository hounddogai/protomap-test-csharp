namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending cost model reservations by code.</summary>
public interface IPendingCostModelReservationStore
{
    PendingCostModelReservation? Find(string code);
    void Save(PendingCostModelReservation item);
    bool Remove(string code);
    List<PendingCostModelReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingCostModelReservationStore(Dictionary<string, PendingCostModelReservation> items, List<string> order) : IPendingCostModelReservationStore
{
    public Dictionary<string, PendingCostModelReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingCostModelReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingCostModelReservation item)
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

    public List<PendingCostModelReservation> List()
    {
        var outValue = new List<PendingCostModelReservation>();
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
public sealed class CachedPendingCostModelReservationStore(MemoryPendingCostModelReservationStore inner, PendingCostModelReservationCache cache, int hits, int misses) : IPendingCostModelReservationStore
{
    public MemoryPendingCostModelReservationStore Inner { get; set; } = inner;
    public PendingCostModelReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingCostModelReservation? Find(string code)
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

    public void Save(PendingCostModelReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingCostModelReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending cost model reservations. Empty fields match all.</summary>
public sealed record PendingCostModelReservationQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PendingCostModelReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingCostModelReservation> Apply(List<PendingCostModelReservation> items)
    {
        var matched = Collections.Filter(items, (PendingCostModelReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending cost model reservation store.</summary>
public static class PendingCostModelReservationStore
{
    public static IPendingCostModelReservationStore NewMemoryPendingCostModelReservationStore()
    {
        return new MemoryPendingCostModelReservationStore(
            new Dictionary<string, PendingCostModelReservation>(),
            new List<string>());
    }

    public static IPendingCostModelReservationStore NewCachedPendingCostModelReservationStore(MemoryPendingCostModelReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPendingCostModelReservationStore(
            inner,
            PendingCostModelReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingCostModelReservation> FindPendingCostModelReservations(IPendingCostModelReservationStore store, PendingCostModelReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
