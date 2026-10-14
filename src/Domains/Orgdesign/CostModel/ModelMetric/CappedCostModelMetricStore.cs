namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped cost model metrics by code.</summary>
public interface ICappedCostModelMetricStore
{
    CappedCostModelMetric? Find(string code);
    void Save(CappedCostModelMetric item);
    bool Remove(string code);
    List<CappedCostModelMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCostModelMetricStore(Dictionary<string, CappedCostModelMetric> items, List<string> order) : ICappedCostModelMetricStore
{
    public Dictionary<string, CappedCostModelMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCostModelMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCostModelMetric item)
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

    public List<CappedCostModelMetric> List()
    {
        var outValue = new List<CappedCostModelMetric>();
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
public sealed class CachedCappedCostModelMetricStore(MemoryCappedCostModelMetricStore inner, CappedCostModelMetricCache cache, int hits, int misses) : ICappedCostModelMetricStore
{
    public MemoryCappedCostModelMetricStore Inner { get; set; } = inner;
    public CappedCostModelMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCostModelMetric? Find(string code)
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

    public void Save(CappedCostModelMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCostModelMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped cost model metrics. Empty fields match all.</summary>
public sealed record CappedCostModelMetricQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCostModelMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedCostModelMetric> Apply(List<CappedCostModelMetric> items)
    {
        var matched = Collections.Filter(items, (CappedCostModelMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped cost model metric store.</summary>
public static class CappedCostModelMetricStore
{
    public static ICappedCostModelMetricStore NewMemoryCappedCostModelMetricStore()
    {
        return new MemoryCappedCostModelMetricStore(
            new Dictionary<string, CappedCostModelMetric>(),
            new List<string>());
    }

    public static ICappedCostModelMetricStore NewCachedCappedCostModelMetricStore(MemoryCappedCostModelMetricStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCostModelMetricStore(inner, CappedCostModelMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedCostModelMetric> FindCappedCostModelMetrics(ICappedCostModelMetricStore store, CappedCostModelMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
