namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty entries by code.</summary>
public interface IWarrantyEntryStore
{
    WarrantyEntry? Find(string code);
    void Save(WarrantyEntry item);
    bool Remove(string code);
    List<WarrantyEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyEntryStore(Dictionary<string, WarrantyEntry> items, List<string> order) : IWarrantyEntryStore
{
    public Dictionary<string, WarrantyEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyEntry item)
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

    public List<WarrantyEntry> List()
    {
        var outValue = new List<WarrantyEntry>();
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
public sealed class CachedWarrantyEntryStore(MemoryWarrantyEntryStore inner, WarrantyEntryCache cache, int hits, int misses) : IWarrantyEntryStore
{
    public MemoryWarrantyEntryStore Inner { get; set; } = inner;
    public WarrantyEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyEntry? Find(string code)
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

    public void Save(WarrantyEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty entries. Empty fields match all.</summary>
public sealed record WarrantyEntryQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyEntry item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyEntry> Apply(List<WarrantyEntry> items)
    {
        var matched = Collections.Filter(items, (WarrantyEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty entry store.</summary>
public static class WarrantyEntryStore
{
    public static IWarrantyEntryStore NewMemoryWarrantyEntryStore()
    {
        return new MemoryWarrantyEntryStore(new Dictionary<string, WarrantyEntry>(), new List<string>());
    }

    public static IWarrantyEntryStore NewCachedWarrantyEntryStore(MemoryWarrantyEntryStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyEntryStore(inner, WarrantyEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyEntry> FindWarrantyEntries(IWarrantyEntryStore store, WarrantyEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
