namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run windows by code.</summary>
public interface IPayRunWindowStore
{
    PayRunWindow? Find(string code);
    void Save(PayRunWindow item);
    bool Remove(string code);
    List<PayRunWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunWindowStore(Dictionary<string, PayRunWindow> items, List<string> order) : IPayRunWindowStore
{
    public Dictionary<string, PayRunWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunWindow item)
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

    public List<PayRunWindow> List()
    {
        var outValue = new List<PayRunWindow>();
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
public sealed class CachedPayRunWindowStore(MemoryPayRunWindowStore inner, PayRunWindowCache cache, int hits, int misses) : IPayRunWindowStore
{
    public MemoryPayRunWindowStore Inner { get; set; } = inner;
    public PayRunWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunWindow? Find(string code)
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

    public void Save(PayRunWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run windows. Empty fields match all.</summary>
public sealed record PayRunWindowQuery(string Category, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunWindow item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunWindow> Apply(List<PayRunWindow> items)
    {
        var matched = Collections.Filter(items, (PayRunWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run window store.</summary>
public static class PayRunWindowStore
{
    public static IPayRunWindowStore NewMemoryPayRunWindowStore()
    {
        return new MemoryPayRunWindowStore(new Dictionary<string, PayRunWindow>(), new List<string>());
    }

    public static IPayRunWindowStore NewCachedPayRunWindowStore(MemoryPayRunWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunWindowStore(inner, PayRunWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunWindow> FindPayRunWindows(IPayRunWindowStore store, PayRunWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
