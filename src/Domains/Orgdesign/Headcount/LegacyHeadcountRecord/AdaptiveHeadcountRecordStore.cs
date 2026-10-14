namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive headcount records by code.</summary>
public interface IAdaptiveHeadcountRecordStore
{
    AdaptiveHeadcountRecord? Find(string code);
    void Save(AdaptiveHeadcountRecord item);
    bool Remove(string code);
    List<AdaptiveHeadcountRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveHeadcountRecordStore(Dictionary<string, AdaptiveHeadcountRecord> items, List<string> order) : IAdaptiveHeadcountRecordStore
{
    public Dictionary<string, AdaptiveHeadcountRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveHeadcountRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveHeadcountRecord item)
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

    public List<AdaptiveHeadcountRecord> List()
    {
        var outValue = new List<AdaptiveHeadcountRecord>();
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
public sealed class CachedAdaptiveHeadcountRecordStore(MemoryAdaptiveHeadcountRecordStore inner, AdaptiveHeadcountRecordCache cache, int hits, int misses) : IAdaptiveHeadcountRecordStore
{
    public MemoryAdaptiveHeadcountRecordStore Inner { get; set; } = inner;
    public AdaptiveHeadcountRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveHeadcountRecord? Find(string code)
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

    public void Save(AdaptiveHeadcountRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveHeadcountRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive headcount records. Empty fields match all.</summary>
public sealed record AdaptiveHeadcountRecordQuery(string Region, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveHeadcountRecord item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveHeadcountRecord> Apply(List<AdaptiveHeadcountRecord> items)
    {
        var matched = Collections.Filter(items, (AdaptiveHeadcountRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive headcount record store.</summary>
public static class AdaptiveHeadcountRecordStore
{
    public static IAdaptiveHeadcountRecordStore NewMemoryAdaptiveHeadcountRecordStore()
    {
        return new MemoryAdaptiveHeadcountRecordStore(
            new Dictionary<string, AdaptiveHeadcountRecord>(),
            new List<string>());
    }

    public static IAdaptiveHeadcountRecordStore NewCachedAdaptiveHeadcountRecordStore(MemoryAdaptiveHeadcountRecordStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveHeadcountRecordStore(
            inner,
            AdaptiveHeadcountRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveHeadcountRecord> FindAdaptiveHeadcountRecords(IAdaptiveHeadcountRecordStore store, AdaptiveHeadcountRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
