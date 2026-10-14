namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark preferences by code.</summary>
public interface IBenchmarkPreferenceStore
{
    BenchmarkPreference? Find(string code);
    void Save(BenchmarkPreference item);
    bool Remove(string code);
    List<BenchmarkPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkPreferenceStore(Dictionary<string, BenchmarkPreference> items, List<string> order) : IBenchmarkPreferenceStore
{
    public Dictionary<string, BenchmarkPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkPreference item)
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

    public List<BenchmarkPreference> List()
    {
        var outValue = new List<BenchmarkPreference>();
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
public sealed class CachedBenchmarkPreferenceStore(MemoryBenchmarkPreferenceStore inner, BenchmarkPreferenceCache cache, int hits, int misses) : IBenchmarkPreferenceStore
{
    public MemoryBenchmarkPreferenceStore Inner { get; set; } = inner;
    public BenchmarkPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkPreference? Find(string code)
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

    public void Save(BenchmarkPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark preferences. Empty fields match all.</summary>
public sealed record BenchmarkPreferenceQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkPreference> Apply(List<BenchmarkPreference> items)
    {
        var matched = Collections.Filter(items, (BenchmarkPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark preference store.</summary>
public static class BenchmarkPreferenceStore
{
    public static IBenchmarkPreferenceStore NewMemoryBenchmarkPreferenceStore()
    {
        return new MemoryBenchmarkPreferenceStore(new Dictionary<string, BenchmarkPreference>(), new List<string>());
    }

    public static IBenchmarkPreferenceStore NewCachedBenchmarkPreferenceStore(MemoryBenchmarkPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkPreferenceStore(inner, BenchmarkPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BenchmarkPreference> FindBenchmarkPreferences(IBenchmarkPreferenceStore store, BenchmarkPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
