namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model summaries by code.</summary>
public interface ICostModelSummaryStore
{
    CostModelSummary? Find(string code);
    void Save(CostModelSummary item);
    bool Remove(string code);
    List<CostModelSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelSummaryStore(Dictionary<string, CostModelSummary> items, List<string> order) : ICostModelSummaryStore
{
    public Dictionary<string, CostModelSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelSummary item)
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

    public List<CostModelSummary> List()
    {
        var outValue = new List<CostModelSummary>();
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
public sealed class CachedCostModelSummaryStore(MemoryCostModelSummaryStore inner, CostModelSummaryCache cache, int hits, int misses) : ICostModelSummaryStore
{
    public MemoryCostModelSummaryStore Inner { get; set; } = inner;
    public CostModelSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelSummary? Find(string code)
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

    public void Save(CostModelSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model summaries. Empty fields match all.</summary>
public sealed record CostModelSummaryQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelSummary item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelSummary> Apply(List<CostModelSummary> items)
    {
        var matched = Collections.Filter(items, (CostModelSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model summary store.</summary>
public static class CostModelSummaryStore
{
    public static ICostModelSummaryStore NewMemoryCostModelSummaryStore()
    {
        return new MemoryCostModelSummaryStore(new Dictionary<string, CostModelSummary>(), new List<string>());
    }

    public static ICostModelSummaryStore NewCachedCostModelSummaryStore(MemoryCostModelSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelSummaryStore(inner, CostModelSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelSummary> FindCostModelSummaries(ICostModelSummaryStore store, CostModelSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
