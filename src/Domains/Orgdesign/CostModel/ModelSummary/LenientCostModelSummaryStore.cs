namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient cost model summaries by code.</summary>
public interface ILenientCostModelSummaryStore
{
    LenientCostModelSummary? Find(string code);
    void Save(LenientCostModelSummary item);
    bool Remove(string code);
    List<LenientCostModelSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientCostModelSummaryStore(Dictionary<string, LenientCostModelSummary> items, List<string> order) : ILenientCostModelSummaryStore
{
    public Dictionary<string, LenientCostModelSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientCostModelSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientCostModelSummary item)
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

    public List<LenientCostModelSummary> List()
    {
        var outValue = new List<LenientCostModelSummary>();
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
public sealed class CachedLenientCostModelSummaryStore(MemoryLenientCostModelSummaryStore inner, LenientCostModelSummaryCache cache, int hits, int misses) : ILenientCostModelSummaryStore
{
    public MemoryLenientCostModelSummaryStore Inner { get; set; } = inner;
    public LenientCostModelSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientCostModelSummary? Find(string code)
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

    public void Save(LenientCostModelSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientCostModelSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient cost model summaries. Empty fields match all.</summary>
public sealed record LenientCostModelSummaryQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LenientCostModelSummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LenientCostModelSummary> Apply(List<LenientCostModelSummary> items)
    {
        var matched = Collections.Filter(items, (LenientCostModelSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient cost model summary store.</summary>
public static class LenientCostModelSummaryStore
{
    public static ILenientCostModelSummaryStore NewMemoryLenientCostModelSummaryStore()
    {
        return new MemoryLenientCostModelSummaryStore(
            new Dictionary<string, LenientCostModelSummary>(),
            new List<string>());
    }

    public static ILenientCostModelSummaryStore NewCachedLenientCostModelSummaryStore(MemoryLenientCostModelSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientCostModelSummaryStore(
            inner,
            LenientCostModelSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientCostModelSummary> FindLenientCostModelSummaries(ILenientCostModelSummaryStore store, LenientCostModelSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
