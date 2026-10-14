namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered equity grant records by code.</summary>
public interface ITieredEquityGrantRecordStore
{
    TieredEquityGrantRecord? Find(string code);
    void Save(TieredEquityGrantRecord item);
    bool Remove(string code);
    List<TieredEquityGrantRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredEquityGrantRecordStore(Dictionary<string, TieredEquityGrantRecord> items, List<string> order) : ITieredEquityGrantRecordStore
{
    public Dictionary<string, TieredEquityGrantRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredEquityGrantRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredEquityGrantRecord item)
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

    public List<TieredEquityGrantRecord> List()
    {
        var outValue = new List<TieredEquityGrantRecord>();
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
public sealed class CachedTieredEquityGrantRecordStore(MemoryTieredEquityGrantRecordStore inner, TieredEquityGrantRecordCache cache, int hits, int misses) : ITieredEquityGrantRecordStore
{
    public MemoryTieredEquityGrantRecordStore Inner { get; set; } = inner;
    public TieredEquityGrantRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredEquityGrantRecord? Find(string code)
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

    public void Save(TieredEquityGrantRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredEquityGrantRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered equity grant records. Empty fields match all.</summary>
public sealed record TieredEquityGrantRecordQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TieredEquityGrantRecord item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredEquityGrantRecord> Apply(List<TieredEquityGrantRecord> items)
    {
        var matched = Collections.Filter(items, (TieredEquityGrantRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered equity grant record store.</summary>
public static class TieredEquityGrantRecordStore
{
    public static ITieredEquityGrantRecordStore NewMemoryTieredEquityGrantRecordStore()
    {
        return new MemoryTieredEquityGrantRecordStore(
            new Dictionary<string, TieredEquityGrantRecord>(),
            new List<string>());
    }

    public static ITieredEquityGrantRecordStore NewCachedTieredEquityGrantRecordStore(MemoryTieredEquityGrantRecordStore inner, int ttl, IClock clock)
    {
        return new CachedTieredEquityGrantRecordStore(
            inner,
            TieredEquityGrantRecordCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredEquityGrantRecord> FindTieredEquityGrantRecords(ITieredEquityGrantRecordStore store, TieredEquityGrantRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
