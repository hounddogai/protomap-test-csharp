namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan summaries by code.</summary>
public interface ISuccessionPlanSummaryStore
{
    SuccessionPlanSummary? Find(string code);
    void Save(SuccessionPlanSummary item);
    bool Remove(string code);
    List<SuccessionPlanSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanSummaryStore(Dictionary<string, SuccessionPlanSummary> items, List<string> order) : ISuccessionPlanSummaryStore
{
    public Dictionary<string, SuccessionPlanSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanSummary item)
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

    public List<SuccessionPlanSummary> List()
    {
        var outValue = new List<SuccessionPlanSummary>();
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
public sealed class CachedSuccessionPlanSummaryStore(MemorySuccessionPlanSummaryStore inner, SuccessionPlanSummaryCache cache, int hits, int misses) : ISuccessionPlanSummaryStore
{
    public MemorySuccessionPlanSummaryStore Inner { get; set; } = inner;
    public SuccessionPlanSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanSummary? Find(string code)
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

    public void Save(SuccessionPlanSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan summaries. Empty fields match all.</summary>
public sealed record SuccessionPlanSummaryQuery(string Category, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanSummary> Apply(List<SuccessionPlanSummary> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan summary store.</summary>
public static class SuccessionPlanSummaryStore
{
    public static ISuccessionPlanSummaryStore NewMemorySuccessionPlanSummaryStore()
    {
        return new MemorySuccessionPlanSummaryStore(
            new Dictionary<string, SuccessionPlanSummary>(),
            new List<string>());
    }

    public static ISuccessionPlanSummaryStore NewCachedSuccessionPlanSummaryStore(MemorySuccessionPlanSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanSummaryStore(inner, SuccessionPlanSummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanSummary> FindSuccessionPlanSummaries(ISuccessionPlanSummaryStore store, SuccessionPlanSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
