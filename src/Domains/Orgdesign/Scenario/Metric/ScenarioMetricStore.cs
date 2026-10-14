namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario metrics by code.</summary>
public interface IScenarioMetricStore
{
    ScenarioMetric? Find(string code);
    void Save(ScenarioMetric item);
    bool Remove(string code);
    List<ScenarioMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioMetricStore(Dictionary<string, ScenarioMetric> items, List<string> order) : IScenarioMetricStore
{
    public Dictionary<string, ScenarioMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioMetric item)
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

    public List<ScenarioMetric> List()
    {
        var outValue = new List<ScenarioMetric>();
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
public sealed class CachedScenarioMetricStore(MemoryScenarioMetricStore inner, ScenarioMetricCache cache, int hits, int misses) : IScenarioMetricStore
{
    public MemoryScenarioMetricStore Inner { get; set; } = inner;
    public ScenarioMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioMetric? Find(string code)
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

    public void Save(ScenarioMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario metrics. Empty fields match all.</summary>
public sealed record ScenarioMetricQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioMetric item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioMetric> Apply(List<ScenarioMetric> items)
    {
        var matched = Collections.Filter(items, (ScenarioMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario metric store.</summary>
public static class ScenarioMetricStore
{
    public static IScenarioMetricStore NewMemoryScenarioMetricStore()
    {
        return new MemoryScenarioMetricStore(new Dictionary<string, ScenarioMetric>(), new List<string>());
    }

    public static IScenarioMetricStore NewCachedScenarioMetricStore(MemoryScenarioMetricStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioMetricStore(inner, ScenarioMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioMetric> FindScenarioMetrics(IScenarioMetricStore store, ScenarioMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
