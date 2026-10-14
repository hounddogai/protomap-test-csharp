namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback garnishment records by code.</summary>
public interface IFallbackGarnishmentRecordStore
{
    FallbackGarnishmentRecord? Find(string code);
    void Save(FallbackGarnishmentRecord item);
    bool Remove(string code);
    List<FallbackGarnishmentRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackGarnishmentRecordStore(Dictionary<string, FallbackGarnishmentRecord> items, List<string> order) : IFallbackGarnishmentRecordStore
{
    public Dictionary<string, FallbackGarnishmentRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackGarnishmentRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackGarnishmentRecord item)
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

    public List<FallbackGarnishmentRecord> List()
    {
        var outValue = new List<FallbackGarnishmentRecord>();
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
public sealed class CachedFallbackGarnishmentRecordStore(MemoryFallbackGarnishmentRecordStore inner, FallbackGarnishmentRecordCache cache, int hits, int misses) : IFallbackGarnishmentRecordStore
{
    public MemoryFallbackGarnishmentRecordStore Inner { get; set; } = inner;
    public FallbackGarnishmentRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackGarnishmentRecord? Find(string code)
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

    public void Save(FallbackGarnishmentRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackGarnishmentRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback garnishment records. Empty fields match all.</summary>
public sealed record FallbackGarnishmentRecordQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackGarnishmentRecord item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackGarnishmentRecord> Apply(List<FallbackGarnishmentRecord> items)
    {
        var matched = Collections.Filter(items, (FallbackGarnishmentRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback garnishment record store.</summary>
public static class FallbackGarnishmentRecordStore
{
    public static IFallbackGarnishmentRecordStore NewMemoryFallbackGarnishmentRecordStore()
    {
        return new MemoryFallbackGarnishmentRecordStore(
            new Dictionary<string, FallbackGarnishmentRecord>(),
            new List<string>());
    }

    public static IFallbackGarnishmentRecordStore NewCachedFallbackGarnishmentRecordStore(MemoryFallbackGarnishmentRecordStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackGarnishmentRecordStore(
            inner,
            FallbackGarnishmentRecordCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackGarnishmentRecord> FindFallbackGarnishmentRecords(IFallbackGarnishmentRecordStore store, FallbackGarnishmentRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
