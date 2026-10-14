namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual mobility records by code.</summary>
public interface IManualMobilityRecordStore
{
    ManualMobilityRecord? Find(string code);
    void Save(ManualMobilityRecord item);
    bool Remove(string code);
    List<ManualMobilityRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualMobilityRecordStore(Dictionary<string, ManualMobilityRecord> items, List<string> order) : IManualMobilityRecordStore
{
    public Dictionary<string, ManualMobilityRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualMobilityRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualMobilityRecord item)
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

    public List<ManualMobilityRecord> List()
    {
        var outValue = new List<ManualMobilityRecord>();
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
public sealed class CachedManualMobilityRecordStore(MemoryManualMobilityRecordStore inner, ManualMobilityRecordCache cache, int hits, int misses) : IManualMobilityRecordStore
{
    public MemoryManualMobilityRecordStore Inner { get; set; } = inner;
    public ManualMobilityRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualMobilityRecord? Find(string code)
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

    public void Save(ManualMobilityRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualMobilityRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual mobility records. Empty fields match all.</summary>
public sealed record ManualMobilityRecordQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(ManualMobilityRecord item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ManualMobilityRecord> Apply(List<ManualMobilityRecord> items)
    {
        var matched = Collections.Filter(items, (ManualMobilityRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual mobility record store.</summary>
public static class ManualMobilityRecordStore
{
    public static IManualMobilityRecordStore NewMemoryManualMobilityRecordStore()
    {
        return new MemoryManualMobilityRecordStore(
            new Dictionary<string, ManualMobilityRecord>(),
            new List<string>());
    }

    public static IManualMobilityRecordStore NewCachedManualMobilityRecordStore(MemoryManualMobilityRecordStore inner, int ttl, IClock clock)
    {
        return new CachedManualMobilityRecordStore(inner, ManualMobilityRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualMobilityRecord> FindManualMobilityRecords(IManualMobilityRecordStore store, ManualMobilityRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
