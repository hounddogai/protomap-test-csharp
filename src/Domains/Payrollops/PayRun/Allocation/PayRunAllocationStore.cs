namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run allocations by code.</summary>
public interface IPayRunAllocationStore
{
    PayRunAllocation? Find(string code);
    void Save(PayRunAllocation item);
    bool Remove(string code);
    List<PayRunAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunAllocationStore(Dictionary<string, PayRunAllocation> items, List<string> order) : IPayRunAllocationStore
{
    public Dictionary<string, PayRunAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunAllocation item)
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

    public List<PayRunAllocation> List()
    {
        var outValue = new List<PayRunAllocation>();
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
public sealed class CachedPayRunAllocationStore(MemoryPayRunAllocationStore inner, PayRunAllocationCache cache, int hits, int misses) : IPayRunAllocationStore
{
    public MemoryPayRunAllocationStore Inner { get; set; } = inner;
    public PayRunAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunAllocation? Find(string code)
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

    public void Save(PayRunAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run allocations. Empty fields match all.</summary>
public sealed record PayRunAllocationQuery(string Unit, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunAllocation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<PayRunAllocation> Apply(List<PayRunAllocation> items)
    {
        var matched = Collections.Filter(items, (PayRunAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run allocation store.</summary>
public static class PayRunAllocationStore
{
    public static IPayRunAllocationStore NewMemoryPayRunAllocationStore()
    {
        return new MemoryPayRunAllocationStore(new Dictionary<string, PayRunAllocation>(), new List<string>());
    }

    public static IPayRunAllocationStore NewCachedPayRunAllocationStore(MemoryPayRunAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunAllocationStore(inner, PayRunAllocationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunAllocation> FindPayRunAllocations(IPayRunAllocationStore store, PayRunAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
