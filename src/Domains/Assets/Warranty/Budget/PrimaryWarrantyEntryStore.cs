namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary warranty entries by code.</summary>
public interface IPrimaryWarrantyEntryStore
{
    PrimaryWarrantyEntry? Find(string code);
    void Save(PrimaryWarrantyEntry item);
    bool Remove(string code);
    List<PrimaryWarrantyEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryWarrantyEntryStore(Dictionary<string, PrimaryWarrantyEntry> items, List<string> order) : IPrimaryWarrantyEntryStore
{
    public Dictionary<string, PrimaryWarrantyEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryWarrantyEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryWarrantyEntry item)
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

    public List<PrimaryWarrantyEntry> List()
    {
        var outValue = new List<PrimaryWarrantyEntry>();
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
public sealed class CachedPrimaryWarrantyEntryStore(MemoryPrimaryWarrantyEntryStore inner, PrimaryWarrantyEntryCache cache, int hits, int misses) : IPrimaryWarrantyEntryStore
{
    public MemoryPrimaryWarrantyEntryStore Inner { get; set; } = inner;
    public PrimaryWarrantyEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryWarrantyEntry? Find(string code)
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

    public void Save(PrimaryWarrantyEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryWarrantyEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary warranty entries. Empty fields match all.</summary>
public sealed record PrimaryWarrantyEntryQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryWarrantyEntry item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryWarrantyEntry> Apply(List<PrimaryWarrantyEntry> items)
    {
        var matched = Collections.Filter(items, (PrimaryWarrantyEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary warranty entry store.</summary>
public static class PrimaryWarrantyEntryStore
{
    public static IPrimaryWarrantyEntryStore NewMemoryPrimaryWarrantyEntryStore()
    {
        return new MemoryPrimaryWarrantyEntryStore(
            new Dictionary<string, PrimaryWarrantyEntry>(),
            new List<string>());
    }

    public static IPrimaryWarrantyEntryStore NewCachedPrimaryWarrantyEntryStore(MemoryPrimaryWarrantyEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryWarrantyEntryStore(inner, PrimaryWarrantyEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryWarrantyEntry> FindPrimaryWarrantyEntries(IPrimaryWarrantyEntryStore store, PrimaryWarrantyEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
