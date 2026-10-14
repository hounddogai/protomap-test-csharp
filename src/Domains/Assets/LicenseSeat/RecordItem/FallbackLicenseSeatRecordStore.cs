namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback license seat records by code.</summary>
public interface IFallbackLicenseSeatRecordStore
{
    FallbackLicenseSeatRecord? Find(string code);
    void Save(FallbackLicenseSeatRecord item);
    bool Remove(string code);
    List<FallbackLicenseSeatRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLicenseSeatRecordStore(Dictionary<string, FallbackLicenseSeatRecord> items, List<string> order) : IFallbackLicenseSeatRecordStore
{
    public Dictionary<string, FallbackLicenseSeatRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLicenseSeatRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLicenseSeatRecord item)
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

    public List<FallbackLicenseSeatRecord> List()
    {
        var outValue = new List<FallbackLicenseSeatRecord>();
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
public sealed class CachedFallbackLicenseSeatRecordStore(MemoryFallbackLicenseSeatRecordStore inner, FallbackLicenseSeatRecordCache cache, int hits, int misses) : IFallbackLicenseSeatRecordStore
{
    public MemoryFallbackLicenseSeatRecordStore Inner { get; set; } = inner;
    public FallbackLicenseSeatRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLicenseSeatRecord? Find(string code)
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

    public void Save(FallbackLicenseSeatRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLicenseSeatRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback license seat records. Empty fields match all.</summary>
public sealed record FallbackLicenseSeatRecordQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLicenseSeatRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLicenseSeatRecord> Apply(List<FallbackLicenseSeatRecord> items)
    {
        var matched = Collections.Filter(items, (FallbackLicenseSeatRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback license seat record store.</summary>
public static class FallbackLicenseSeatRecordStore
{
    public static IFallbackLicenseSeatRecordStore NewMemoryFallbackLicenseSeatRecordStore()
    {
        return new MemoryFallbackLicenseSeatRecordStore(
            new Dictionary<string, FallbackLicenseSeatRecord>(),
            new List<string>());
    }

    public static IFallbackLicenseSeatRecordStore NewCachedFallbackLicenseSeatRecordStore(MemoryFallbackLicenseSeatRecordStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLicenseSeatRecordStore(
            inner,
            FallbackLicenseSeatRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLicenseSeatRecord> FindFallbackLicenseSeatRecords(IFallbackLicenseSeatRecordStore store, FallbackLicenseSeatRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
