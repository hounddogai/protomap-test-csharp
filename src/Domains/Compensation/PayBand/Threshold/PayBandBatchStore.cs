namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band batches by code.</summary>
public interface IPayBandBatchStore
{
    PayBandBatch? Find(string code);
    void Save(PayBandBatch item);
    bool Remove(string code);
    List<PayBandBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandBatchStore(Dictionary<string, PayBandBatch> items, List<string> order) : IPayBandBatchStore
{
    public Dictionary<string, PayBandBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandBatch item)
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

    public List<PayBandBatch> List()
    {
        var outValue = new List<PayBandBatch>();
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
public sealed class CachedPayBandBatchStore(MemoryPayBandBatchStore inner, PayBandBatchCache cache, int hits, int misses) : IPayBandBatchStore
{
    public MemoryPayBandBatchStore Inner { get; set; } = inner;
    public PayBandBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandBatch? Find(string code)
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

    public void Save(PayBandBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band batches. Empty fields match all.</summary>
public sealed record PayBandBatchQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandBatch item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandBatch> Apply(List<PayBandBatch> items)
    {
        var matched = Collections.Filter(items, (PayBandBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band batch store.</summary>
public static class PayBandBatchStore
{
    public static IPayBandBatchStore NewMemoryPayBandBatchStore()
    {
        return new MemoryPayBandBatchStore(new Dictionary<string, PayBandBatch>(), new List<string>());
    }

    public static IPayBandBatchStore NewCachedPayBandBatchStore(MemoryPayBandBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandBatchStore(inner, PayBandBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandBatch> FindPayBandBatches(IPayBandBatchStore store, PayBandBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
