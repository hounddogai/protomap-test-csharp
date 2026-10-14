namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor reservations by code.</summary>
public interface IVendorReservationStore
{
    VendorReservation? Find(string code);
    void Save(VendorReservation item);
    bool Remove(string code);
    List<VendorReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorReservationStore(Dictionary<string, VendorReservation> items, List<string> order) : IVendorReservationStore
{
    public Dictionary<string, VendorReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorReservation item)
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

    public List<VendorReservation> List()
    {
        var outValue = new List<VendorReservation>();
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
public sealed class CachedVendorReservationStore(MemoryVendorReservationStore inner, VendorReservationCache cache, int hits, int misses) : IVendorReservationStore
{
    public MemoryVendorReservationStore Inner { get; set; } = inner;
    public VendorReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorReservation? Find(string code)
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

    public void Save(VendorReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor reservations. Empty fields match all.</summary>
public sealed record VendorReservationQuery(string Region, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(VendorReservation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorReservation> Apply(List<VendorReservation> items)
    {
        var matched = Collections.Filter(items, (VendorReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor reservation store.</summary>
public static class VendorReservationStore
{
    public static IVendorReservationStore NewMemoryVendorReservationStore()
    {
        return new MemoryVendorReservationStore(new Dictionary<string, VendorReservation>(), new List<string>());
    }

    public static IVendorReservationStore NewCachedVendorReservationStore(MemoryVendorReservationStore inner, int ttl, IClock clock)
    {
        return new CachedVendorReservationStore(inner, VendorReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorReservation> FindVendorReservations(IVendorReservationStore store, VendorReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
