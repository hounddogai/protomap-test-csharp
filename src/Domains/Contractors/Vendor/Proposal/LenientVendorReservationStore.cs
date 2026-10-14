namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient vendor reservations by code.</summary>
public interface ILenientVendorReservationStore
{
    LenientVendorReservation? Find(string code);
    void Save(LenientVendorReservation item);
    bool Remove(string code);
    List<LenientVendorReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientVendorReservationStore(Dictionary<string, LenientVendorReservation> items, List<string> order) : ILenientVendorReservationStore
{
    public Dictionary<string, LenientVendorReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientVendorReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientVendorReservation item)
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

    public List<LenientVendorReservation> List()
    {
        var outValue = new List<LenientVendorReservation>();
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
public sealed class CachedLenientVendorReservationStore(MemoryLenientVendorReservationStore inner, LenientVendorReservationCache cache, int hits, int misses) : ILenientVendorReservationStore
{
    public MemoryLenientVendorReservationStore Inner { get; set; } = inner;
    public LenientVendorReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientVendorReservation? Find(string code)
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

    public void Save(LenientVendorReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientVendorReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient vendor reservations. Empty fields match all.</summary>
public sealed record LenientVendorReservationQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(LenientVendorReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<LenientVendorReservation> Apply(List<LenientVendorReservation> items)
    {
        var matched = Collections.Filter(items, (LenientVendorReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient vendor reservation store.</summary>
public static class LenientVendorReservationStore
{
    public static ILenientVendorReservationStore NewMemoryLenientVendorReservationStore()
    {
        return new MemoryLenientVendorReservationStore(
            new Dictionary<string, LenientVendorReservation>(),
            new List<string>());
    }

    public static ILenientVendorReservationStore NewCachedLenientVendorReservationStore(MemoryLenientVendorReservationStore inner, int ttl, IClock clock)
    {
        return new CachedLenientVendorReservationStore(
            inner,
            LenientVendorReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientVendorReservation> FindLenientVendorReservations(ILenientVendorReservationStore store, LenientVendorReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
