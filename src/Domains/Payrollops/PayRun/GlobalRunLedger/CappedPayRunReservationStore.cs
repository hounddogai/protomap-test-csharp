namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pay run reservations by code.</summary>
public interface ICappedPayRunReservationStore
{
    CappedPayRunReservation? Find(string code);
    void Save(CappedPayRunReservation item);
    bool Remove(string code);
    List<CappedPayRunReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayRunReservationStore(Dictionary<string, CappedPayRunReservation> items, List<string> order) : ICappedPayRunReservationStore
{
    public Dictionary<string, CappedPayRunReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayRunReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayRunReservation item)
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

    public List<CappedPayRunReservation> List()
    {
        var outValue = new List<CappedPayRunReservation>();
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
public sealed class CachedCappedPayRunReservationStore(MemoryCappedPayRunReservationStore inner, CappedPayRunReservationCache cache, int hits, int misses) : ICappedPayRunReservationStore
{
    public MemoryCappedPayRunReservationStore Inner { get; set; } = inner;
    public CappedPayRunReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayRunReservation? Find(string code)
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

    public void Save(CappedPayRunReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayRunReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pay run reservations. Empty fields match all.</summary>
public sealed record CappedPayRunReservationQuery(string Region, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayRunReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayRunReservation> Apply(List<CappedPayRunReservation> items)
    {
        var matched = Collections.Filter(items, (CappedPayRunReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pay run reservation store.</summary>
public static class CappedPayRunReservationStore
{
    public static ICappedPayRunReservationStore NewMemoryCappedPayRunReservationStore()
    {
        return new MemoryCappedPayRunReservationStore(
            new Dictionary<string, CappedPayRunReservation>(),
            new List<string>());
    }

    public static ICappedPayRunReservationStore NewCachedCappedPayRunReservationStore(MemoryCappedPayRunReservationStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayRunReservationStore(
            inner,
            CappedPayRunReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedPayRunReservation> FindCappedPayRunReservations(ICappedPayRunReservationStore store, CappedPayRunReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
