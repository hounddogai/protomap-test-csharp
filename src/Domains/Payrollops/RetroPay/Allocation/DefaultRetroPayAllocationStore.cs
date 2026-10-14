namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default retro pay allocations by code.</summary>
public interface IDefaultRetroPayAllocationStore
{
    DefaultRetroPayAllocation? Find(string code);
    void Save(DefaultRetroPayAllocation item);
    bool Remove(string code);
    List<DefaultRetroPayAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultRetroPayAllocationStore(Dictionary<string, DefaultRetroPayAllocation> items, List<string> order) : IDefaultRetroPayAllocationStore
{
    public Dictionary<string, DefaultRetroPayAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultRetroPayAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultRetroPayAllocation item)
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

    public List<DefaultRetroPayAllocation> List()
    {
        var outValue = new List<DefaultRetroPayAllocation>();
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
public sealed class CachedDefaultRetroPayAllocationStore(MemoryDefaultRetroPayAllocationStore inner, DefaultRetroPayAllocationCache cache, int hits, int misses) : IDefaultRetroPayAllocationStore
{
    public MemoryDefaultRetroPayAllocationStore Inner { get; set; } = inner;
    public DefaultRetroPayAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultRetroPayAllocation? Find(string code)
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

    public void Save(DefaultRetroPayAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultRetroPayAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default retro pay allocations. Empty fields match all.</summary>
public sealed record DefaultRetroPayAllocationQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultRetroPayAllocation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<DefaultRetroPayAllocation> Apply(List<DefaultRetroPayAllocation> items)
    {
        var matched = Collections.Filter(items, (DefaultRetroPayAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default retro pay allocation store.</summary>
public static class DefaultRetroPayAllocationStore
{
    public static IDefaultRetroPayAllocationStore NewMemoryDefaultRetroPayAllocationStore()
    {
        return new MemoryDefaultRetroPayAllocationStore(
            new Dictionary<string, DefaultRetroPayAllocation>(),
            new List<string>());
    }

    public static IDefaultRetroPayAllocationStore NewCachedDefaultRetroPayAllocationStore(MemoryDefaultRetroPayAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultRetroPayAllocationStore(
            inner,
            DefaultRetroPayAllocationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultRetroPayAllocation> FindDefaultRetroPayAllocations(IDefaultRetroPayAllocationStore store, DefaultRetroPayAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
