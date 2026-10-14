namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay summaries by code.</summary>
public interface IRetroPaySummaryStore
{
    RetroPaySummary? Find(string code);
    void Save(RetroPaySummary item);
    bool Remove(string code);
    List<RetroPaySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPaySummaryStore(Dictionary<string, RetroPaySummary> items, List<string> order) : IRetroPaySummaryStore
{
    public Dictionary<string, RetroPaySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPaySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPaySummary item)
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

    public List<RetroPaySummary> List()
    {
        var outValue = new List<RetroPaySummary>();
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
public sealed class CachedRetroPaySummaryStore(MemoryRetroPaySummaryStore inner, RetroPaySummaryCache cache, int hits, int misses) : IRetroPaySummaryStore
{
    public MemoryRetroPaySummaryStore Inner { get; set; } = inner;
    public RetroPaySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPaySummary? Find(string code)
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

    public void Save(RetroPaySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPaySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay summaries. Empty fields match all.</summary>
public sealed record RetroPaySummaryQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPaySummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPaySummary> Apply(List<RetroPaySummary> items)
    {
        var matched = Collections.Filter(items, (RetroPaySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay summary store.</summary>
public static class RetroPaySummaryStore
{
    public static IRetroPaySummaryStore NewMemoryRetroPaySummaryStore()
    {
        return new MemoryRetroPaySummaryStore(new Dictionary<string, RetroPaySummary>(), new List<string>());
    }

    public static IRetroPaySummaryStore NewCachedRetroPaySummaryStore(MemoryRetroPaySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPaySummaryStore(inner, RetroPaySummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPaySummary> FindRetroPaySummaries(IRetroPaySummaryStore store, RetroPaySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
