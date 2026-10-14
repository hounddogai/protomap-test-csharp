namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global benchmark policies by code.</summary>
public interface IGlobalBenchmarkPolicyStore
{
    GlobalBenchmarkPolicy? Find(string code);
    void Save(GlobalBenchmarkPolicy item);
    bool Remove(string code);
    List<GlobalBenchmarkPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalBenchmarkPolicyStore(Dictionary<string, GlobalBenchmarkPolicy> items, List<string> order) : IGlobalBenchmarkPolicyStore
{
    public Dictionary<string, GlobalBenchmarkPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalBenchmarkPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalBenchmarkPolicy item)
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

    public List<GlobalBenchmarkPolicy> List()
    {
        var outValue = new List<GlobalBenchmarkPolicy>();
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
public sealed class CachedGlobalBenchmarkPolicyStore(MemoryGlobalBenchmarkPolicyStore inner, GlobalBenchmarkPolicyCache cache, int hits, int misses) : IGlobalBenchmarkPolicyStore
{
    public MemoryGlobalBenchmarkPolicyStore Inner { get; set; } = inner;
    public GlobalBenchmarkPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalBenchmarkPolicy? Find(string code)
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

    public void Save(GlobalBenchmarkPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalBenchmarkPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global benchmark policies. Empty fields match all.</summary>
public sealed record GlobalBenchmarkPolicyQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalBenchmarkPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalBenchmarkPolicy> Apply(List<GlobalBenchmarkPolicy> items)
    {
        var matched = Collections.Filter(items, (GlobalBenchmarkPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global benchmark policy store.</summary>
public static class GlobalBenchmarkPolicyStore
{
    public static IGlobalBenchmarkPolicyStore NewMemoryGlobalBenchmarkPolicyStore()
    {
        return new MemoryGlobalBenchmarkPolicyStore(
            new Dictionary<string, GlobalBenchmarkPolicy>(),
            new List<string>());
    }

    public static IGlobalBenchmarkPolicyStore NewCachedGlobalBenchmarkPolicyStore(MemoryGlobalBenchmarkPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalBenchmarkPolicyStore(inner, GlobalBenchmarkPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GlobalBenchmarkPolicy> FindGlobalBenchmarkPolicies(IGlobalBenchmarkPolicyStore store, GlobalBenchmarkPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
