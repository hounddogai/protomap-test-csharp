namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception bundles by code.</summary>
public interface IExceptionBundleStore
{
    ExceptionBundle? Find(string code);
    void Save(ExceptionBundle item);
    bool Remove(string code);
    List<ExceptionBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionBundleStore(Dictionary<string, ExceptionBundle> items, List<string> order) : IExceptionBundleStore
{
    public Dictionary<string, ExceptionBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionBundle item)
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

    public List<ExceptionBundle> List()
    {
        var outValue = new List<ExceptionBundle>();
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
public sealed class CachedExceptionBundleStore(MemoryExceptionBundleStore inner, ExceptionBundleCache cache, int hits, int misses) : IExceptionBundleStore
{
    public MemoryExceptionBundleStore Inner { get; set; } = inner;
    public ExceptionBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionBundle? Find(string code)
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

    public void Save(ExceptionBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception bundles. Empty fields match all.</summary>
public sealed record ExceptionBundleQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionBundle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ExceptionBundle> Apply(List<ExceptionBundle> items)
    {
        var matched = Collections.Filter(items, (ExceptionBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception bundle store.</summary>
public static class ExceptionBundleStore
{
    public static IExceptionBundleStore NewMemoryExceptionBundleStore()
    {
        return new MemoryExceptionBundleStore(new Dictionary<string, ExceptionBundle>(), new List<string>());
    }

    public static IExceptionBundleStore NewCachedExceptionBundleStore(MemoryExceptionBundleStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionBundleStore(inner, ExceptionBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ExceptionBundle> FindExceptionBundles(IExceptionBundleStore store, ExceptionBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
