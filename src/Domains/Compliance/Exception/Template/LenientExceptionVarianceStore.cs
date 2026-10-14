namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient exception variances by code.</summary>
public interface ILenientExceptionVarianceStore
{
    LenientExceptionVariance? Find(string code);
    void Save(LenientExceptionVariance item);
    bool Remove(string code);
    List<LenientExceptionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientExceptionVarianceStore(Dictionary<string, LenientExceptionVariance> items, List<string> order) : ILenientExceptionVarianceStore
{
    public Dictionary<string, LenientExceptionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientExceptionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientExceptionVariance item)
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

    public List<LenientExceptionVariance> List()
    {
        var outValue = new List<LenientExceptionVariance>();
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
public sealed class CachedLenientExceptionVarianceStore(MemoryLenientExceptionVarianceStore inner, LenientExceptionVarianceCache cache, int hits, int misses) : ILenientExceptionVarianceStore
{
    public MemoryLenientExceptionVarianceStore Inner { get; set; } = inner;
    public LenientExceptionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientExceptionVariance? Find(string code)
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

    public void Save(LenientExceptionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientExceptionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient exception variances. Empty fields match all.</summary>
public sealed record LenientExceptionVarianceQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LenientExceptionVariance item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientExceptionVariance> Apply(List<LenientExceptionVariance> items)
    {
        var matched = Collections.Filter(items, (LenientExceptionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient exception variance store.</summary>
public static class LenientExceptionVarianceStore
{
    public static ILenientExceptionVarianceStore NewMemoryLenientExceptionVarianceStore()
    {
        return new MemoryLenientExceptionVarianceStore(
            new Dictionary<string, LenientExceptionVariance>(),
            new List<string>());
    }

    public static ILenientExceptionVarianceStore NewCachedLenientExceptionVarianceStore(MemoryLenientExceptionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedLenientExceptionVarianceStore(
            inner,
            LenientExceptionVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientExceptionVariance> FindLenientExceptionVariances(ILenientExceptionVarianceStore store, LenientExceptionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
