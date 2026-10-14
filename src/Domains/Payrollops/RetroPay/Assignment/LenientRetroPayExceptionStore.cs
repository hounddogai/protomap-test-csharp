namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient retro pay exceptions by code.</summary>
public interface ILenientRetroPayExceptionStore
{
    LenientRetroPayException? Find(string code);
    void Save(LenientRetroPayException item);
    bool Remove(string code);
    List<LenientRetroPayException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRetroPayExceptionStore(Dictionary<string, LenientRetroPayException> items, List<string> order) : ILenientRetroPayExceptionStore
{
    public Dictionary<string, LenientRetroPayException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRetroPayException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRetroPayException item)
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

    public List<LenientRetroPayException> List()
    {
        var outValue = new List<LenientRetroPayException>();
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
public sealed class CachedLenientRetroPayExceptionStore(MemoryLenientRetroPayExceptionStore inner, LenientRetroPayExceptionCache cache, int hits, int misses) : ILenientRetroPayExceptionStore
{
    public MemoryLenientRetroPayExceptionStore Inner { get; set; } = inner;
    public LenientRetroPayExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRetroPayException? Find(string code)
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

    public void Save(LenientRetroPayException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRetroPayException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient retro pay exceptions. Empty fields match all.</summary>
public sealed record LenientRetroPayExceptionQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRetroPayException item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientRetroPayException> Apply(List<LenientRetroPayException> items)
    {
        var matched = Collections.Filter(items, (LenientRetroPayException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient retro pay exception store.</summary>
public static class LenientRetroPayExceptionStore
{
    public static ILenientRetroPayExceptionStore NewMemoryLenientRetroPayExceptionStore()
    {
        return new MemoryLenientRetroPayExceptionStore(
            new Dictionary<string, LenientRetroPayException>(),
            new List<string>());
    }

    public static ILenientRetroPayExceptionStore NewCachedLenientRetroPayExceptionStore(MemoryLenientRetroPayExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRetroPayExceptionStore(
            inner,
            LenientRetroPayExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientRetroPayException> FindLenientRetroPayExceptions(ILenientRetroPayExceptionStore store, LenientRetroPayExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
