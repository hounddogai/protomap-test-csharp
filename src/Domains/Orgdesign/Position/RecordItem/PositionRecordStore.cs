namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position records by code.</summary>
public interface IPositionRecordStore
{
    PositionRecord? Find(string code);
    void Save(PositionRecord item);
    bool Remove(string code);
    List<PositionRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionRecordStore(Dictionary<string, PositionRecord> items, List<string> order) : IPositionRecordStore
{
    public Dictionary<string, PositionRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionRecord item)
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

    public List<PositionRecord> List()
    {
        var outValue = new List<PositionRecord>();
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
public sealed class CachedPositionRecordStore(MemoryPositionRecordStore inner, PositionRecordCache cache, int hits, int misses) : IPositionRecordStore
{
    public MemoryPositionRecordStore Inner { get; set; } = inner;
    public PositionRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionRecord? Find(string code)
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

    public void Save(PositionRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position records. Empty fields match all.</summary>
public sealed record PositionRecordQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PositionRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionRecord> Apply(List<PositionRecord> items)
    {
        var matched = Collections.Filter(items, (PositionRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position record store.</summary>
public static class PositionRecordStore
{
    public static IPositionRecordStore NewMemoryPositionRecordStore()
    {
        return new MemoryPositionRecordStore(new Dictionary<string, PositionRecord>(), new List<string>());
    }

    public static IPositionRecordStore NewCachedPositionRecordStore(MemoryPositionRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPositionRecordStore(inner, PositionRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PositionRecord> FindPositionRecords(IPositionRecordStore store, PositionRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
