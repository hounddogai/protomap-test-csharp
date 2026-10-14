namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception summaries by code.</summary>
public interface IExceptionSummaryStore
{
    ExceptionSummary? Find(string code);
    void Save(ExceptionSummary item);
    bool Remove(string code);
    List<ExceptionSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionSummaryStore(Dictionary<string, ExceptionSummary> items, List<string> order) : IExceptionSummaryStore
{
    public Dictionary<string, ExceptionSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionSummary item)
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

    public List<ExceptionSummary> List()
    {
        var outValue = new List<ExceptionSummary>();
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
public sealed class CachedExceptionSummaryStore(MemoryExceptionSummaryStore inner, ExceptionSummaryCache cache, int hits, int misses) : IExceptionSummaryStore
{
    public MemoryExceptionSummaryStore Inner { get; set; } = inner;
    public ExceptionSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionSummary? Find(string code)
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

    public void Save(ExceptionSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception summaries. Empty fields match all.</summary>
public sealed record ExceptionSummaryQuery(string Team, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionSummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ExceptionSummary> Apply(List<ExceptionSummary> items)
    {
        var matched = Collections.Filter(items, (ExceptionSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception summary store.</summary>
public static class ExceptionSummaryStore
{
    public static IExceptionSummaryStore NewMemoryExceptionSummaryStore()
    {
        return new MemoryExceptionSummaryStore(new Dictionary<string, ExceptionSummary>(), new List<string>());
    }

    public static IExceptionSummaryStore NewCachedExceptionSummaryStore(MemoryExceptionSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionSummaryStore(inner, ExceptionSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionSummary> FindExceptionSummaries(IExceptionSummaryStore store, ExceptionSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
