namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario summaries by code.</summary>
public interface IScenarioSummaryStore
{
    ScenarioSummary? Find(string code);
    void Save(ScenarioSummary item);
    bool Remove(string code);
    List<ScenarioSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioSummaryStore(Dictionary<string, ScenarioSummary> items, List<string> order) : IScenarioSummaryStore
{
    public Dictionary<string, ScenarioSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioSummary item)
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

    public List<ScenarioSummary> List()
    {
        var outValue = new List<ScenarioSummary>();
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
public sealed class CachedScenarioSummaryStore(MemoryScenarioSummaryStore inner, ScenarioSummaryCache cache, int hits, int misses) : IScenarioSummaryStore
{
    public MemoryScenarioSummaryStore Inner { get; set; } = inner;
    public ScenarioSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioSummary? Find(string code)
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

    public void Save(ScenarioSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario summaries. Empty fields match all.</summary>
public sealed record ScenarioSummaryQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioSummary item)
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

    public List<ScenarioSummary> Apply(List<ScenarioSummary> items)
    {
        var matched = Collections.Filter(items, (ScenarioSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario summary store.</summary>
public static class ScenarioSummaryStore
{
    public static IScenarioSummaryStore NewMemoryScenarioSummaryStore()
    {
        return new MemoryScenarioSummaryStore(new Dictionary<string, ScenarioSummary>(), new List<string>());
    }

    public static IScenarioSummaryStore NewCachedScenarioSummaryStore(MemoryScenarioSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioSummaryStore(inner, ScenarioSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScenarioSummary> FindScenarioSummaries(IScenarioSummaryStore store, ScenarioSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
