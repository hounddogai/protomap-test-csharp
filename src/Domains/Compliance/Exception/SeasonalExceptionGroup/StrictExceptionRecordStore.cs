namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict exception records by code.</summary>
public interface IStrictExceptionRecordStore
{
    StrictExceptionRecord? Find(string code);
    void Save(StrictExceptionRecord item);
    bool Remove(string code);
    List<StrictExceptionRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictExceptionRecordStore(Dictionary<string, StrictExceptionRecord> items, List<string> order) : IStrictExceptionRecordStore
{
    public Dictionary<string, StrictExceptionRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictExceptionRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictExceptionRecord item)
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

    public List<StrictExceptionRecord> List()
    {
        var outValue = new List<StrictExceptionRecord>();
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
public sealed class CachedStrictExceptionRecordStore(MemoryStrictExceptionRecordStore inner, StrictExceptionRecordCache cache, int hits, int misses) : IStrictExceptionRecordStore
{
    public MemoryStrictExceptionRecordStore Inner { get; set; } = inner;
    public StrictExceptionRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictExceptionRecord? Find(string code)
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

    public void Save(StrictExceptionRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictExceptionRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict exception records. Empty fields match all.</summary>
public sealed record StrictExceptionRecordQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(StrictExceptionRecord item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictExceptionRecord> Apply(List<StrictExceptionRecord> items)
    {
        var matched = Collections.Filter(items, (StrictExceptionRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict exception record store.</summary>
public static class StrictExceptionRecordStore
{
    public static IStrictExceptionRecordStore NewMemoryStrictExceptionRecordStore()
    {
        return new MemoryStrictExceptionRecordStore(
            new Dictionary<string, StrictExceptionRecord>(),
            new List<string>());
    }

    public static IStrictExceptionRecordStore NewCachedStrictExceptionRecordStore(MemoryStrictExceptionRecordStore inner, int ttl, IClock clock)
    {
        return new CachedStrictExceptionRecordStore(inner, StrictExceptionRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictExceptionRecord> FindStrictExceptionRecords(IStrictExceptionRecordStore store, StrictExceptionRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
