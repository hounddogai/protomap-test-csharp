namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band adjustments by code.</summary>
public interface IPayBandAdjustmentStore
{
    PayBandAdjustment? Find(string code);
    void Save(PayBandAdjustment item);
    bool Remove(string code);
    List<PayBandAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandAdjustmentStore(Dictionary<string, PayBandAdjustment> items, List<string> order) : IPayBandAdjustmentStore
{
    public Dictionary<string, PayBandAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandAdjustment item)
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

    public List<PayBandAdjustment> List()
    {
        var outValue = new List<PayBandAdjustment>();
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
public sealed class CachedPayBandAdjustmentStore(MemoryPayBandAdjustmentStore inner, PayBandAdjustmentCache cache, int hits, int misses) : IPayBandAdjustmentStore
{
    public MemoryPayBandAdjustmentStore Inner { get; set; } = inner;
    public PayBandAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandAdjustment? Find(string code)
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

    public void Save(PayBandAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band adjustments. Empty fields match all.</summary>
public sealed record PayBandAdjustmentQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandAdjustment> Apply(List<PayBandAdjustment> items)
    {
        var matched = Collections.Filter(items, (PayBandAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band adjustment store.</summary>
public static class PayBandAdjustmentStore
{
    public static IPayBandAdjustmentStore NewMemoryPayBandAdjustmentStore()
    {
        return new MemoryPayBandAdjustmentStore(new Dictionary<string, PayBandAdjustment>(), new List<string>());
    }

    public static IPayBandAdjustmentStore NewCachedPayBandAdjustmentStore(MemoryPayBandAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandAdjustmentStore(inner, PayBandAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandAdjustment> FindPayBandAdjustments(IPayBandAdjustmentStore store, PayBandAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
