namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant records by code.</summary>
public interface IEquityGrantRecordStore
{
    EquityGrantRecord? Find(string code);
    void Save(EquityGrantRecord item);
    bool Remove(string code);
    List<EquityGrantRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantRecordStore(Dictionary<string, EquityGrantRecord> items, List<string> order) : IEquityGrantRecordStore
{
    public Dictionary<string, EquityGrantRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantRecord item)
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

    public List<EquityGrantRecord> List()
    {
        var outValue = new List<EquityGrantRecord>();
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
public sealed class CachedEquityGrantRecordStore(MemoryEquityGrantRecordStore inner, EquityGrantRecordCache cache, int hits, int misses) : IEquityGrantRecordStore
{
    public MemoryEquityGrantRecordStore Inner { get; set; } = inner;
    public EquityGrantRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantRecord? Find(string code)
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

    public void Save(EquityGrantRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant records. Empty fields match all.</summary>
public sealed record EquityGrantRecordQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantRecord item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantRecord> Apply(List<EquityGrantRecord> items)
    {
        var matched = Collections.Filter(items, (EquityGrantRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant record store.</summary>
public static class EquityGrantRecordStore
{
    public static IEquityGrantRecordStore NewMemoryEquityGrantRecordStore()
    {
        return new MemoryEquityGrantRecordStore(new Dictionary<string, EquityGrantRecord>(), new List<string>());
    }

    public static IEquityGrantRecordStore NewCachedEquityGrantRecordStore(MemoryEquityGrantRecordStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantRecordStore(inner, EquityGrantRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantRecord> FindEquityGrantRecords(IEquityGrantRecordStore store, EquityGrantRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
