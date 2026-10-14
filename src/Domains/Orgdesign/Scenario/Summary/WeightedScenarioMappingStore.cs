namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted scenario mappings by code.</summary>
public interface IWeightedScenarioMappingStore
{
    WeightedScenarioMapping? Find(string code);
    void Save(WeightedScenarioMapping item);
    bool Remove(string code);
    List<WeightedScenarioMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedScenarioMappingStore(Dictionary<string, WeightedScenarioMapping> items, List<string> order) : IWeightedScenarioMappingStore
{
    public Dictionary<string, WeightedScenarioMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedScenarioMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedScenarioMapping item)
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

    public List<WeightedScenarioMapping> List()
    {
        var outValue = new List<WeightedScenarioMapping>();
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
public sealed class CachedWeightedScenarioMappingStore(MemoryWeightedScenarioMappingStore inner, WeightedScenarioMappingCache cache, int hits, int misses) : IWeightedScenarioMappingStore
{
    public MemoryWeightedScenarioMappingStore Inner { get; set; } = inner;
    public WeightedScenarioMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedScenarioMapping? Find(string code)
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

    public void Save(WeightedScenarioMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedScenarioMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted scenario mappings. Empty fields match all.</summary>
public sealed record WeightedScenarioMappingQuery(string Department, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedScenarioMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedScenarioMapping> Apply(List<WeightedScenarioMapping> items)
    {
        var matched = Collections.Filter(items, (WeightedScenarioMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted scenario mapping store.</summary>
public static class WeightedScenarioMappingStore
{
    public static IWeightedScenarioMappingStore NewMemoryWeightedScenarioMappingStore()
    {
        return new MemoryWeightedScenarioMappingStore(
            new Dictionary<string, WeightedScenarioMapping>(),
            new List<string>());
    }

    public static IWeightedScenarioMappingStore NewCachedWeightedScenarioMappingStore(MemoryWeightedScenarioMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedScenarioMappingStore(
            inner,
            WeightedScenarioMappingCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedScenarioMapping> FindWeightedScenarioMappings(IWeightedScenarioMappingStore store, WeightedScenarioMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
