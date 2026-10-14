namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run exceptions by code.</summary>
public interface IPayRunExceptionStore
{
    PayRunException? Find(string code);
    void Save(PayRunException item);
    bool Remove(string code);
    List<PayRunException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunExceptionStore(Dictionary<string, PayRunException> items, List<string> order) : IPayRunExceptionStore
{
    public Dictionary<string, PayRunException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunException item)
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

    public List<PayRunException> List()
    {
        var outValue = new List<PayRunException>();
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
public sealed class CachedPayRunExceptionStore(MemoryPayRunExceptionStore inner, PayRunExceptionCache cache, int hits, int misses) : IPayRunExceptionStore
{
    public MemoryPayRunExceptionStore Inner { get; set; } = inner;
    public PayRunExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunException? Find(string code)
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

    public void Save(PayRunException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run exceptions. Empty fields match all.</summary>
public sealed record PayRunExceptionQuery(string Category, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunException item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunException> Apply(List<PayRunException> items)
    {
        var matched = Collections.Filter(items, (PayRunException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run exception store.</summary>
public static class PayRunExceptionStore
{
    public static IPayRunExceptionStore NewMemoryPayRunExceptionStore()
    {
        return new MemoryPayRunExceptionStore(new Dictionary<string, PayRunException>(), new List<string>());
    }

    public static IPayRunExceptionStore NewCachedPayRunExceptionStore(MemoryPayRunExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunExceptionStore(inner, PayRunExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunException> FindPayRunExceptions(IPayRunExceptionStore store, PayRunExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
