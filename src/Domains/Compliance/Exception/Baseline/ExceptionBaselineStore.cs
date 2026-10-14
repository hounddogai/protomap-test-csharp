namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception baselines by code.</summary>
public interface IExceptionBaselineStore
{
    ExceptionBaseline? Find(string code);
    void Save(ExceptionBaseline item);
    bool Remove(string code);
    List<ExceptionBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionBaselineStore(Dictionary<string, ExceptionBaseline> items, List<string> order) : IExceptionBaselineStore
{
    public Dictionary<string, ExceptionBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionBaseline item)
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

    public List<ExceptionBaseline> List()
    {
        var outValue = new List<ExceptionBaseline>();
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
public sealed class CachedExceptionBaselineStore(MemoryExceptionBaselineStore inner, ExceptionBaselineCache cache, int hits, int misses) : IExceptionBaselineStore
{
    public MemoryExceptionBaselineStore Inner { get; set; } = inner;
    public ExceptionBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionBaseline? Find(string code)
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

    public void Save(ExceptionBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception baselines. Empty fields match all.</summary>
public sealed record ExceptionBaselineQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionBaseline> Apply(List<ExceptionBaseline> items)
    {
        var matched = Collections.Filter(items, (ExceptionBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception baseline store.</summary>
public static class ExceptionBaselineStore
{
    public static IExceptionBaselineStore NewMemoryExceptionBaselineStore()
    {
        return new MemoryExceptionBaselineStore(new Dictionary<string, ExceptionBaseline>(), new List<string>());
    }

    public static IExceptionBaselineStore NewCachedExceptionBaselineStore(MemoryExceptionBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionBaselineStore(inner, ExceptionBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionBaseline> FindExceptionBaselines(IExceptionBaselineStore store, ExceptionBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
