namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band records by code.</summary>
public interface IPayBandRecordStore
{
    PayBandRecord? Find(string code);
    void Save(PayBandRecord item);
    bool Remove(string code);
    List<PayBandRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandRecordStore(Dictionary<string, PayBandRecord> items, List<string> order) : IPayBandRecordStore
{
    public Dictionary<string, PayBandRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandRecord item)
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

    public List<PayBandRecord> List()
    {
        var outValue = new List<PayBandRecord>();
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
public sealed class CachedPayBandRecordStore(MemoryPayBandRecordStore inner, PayBandRecordCache cache, int hits, int misses) : IPayBandRecordStore
{
    public MemoryPayBandRecordStore Inner { get; set; } = inner;
    public PayBandRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandRecord? Find(string code)
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

    public void Save(PayBandRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band records. Empty fields match all.</summary>
public sealed record PayBandRecordQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<PayBandRecord> Apply(List<PayBandRecord> items)
    {
        var matched = Collections.Filter(items, (PayBandRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band record store.</summary>
public static class PayBandRecordStore
{
    public static IPayBandRecordStore NewMemoryPayBandRecordStore()
    {
        return new MemoryPayBandRecordStore(new Dictionary<string, PayBandRecord>(), new List<string>());
    }

    public static IPayBandRecordStore NewCachedPayBandRecordStore(MemoryPayBandRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandRecordStore(inner, PayBandRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayBandRecord> FindPayBandRecords(IPayBandRecordStore store, PayBandRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
