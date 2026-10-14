namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual scenario estimates by code.</summary>
public interface IManualScenarioEstimateStore
{
    ManualScenarioEstimate? Find(string code);
    void Save(ManualScenarioEstimate item);
    bool Remove(string code);
    List<ManualScenarioEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualScenarioEstimateStore(Dictionary<string, ManualScenarioEstimate> items, List<string> order) : IManualScenarioEstimateStore
{
    public Dictionary<string, ManualScenarioEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualScenarioEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualScenarioEstimate item)
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

    public List<ManualScenarioEstimate> List()
    {
        var outValue = new List<ManualScenarioEstimate>();
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
public sealed class CachedManualScenarioEstimateStore(MemoryManualScenarioEstimateStore inner, ManualScenarioEstimateCache cache, int hits, int misses) : IManualScenarioEstimateStore
{
    public MemoryManualScenarioEstimateStore Inner { get; set; } = inner;
    public ManualScenarioEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualScenarioEstimate? Find(string code)
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

    public void Save(ManualScenarioEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualScenarioEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual scenario estimates. Empty fields match all.</summary>
public sealed record ManualScenarioEstimateQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ManualScenarioEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ManualScenarioEstimate> Apply(List<ManualScenarioEstimate> items)
    {
        var matched = Collections.Filter(items, (ManualScenarioEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual scenario estimate store.</summary>
public static class ManualScenarioEstimateStore
{
    public static IManualScenarioEstimateStore NewMemoryManualScenarioEstimateStore()
    {
        return new MemoryManualScenarioEstimateStore(
            new Dictionary<string, ManualScenarioEstimate>(),
            new List<string>());
    }

    public static IManualScenarioEstimateStore NewCachedManualScenarioEstimateStore(MemoryManualScenarioEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedManualScenarioEstimateStore(
            inner,
            ManualScenarioEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualScenarioEstimate> FindManualScenarioEstimates(IManualScenarioEstimateStore store, ManualScenarioEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
