namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor records by code.</summary>
public interface IVendorRecordStore
{
    VendorRecord? Find(string code);
    void Save(VendorRecord item);
    bool Remove(string code);
    List<VendorRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorRecordStore(Dictionary<string, VendorRecord> items, List<string> order) : IVendorRecordStore
{
    public Dictionary<string, VendorRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorRecord item)
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

    public List<VendorRecord> List()
    {
        var outValue = new List<VendorRecord>();
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
public sealed class CachedVendorRecordStore(MemoryVendorRecordStore inner, VendorRecordCache cache, int hits, int misses) : IVendorRecordStore
{
    public MemoryVendorRecordStore Inner { get; set; } = inner;
    public VendorRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorRecord? Find(string code)
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

    public void Save(VendorRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor records. Empty fields match all.</summary>
public sealed record VendorRecordQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VendorRecord item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorRecord> Apply(List<VendorRecord> items)
    {
        var matched = Collections.Filter(items, (VendorRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor record store.</summary>
public static class VendorRecordStore
{
    public static IVendorRecordStore NewMemoryVendorRecordStore()
    {
        return new MemoryVendorRecordStore(new Dictionary<string, VendorRecord>(), new List<string>());
    }

    public static IVendorRecordStore NewCachedVendorRecordStore(MemoryVendorRecordStore inner, int ttl, IClock clock)
    {
        return new CachedVendorRecordStore(inner, VendorRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorRecord> FindVendorRecords(IVendorRecordStore store, VendorRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
