namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback benchmark rules by code.</summary>
public interface IFallbackBenchmarkRuleStore
{
    FallbackBenchmarkRule? Find(string code);
    void Save(FallbackBenchmarkRule item);
    bool Remove(string code);
    List<FallbackBenchmarkRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBenchmarkRuleStore(Dictionary<string, FallbackBenchmarkRule> items, List<string> order) : IFallbackBenchmarkRuleStore
{
    public Dictionary<string, FallbackBenchmarkRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBenchmarkRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBenchmarkRule item)
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

    public List<FallbackBenchmarkRule> List()
    {
        var outValue = new List<FallbackBenchmarkRule>();
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
public sealed class CachedFallbackBenchmarkRuleStore(MemoryFallbackBenchmarkRuleStore inner, FallbackBenchmarkRuleCache cache, int hits, int misses) : IFallbackBenchmarkRuleStore
{
    public MemoryFallbackBenchmarkRuleStore Inner { get; set; } = inner;
    public FallbackBenchmarkRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBenchmarkRule? Find(string code)
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

    public void Save(FallbackBenchmarkRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBenchmarkRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback benchmark rules. Empty fields match all.</summary>
public sealed record FallbackBenchmarkRuleQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBenchmarkRule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackBenchmarkRule> Apply(List<FallbackBenchmarkRule> items)
    {
        var matched = Collections.Filter(items, (FallbackBenchmarkRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback benchmark rule store.</summary>
public static class FallbackBenchmarkRuleStore
{
    public static IFallbackBenchmarkRuleStore NewMemoryFallbackBenchmarkRuleStore()
    {
        return new MemoryFallbackBenchmarkRuleStore(
            new Dictionary<string, FallbackBenchmarkRule>(),
            new List<string>());
    }

    public static IFallbackBenchmarkRuleStore NewCachedFallbackBenchmarkRuleStore(MemoryFallbackBenchmarkRuleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBenchmarkRuleStore(inner, FallbackBenchmarkRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackBenchmarkRule> FindFallbackBenchmarkRules(IFallbackBenchmarkRuleStore store, FallbackBenchmarkRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
