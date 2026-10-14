namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped vendor entries by code.</summary>
public interface ICappedVendorEntryStore
{
    CappedVendorEntry? Find(string code);
    void Save(CappedVendorEntry item);
    bool Remove(string code);
    List<CappedVendorEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedVendorEntryStore(Dictionary<string, CappedVendorEntry> items, List<string> order) : ICappedVendorEntryStore
{
    public Dictionary<string, CappedVendorEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedVendorEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedVendorEntry item)
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

    public List<CappedVendorEntry> List()
    {
        var outValue = new List<CappedVendorEntry>();
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
public sealed class CachedCappedVendorEntryStore(MemoryCappedVendorEntryStore inner, CappedVendorEntryCache cache, int hits, int misses) : ICappedVendorEntryStore
{
    public MemoryCappedVendorEntryStore Inner { get; set; } = inner;
    public CappedVendorEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedVendorEntry? Find(string code)
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

    public void Save(CappedVendorEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedVendorEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped vendor entries. Empty fields match all.</summary>
public sealed record CappedVendorEntryQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(CappedVendorEntry item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedVendorEntry> Apply(List<CappedVendorEntry> items)
    {
        var matched = Collections.Filter(items, (CappedVendorEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped vendor entry store.</summary>
public static class CappedVendorEntryStore
{
    public static ICappedVendorEntryStore NewMemoryCappedVendorEntryStore()
    {
        return new MemoryCappedVendorEntryStore(new Dictionary<string, CappedVendorEntry>(), new List<string>());
    }

    public static ICappedVendorEntryStore NewCachedCappedVendorEntryStore(MemoryCappedVendorEntryStore inner, int ttl, IClock clock)
    {
        return new CachedCappedVendorEntryStore(inner, CappedVendorEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedVendorEntry> FindCappedVendorEntries(ICappedVendorEntryStore store, CappedVendorEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
