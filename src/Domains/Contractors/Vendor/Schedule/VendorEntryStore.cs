namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor entries by code.</summary>
public interface IVendorEntryStore
{
    VendorEntry? Find(string code);
    void Save(VendorEntry item);
    bool Remove(string code);
    List<VendorEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorEntryStore(Dictionary<string, VendorEntry> items, List<string> order) : IVendorEntryStore
{
    public Dictionary<string, VendorEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorEntry item)
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

    public List<VendorEntry> List()
    {
        var outValue = new List<VendorEntry>();
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
public sealed class CachedVendorEntryStore(MemoryVendorEntryStore inner, VendorEntryCache cache, int hits, int misses) : IVendorEntryStore
{
    public MemoryVendorEntryStore Inner { get; set; } = inner;
    public VendorEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorEntry? Find(string code)
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

    public void Save(VendorEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor entries. Empty fields match all.</summary>
public sealed record VendorEntryQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(VendorEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorEntry> Apply(List<VendorEntry> items)
    {
        var matched = Collections.Filter(items, (VendorEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor entry store.</summary>
public static class VendorEntryStore
{
    public static IVendorEntryStore NewMemoryVendorEntryStore()
    {
        return new MemoryVendorEntryStore(new Dictionary<string, VendorEntry>(), new List<string>());
    }

    public static IVendorEntryStore NewCachedVendorEntryStore(MemoryVendorEntryStore inner, int ttl, IClock clock)
    {
        return new CachedVendorEntryStore(inner, VendorEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VendorEntry> FindVendorEntries(IVendorEntryStore store, VendorEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
