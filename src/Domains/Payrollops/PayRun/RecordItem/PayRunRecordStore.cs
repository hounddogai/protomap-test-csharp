namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run records by code.</summary>
public interface IPayRunRecordStore
{
    PayRunRecord? Find(string code);
    void Save(PayRunRecord item);
    bool Remove(string code);
    List<PayRunRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunRecordStore(Dictionary<string, PayRunRecord> items, List<string> order) : IPayRunRecordStore
{
    public Dictionary<string, PayRunRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunRecord item)
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

    public List<PayRunRecord> List()
    {
        var outValue = new List<PayRunRecord>();
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
public sealed class CachedPayRunRecordStore(MemoryPayRunRecordStore inner, PayRunRecordCache cache, int hits, int misses) : IPayRunRecordStore
{
    public MemoryPayRunRecordStore Inner { get; set; } = inner;
    public PayRunRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunRecord? Find(string code)
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

    public void Save(PayRunRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run records. Empty fields match all.</summary>
public sealed record PayRunRecordQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunRecord> Apply(List<PayRunRecord> items)
    {
        var matched = Collections.Filter(items, (PayRunRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run record store.</summary>
public static class PayRunRecordStore
{
    public static IPayRunRecordStore NewMemoryPayRunRecordStore()
    {
        return new MemoryPayRunRecordStore(new Dictionary<string, PayRunRecord>(), new List<string>());
    }

    public static IPayRunRecordStore NewCachedPayRunRecordStore(MemoryPayRunRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunRecordStore(inner, PayRunRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunRecord> FindPayRunRecords(IPayRunRecordStore store, PayRunRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
