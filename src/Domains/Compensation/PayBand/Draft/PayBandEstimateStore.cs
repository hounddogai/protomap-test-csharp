namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band estimates by code.</summary>
public interface IPayBandEstimateStore
{
    PayBandEstimate? Find(string code);
    void Save(PayBandEstimate item);
    bool Remove(string code);
    List<PayBandEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandEstimateStore(Dictionary<string, PayBandEstimate> items, List<string> order) : IPayBandEstimateStore
{
    public Dictionary<string, PayBandEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandEstimate item)
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

    public List<PayBandEstimate> List()
    {
        var outValue = new List<PayBandEstimate>();
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
public sealed class CachedPayBandEstimateStore(MemoryPayBandEstimateStore inner, PayBandEstimateCache cache, int hits, int misses) : IPayBandEstimateStore
{
    public MemoryPayBandEstimateStore Inner { get; set; } = inner;
    public PayBandEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandEstimate? Find(string code)
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

    public void Save(PayBandEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band estimates. Empty fields match all.</summary>
public sealed record PayBandEstimateQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandEstimate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<PayBandEstimate> Apply(List<PayBandEstimate> items)
    {
        var matched = Collections.Filter(items, (PayBandEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band estimate store.</summary>
public static class PayBandEstimateStore
{
    public static IPayBandEstimateStore NewMemoryPayBandEstimateStore()
    {
        return new MemoryPayBandEstimateStore(new Dictionary<string, PayBandEstimate>(), new List<string>());
    }

    public static IPayBandEstimateStore NewCachedPayBandEstimateStore(MemoryPayBandEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandEstimateStore(inner, PayBandEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayBandEstimate> FindPayBandEstimates(IPayBandEstimateStore store, PayBandEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
