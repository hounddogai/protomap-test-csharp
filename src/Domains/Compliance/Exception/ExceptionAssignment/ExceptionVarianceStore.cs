namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception variances by code.</summary>
public interface IExceptionVarianceStore
{
    ExceptionVariance? Find(string code);
    void Save(ExceptionVariance item);
    bool Remove(string code);
    List<ExceptionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionVarianceStore(Dictionary<string, ExceptionVariance> items, List<string> order) : IExceptionVarianceStore
{
    public Dictionary<string, ExceptionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionVariance item)
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

    public List<ExceptionVariance> List()
    {
        var outValue = new List<ExceptionVariance>();
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
public sealed class CachedExceptionVarianceStore(MemoryExceptionVarianceStore inner, ExceptionVarianceCache cache, int hits, int misses) : IExceptionVarianceStore
{
    public MemoryExceptionVarianceStore Inner { get; set; } = inner;
    public ExceptionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionVariance? Find(string code)
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

    public void Save(ExceptionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception variances. Empty fields match all.</summary>
public sealed record ExceptionVarianceQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ExceptionVariance> Apply(List<ExceptionVariance> items)
    {
        var matched = Collections.Filter(items, (ExceptionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception variance store.</summary>
public static class ExceptionVarianceStore
{
    public static IExceptionVarianceStore NewMemoryExceptionVarianceStore()
    {
        return new MemoryExceptionVarianceStore(new Dictionary<string, ExceptionVariance>(), new List<string>());
    }

    public static IExceptionVarianceStore NewCachedExceptionVarianceStore(MemoryExceptionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionVarianceStore(inner, ExceptionVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionVariance> FindExceptionVariances(IExceptionVarianceStore store, ExceptionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
