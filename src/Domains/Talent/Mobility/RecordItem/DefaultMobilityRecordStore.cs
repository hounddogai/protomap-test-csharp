namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default mobility records by code.</summary>
public interface IDefaultMobilityRecordStore
{
    DefaultMobilityRecord? Find(string code);
    void Save(DefaultMobilityRecord item);
    bool Remove(string code);
    List<DefaultMobilityRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultMobilityRecordStore(Dictionary<string, DefaultMobilityRecord> items, List<string> order) : IDefaultMobilityRecordStore
{
    public Dictionary<string, DefaultMobilityRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultMobilityRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultMobilityRecord item)
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

    public List<DefaultMobilityRecord> List()
    {
        var outValue = new List<DefaultMobilityRecord>();
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
public sealed class CachedDefaultMobilityRecordStore(MemoryDefaultMobilityRecordStore inner, DefaultMobilityRecordCache cache, int hits, int misses) : IDefaultMobilityRecordStore
{
    public MemoryDefaultMobilityRecordStore Inner { get; set; } = inner;
    public DefaultMobilityRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultMobilityRecord? Find(string code)
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

    public void Save(DefaultMobilityRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultMobilityRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default mobility records. Empty fields match all.</summary>
public sealed record DefaultMobilityRecordQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultMobilityRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultMobilityRecord> Apply(List<DefaultMobilityRecord> items)
    {
        var matched = Collections.Filter(items, (DefaultMobilityRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default mobility record store.</summary>
public static class DefaultMobilityRecordStore
{
    public static IDefaultMobilityRecordStore NewMemoryDefaultMobilityRecordStore()
    {
        return new MemoryDefaultMobilityRecordStore(
            new Dictionary<string, DefaultMobilityRecord>(),
            new List<string>());
    }

    public static IDefaultMobilityRecordStore NewCachedDefaultMobilityRecordStore(MemoryDefaultMobilityRecordStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultMobilityRecordStore(inner, DefaultMobilityRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultMobilityRecord> FindDefaultMobilityRecords(IDefaultMobilityRecordStore store, DefaultMobilityRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
