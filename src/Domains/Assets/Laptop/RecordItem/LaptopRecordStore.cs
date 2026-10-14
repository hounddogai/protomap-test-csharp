namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop records by code.</summary>
public interface ILaptopRecordStore
{
    LaptopRecord? Find(string code);
    void Save(LaptopRecord item);
    bool Remove(string code);
    List<LaptopRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopRecordStore(Dictionary<string, LaptopRecord> items, List<string> order) : ILaptopRecordStore
{
    public Dictionary<string, LaptopRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopRecord item)
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

    public List<LaptopRecord> List()
    {
        var outValue = new List<LaptopRecord>();
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
public sealed class CachedLaptopRecordStore(MemoryLaptopRecordStore inner, LaptopRecordCache cache, int hits, int misses) : ILaptopRecordStore
{
    public MemoryLaptopRecordStore Inner { get; set; } = inner;
    public LaptopRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopRecord? Find(string code)
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

    public void Save(LaptopRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop records. Empty fields match all.</summary>
public sealed record LaptopRecordQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopRecord item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopRecord> Apply(List<LaptopRecord> items)
    {
        var matched = Collections.Filter(items, (LaptopRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop record store.</summary>
public static class LaptopRecordStore
{
    public static ILaptopRecordStore NewMemoryLaptopRecordStore()
    {
        return new MemoryLaptopRecordStore(new Dictionary<string, LaptopRecord>(), new List<string>());
    }

    public static ILaptopRecordStore NewCachedLaptopRecordStore(MemoryLaptopRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopRecordStore(inner, LaptopRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopRecord> FindLaptopRecords(ILaptopRecordStore store, LaptopRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
