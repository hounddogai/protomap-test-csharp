namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model reservations by code.</summary>
public interface ICostModelReservationStore
{
    CostModelReservation? Find(string code);
    void Save(CostModelReservation item);
    bool Remove(string code);
    List<CostModelReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelReservationStore(Dictionary<string, CostModelReservation> items, List<string> order) : ICostModelReservationStore
{
    public Dictionary<string, CostModelReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelReservation item)
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

    public List<CostModelReservation> List()
    {
        var outValue = new List<CostModelReservation>();
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
public sealed class CachedCostModelReservationStore(MemoryCostModelReservationStore inner, CostModelReservationCache cache, int hits, int misses) : ICostModelReservationStore
{
    public MemoryCostModelReservationStore Inner { get; set; } = inner;
    public CostModelReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelReservation? Find(string code)
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

    public void Save(CostModelReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model reservations. Empty fields match all.</summary>
public sealed record CostModelReservationQuery(string Unit, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelReservation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelReservation> Apply(List<CostModelReservation> items)
    {
        var matched = Collections.Filter(items, (CostModelReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model reservation store.</summary>
public static class CostModelReservationStore
{
    public static ICostModelReservationStore NewMemoryCostModelReservationStore()
    {
        return new MemoryCostModelReservationStore(
            new Dictionary<string, CostModelReservation>(),
            new List<string>());
    }

    public static ICostModelReservationStore NewCachedCostModelReservationStore(MemoryCostModelReservationStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelReservationStore(inner, CostModelReservationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CostModelReservation> FindCostModelReservations(ICostModelReservationStore store, CostModelReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
