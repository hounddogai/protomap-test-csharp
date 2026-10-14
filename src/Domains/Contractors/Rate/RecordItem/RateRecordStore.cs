namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate records by code.</summary>
public interface IRateRecordStore
{
    RateRecord? Find(string code);
    void Save(RateRecord item);
    bool Remove(string code);
    List<RateRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateRecordStore(Dictionary<string, RateRecord> items, List<string> order) : IRateRecordStore
{
    public Dictionary<string, RateRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateRecord item)
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

    public List<RateRecord> List()
    {
        var outValue = new List<RateRecord>();
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
public sealed class CachedRateRecordStore(MemoryRateRecordStore inner, RateRecordCache cache, int hits, int misses) : IRateRecordStore
{
    public MemoryRateRecordStore Inner { get; set; } = inner;
    public RateRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateRecord? Find(string code)
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

    public void Save(RateRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate records. Empty fields match all.</summary>
public sealed record RateRecordQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RateRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateRecord> Apply(List<RateRecord> items)
    {
        var matched = Collections.Filter(items, (RateRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate record store.</summary>
public static class RateRecordStore
{
    public static IRateRecordStore NewMemoryRateRecordStore()
    {
        return new MemoryRateRecordStore(new Dictionary<string, RateRecord>(), new List<string>());
    }

    public static IRateRecordStore NewCachedRateRecordStore(MemoryRateRecordStore inner, int ttl, IClock clock)
    {
        return new CachedRateRecordStore(inner, RateRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RateRecord> FindRateRecords(IRateRecordStore store, RateRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
