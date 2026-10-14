namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional cost model summaries by code.</summary>
public interface IRegionalCostModelSummaryStore
{
    RegionalCostModelSummary? Find(string code);
    void Save(RegionalCostModelSummary item);
    bool Remove(string code);
    List<RegionalCostModelSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalCostModelSummaryStore(Dictionary<string, RegionalCostModelSummary> items, List<string> order) : IRegionalCostModelSummaryStore
{
    public Dictionary<string, RegionalCostModelSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalCostModelSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalCostModelSummary item)
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

    public List<RegionalCostModelSummary> List()
    {
        var outValue = new List<RegionalCostModelSummary>();
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
public sealed class CachedRegionalCostModelSummaryStore(MemoryRegionalCostModelSummaryStore inner, RegionalCostModelSummaryCache cache, int hits, int misses) : IRegionalCostModelSummaryStore
{
    public MemoryRegionalCostModelSummaryStore Inner { get; set; } = inner;
    public RegionalCostModelSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalCostModelSummary? Find(string code)
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

    public void Save(RegionalCostModelSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalCostModelSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional cost model summaries. Empty fields match all.</summary>
public sealed record RegionalCostModelSummaryQuery(string Category, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalCostModelSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalCostModelSummary> Apply(List<RegionalCostModelSummary> items)
    {
        var matched = Collections.Filter(items, (RegionalCostModelSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional cost model summary store.</summary>
public static class RegionalCostModelSummaryStore
{
    public static IRegionalCostModelSummaryStore NewMemoryRegionalCostModelSummaryStore()
    {
        return new MemoryRegionalCostModelSummaryStore(
            new Dictionary<string, RegionalCostModelSummary>(),
            new List<string>());
    }

    public static IRegionalCostModelSummaryStore NewCachedRegionalCostModelSummaryStore(MemoryRegionalCostModelSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalCostModelSummaryStore(
            inner,
            RegionalCostModelSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalCostModelSummary> FindRegionalCostModelSummaries(IRegionalCostModelSummaryStore store, RegionalCostModelSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
