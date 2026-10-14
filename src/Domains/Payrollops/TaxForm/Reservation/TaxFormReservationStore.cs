namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form reservations by code.</summary>
public interface ITaxFormReservationStore
{
    TaxFormReservation? Find(string code);
    void Save(TaxFormReservation item);
    bool Remove(string code);
    List<TaxFormReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormReservationStore(Dictionary<string, TaxFormReservation> items, List<string> order) : ITaxFormReservationStore
{
    public Dictionary<string, TaxFormReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormReservation item)
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

    public List<TaxFormReservation> List()
    {
        var outValue = new List<TaxFormReservation>();
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
public sealed class CachedTaxFormReservationStore(MemoryTaxFormReservationStore inner, TaxFormReservationCache cache, int hits, int misses) : ITaxFormReservationStore
{
    public MemoryTaxFormReservationStore Inner { get; set; } = inner;
    public TaxFormReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormReservation? Find(string code)
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

    public void Save(TaxFormReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form reservations. Empty fields match all.</summary>
public sealed record TaxFormReservationQuery(string Region, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormReservation> Apply(List<TaxFormReservation> items)
    {
        var matched = Collections.Filter(items, (TaxFormReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form reservation store.</summary>
public static class TaxFormReservationStore
{
    public static ITaxFormReservationStore NewMemoryTaxFormReservationStore()
    {
        return new MemoryTaxFormReservationStore(new Dictionary<string, TaxFormReservation>(), new List<string>());
    }

    public static ITaxFormReservationStore NewCachedTaxFormReservationStore(MemoryTaxFormReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormReservationStore(inner, TaxFormReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormReservation> FindTaxFormReservations(ITaxFormReservationStore store, TaxFormReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
