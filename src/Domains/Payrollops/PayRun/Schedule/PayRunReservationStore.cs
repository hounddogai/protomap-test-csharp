namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run reservations by code.</summary>
public interface IPayRunReservationStore
{
    PayRunReservation? Find(string code);
    void Save(PayRunReservation item);
    bool Remove(string code);
    List<PayRunReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunReservationStore(Dictionary<string, PayRunReservation> items, List<string> order) : IPayRunReservationStore
{
    public Dictionary<string, PayRunReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunReservation item)
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

    public List<PayRunReservation> List()
    {
        var outValue = new List<PayRunReservation>();
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
public sealed class CachedPayRunReservationStore(MemoryPayRunReservationStore inner, PayRunReservationCache cache, int hits, int misses) : IPayRunReservationStore
{
    public MemoryPayRunReservationStore Inner { get; set; } = inner;
    public PayRunReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunReservation? Find(string code)
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

    public void Save(PayRunReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run reservations. Empty fields match all.</summary>
public sealed record PayRunReservationQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunReservation> Apply(List<PayRunReservation> items)
    {
        var matched = Collections.Filter(items, (PayRunReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run reservation store.</summary>
public static class PayRunReservationStore
{
    public static IPayRunReservationStore NewMemoryPayRunReservationStore()
    {
        return new MemoryPayRunReservationStore(new Dictionary<string, PayRunReservation>(), new List<string>());
    }

    public static IPayRunReservationStore NewCachedPayRunReservationStore(MemoryPayRunReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunReservationStore(inner, PayRunReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunReservation> FindPayRunReservations(IPayRunReservationStore store, PayRunReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
