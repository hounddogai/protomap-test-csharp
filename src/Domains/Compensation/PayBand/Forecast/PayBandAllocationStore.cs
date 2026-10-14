namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band allocations by code.</summary>
public interface IPayBandAllocationStore
{
    PayBandAllocation? Find(string code);
    void Save(PayBandAllocation item);
    bool Remove(string code);
    List<PayBandAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandAllocationStore(Dictionary<string, PayBandAllocation> items, List<string> order) : IPayBandAllocationStore
{
    public Dictionary<string, PayBandAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandAllocation item)
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

    public List<PayBandAllocation> List()
    {
        var outValue = new List<PayBandAllocation>();
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
public sealed class CachedPayBandAllocationStore(MemoryPayBandAllocationStore inner, PayBandAllocationCache cache, int hits, int misses) : IPayBandAllocationStore
{
    public MemoryPayBandAllocationStore Inner { get; set; } = inner;
    public PayBandAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandAllocation? Find(string code)
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

    public void Save(PayBandAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band allocations. Empty fields match all.</summary>
public sealed record PayBandAllocationQuery(string Country, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandAllocation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandAllocation> Apply(List<PayBandAllocation> items)
    {
        var matched = Collections.Filter(items, (PayBandAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band allocation store.</summary>
public static class PayBandAllocationStore
{
    public static IPayBandAllocationStore NewMemoryPayBandAllocationStore()
    {
        return new MemoryPayBandAllocationStore(new Dictionary<string, PayBandAllocation>(), new List<string>());
    }

    public static IPayBandAllocationStore NewCachedPayBandAllocationStore(MemoryPayBandAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandAllocationStore(inner, PayBandAllocationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandAllocation> FindPayBandAllocations(IPayBandAllocationStore store, PayBandAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
