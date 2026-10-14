namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback position records by code.</summary>
public interface IFallbackPositionRecordStore
{
    FallbackPositionRecord? Find(string code);
    void Save(FallbackPositionRecord item);
    bool Remove(string code);
    List<FallbackPositionRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPositionRecordStore(Dictionary<string, FallbackPositionRecord> items, List<string> order) : IFallbackPositionRecordStore
{
    public Dictionary<string, FallbackPositionRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPositionRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPositionRecord item)
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

    public List<FallbackPositionRecord> List()
    {
        var outValue = new List<FallbackPositionRecord>();
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
public sealed class CachedFallbackPositionRecordStore(MemoryFallbackPositionRecordStore inner, FallbackPositionRecordCache cache, int hits, int misses) : IFallbackPositionRecordStore
{
    public MemoryFallbackPositionRecordStore Inner { get; set; } = inner;
    public FallbackPositionRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPositionRecord? Find(string code)
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

    public void Save(FallbackPositionRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPositionRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback position records. Empty fields match all.</summary>
public sealed record FallbackPositionRecordQuery(string Category, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPositionRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPositionRecord> Apply(List<FallbackPositionRecord> items)
    {
        var matched = Collections.Filter(items, (FallbackPositionRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback position record store.</summary>
public static class FallbackPositionRecordStore
{
    public static IFallbackPositionRecordStore NewMemoryFallbackPositionRecordStore()
    {
        return new MemoryFallbackPositionRecordStore(
            new Dictionary<string, FallbackPositionRecord>(),
            new List<string>());
    }

    public static IFallbackPositionRecordStore NewCachedFallbackPositionRecordStore(MemoryFallbackPositionRecordStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPositionRecordStore(
            inner,
            FallbackPositionRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPositionRecord> FindFallbackPositionRecords(IFallbackPositionRecordStore store, FallbackPositionRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
