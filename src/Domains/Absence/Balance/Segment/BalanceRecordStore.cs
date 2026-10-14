namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance records by code.</summary>
public interface IBalanceRecordStore
{
    BalanceRecord? Find(string code);
    void Save(BalanceRecord item);
    bool Remove(string code);
    List<BalanceRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceRecordStore(Dictionary<string, BalanceRecord> items, List<string> order) : IBalanceRecordStore
{
    public Dictionary<string, BalanceRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceRecord item)
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

    public List<BalanceRecord> List()
    {
        var outValue = new List<BalanceRecord>();
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
public sealed class CachedBalanceRecordStore(MemoryBalanceRecordStore inner, BalanceRecordCache cache, int hits, int misses) : IBalanceRecordStore
{
    public MemoryBalanceRecordStore Inner { get; set; } = inner;
    public BalanceRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceRecord? Find(string code)
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

    public void Save(BalanceRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance records. Empty fields match all.</summary>
public sealed record BalanceRecordQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceRecord item)
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

    public List<BalanceRecord> Apply(List<BalanceRecord> items)
    {
        var matched = Collections.Filter(items, (BalanceRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance record store.</summary>
public static class BalanceRecordStore
{
    public static IBalanceRecordStore NewMemoryBalanceRecordStore()
    {
        return new MemoryBalanceRecordStore(new Dictionary<string, BalanceRecord>(), new List<string>());
    }

    public static IBalanceRecordStore NewCachedBalanceRecordStore(MemoryBalanceRecordStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceRecordStore(inner, BalanceRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceRecord> FindBalanceRecords(IBalanceRecordStore store, BalanceRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
