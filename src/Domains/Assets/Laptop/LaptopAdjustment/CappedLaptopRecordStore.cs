namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped laptop records by code.</summary>
public interface ICappedLaptopRecordStore
{
    CappedLaptopRecord? Find(string code);
    void Save(CappedLaptopRecord item);
    bool Remove(string code);
    List<CappedLaptopRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLaptopRecordStore(Dictionary<string, CappedLaptopRecord> items, List<string> order) : ICappedLaptopRecordStore
{
    public Dictionary<string, CappedLaptopRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLaptopRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLaptopRecord item)
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

    public List<CappedLaptopRecord> List()
    {
        var outValue = new List<CappedLaptopRecord>();
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
public sealed class CachedCappedLaptopRecordStore(MemoryCappedLaptopRecordStore inner, CappedLaptopRecordCache cache, int hits, int misses) : ICappedLaptopRecordStore
{
    public MemoryCappedLaptopRecordStore Inner { get; set; } = inner;
    public CappedLaptopRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLaptopRecord? Find(string code)
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

    public void Save(CappedLaptopRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLaptopRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped laptop records. Empty fields match all.</summary>
public sealed record CappedLaptopRecordQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLaptopRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedLaptopRecord> Apply(List<CappedLaptopRecord> items)
    {
        var matched = Collections.Filter(items, (CappedLaptopRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped laptop record store.</summary>
public static class CappedLaptopRecordStore
{
    public static ICappedLaptopRecordStore NewMemoryCappedLaptopRecordStore()
    {
        return new MemoryCappedLaptopRecordStore(new Dictionary<string, CappedLaptopRecord>(), new List<string>());
    }

    public static ICappedLaptopRecordStore NewCachedCappedLaptopRecordStore(MemoryCappedLaptopRecordStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLaptopRecordStore(inner, CappedLaptopRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedLaptopRecord> FindCappedLaptopRecords(ICappedLaptopRecordStore store, CappedLaptopRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
