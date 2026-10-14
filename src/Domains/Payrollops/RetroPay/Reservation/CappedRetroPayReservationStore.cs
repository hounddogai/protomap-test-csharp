namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped retro pay reservations by code.</summary>
public interface ICappedRetroPayReservationStore
{
    CappedRetroPayReservation? Find(string code);
    void Save(CappedRetroPayReservation item);
    bool Remove(string code);
    List<CappedRetroPayReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRetroPayReservationStore(Dictionary<string, CappedRetroPayReservation> items, List<string> order) : ICappedRetroPayReservationStore
{
    public Dictionary<string, CappedRetroPayReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRetroPayReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRetroPayReservation item)
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

    public List<CappedRetroPayReservation> List()
    {
        var outValue = new List<CappedRetroPayReservation>();
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
public sealed class CachedCappedRetroPayReservationStore(MemoryCappedRetroPayReservationStore inner, CappedRetroPayReservationCache cache, int hits, int misses) : ICappedRetroPayReservationStore
{
    public MemoryCappedRetroPayReservationStore Inner { get; set; } = inner;
    public CappedRetroPayReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRetroPayReservation? Find(string code)
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

    public void Save(CappedRetroPayReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRetroPayReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped retro pay reservations. Empty fields match all.</summary>
public sealed record CappedRetroPayReservationQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRetroPayReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRetroPayReservation> Apply(List<CappedRetroPayReservation> items)
    {
        var matched = Collections.Filter(items, (CappedRetroPayReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped retro pay reservation store.</summary>
public static class CappedRetroPayReservationStore
{
    public static ICappedRetroPayReservationStore NewMemoryCappedRetroPayReservationStore()
    {
        return new MemoryCappedRetroPayReservationStore(
            new Dictionary<string, CappedRetroPayReservation>(),
            new List<string>());
    }

    public static ICappedRetroPayReservationStore NewCachedCappedRetroPayReservationStore(MemoryCappedRetroPayReservationStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRetroPayReservationStore(
            inner,
            CappedRetroPayReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedRetroPayReservation> FindCappedRetroPayReservations(ICappedRetroPayReservationStore store, CappedRetroPayReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
