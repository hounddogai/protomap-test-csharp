namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered offboarding records by code.</summary>
public interface ITieredOffboardingRecordStore
{
    TieredOffboardingRecord? Find(string code);
    void Save(TieredOffboardingRecord item);
    bool Remove(string code);
    List<TieredOffboardingRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredOffboardingRecordStore(Dictionary<string, TieredOffboardingRecord> items, List<string> order) : ITieredOffboardingRecordStore
{
    public Dictionary<string, TieredOffboardingRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredOffboardingRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredOffboardingRecord item)
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

    public List<TieredOffboardingRecord> List()
    {
        var outValue = new List<TieredOffboardingRecord>();
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
public sealed class CachedTieredOffboardingRecordStore(MemoryTieredOffboardingRecordStore inner, TieredOffboardingRecordCache cache, int hits, int misses) : ITieredOffboardingRecordStore
{
    public MemoryTieredOffboardingRecordStore Inner { get; set; } = inner;
    public TieredOffboardingRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredOffboardingRecord? Find(string code)
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

    public void Save(TieredOffboardingRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredOffboardingRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered offboarding records. Empty fields match all.</summary>
public sealed record TieredOffboardingRecordQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TieredOffboardingRecord item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<TieredOffboardingRecord> Apply(List<TieredOffboardingRecord> items)
    {
        var matched = Collections.Filter(items, (TieredOffboardingRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered offboarding record store.</summary>
public static class TieredOffboardingRecordStore
{
    public static ITieredOffboardingRecordStore NewMemoryTieredOffboardingRecordStore()
    {
        return new MemoryTieredOffboardingRecordStore(
            new Dictionary<string, TieredOffboardingRecord>(),
            new List<string>());
    }

    public static ITieredOffboardingRecordStore NewCachedTieredOffboardingRecordStore(MemoryTieredOffboardingRecordStore inner, int ttl, IClock clock)
    {
        return new CachedTieredOffboardingRecordStore(
            inner,
            TieredOffboardingRecordCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<TieredOffboardingRecord> FindTieredOffboardingRecords(ITieredOffboardingRecordStore store, TieredOffboardingRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
