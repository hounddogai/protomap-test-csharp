namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pay band thresholds by code.</summary>
public interface ICappedPayBandThresholdStore
{
    CappedPayBandThreshold? Find(string code);
    void Save(CappedPayBandThreshold item);
    bool Remove(string code);
    List<CappedPayBandThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayBandThresholdStore(Dictionary<string, CappedPayBandThreshold> items, List<string> order) : ICappedPayBandThresholdStore
{
    public Dictionary<string, CappedPayBandThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayBandThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayBandThreshold item)
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

    public List<CappedPayBandThreshold> List()
    {
        var outValue = new List<CappedPayBandThreshold>();
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
public sealed class CachedCappedPayBandThresholdStore(MemoryCappedPayBandThresholdStore inner, CappedPayBandThresholdCache cache, int hits, int misses) : ICappedPayBandThresholdStore
{
    public MemoryCappedPayBandThresholdStore Inner { get; set; } = inner;
    public CappedPayBandThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayBandThreshold? Find(string code)
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

    public void Save(CappedPayBandThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayBandThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pay band thresholds. Empty fields match all.</summary>
public sealed record CappedPayBandThresholdQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayBandThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayBandThreshold> Apply(List<CappedPayBandThreshold> items)
    {
        var matched = Collections.Filter(items, (CappedPayBandThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pay band threshold store.</summary>
public static class CappedPayBandThresholdStore
{
    public static ICappedPayBandThresholdStore NewMemoryCappedPayBandThresholdStore()
    {
        return new MemoryCappedPayBandThresholdStore(
            new Dictionary<string, CappedPayBandThreshold>(),
            new List<string>());
    }

    public static ICappedPayBandThresholdStore NewCachedCappedPayBandThresholdStore(MemoryCappedPayBandThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayBandThresholdStore(
            inner,
            CappedPayBandThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedPayBandThreshold> FindCappedPayBandThresholds(ICappedPayBandThresholdStore store, CappedPayBandThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
