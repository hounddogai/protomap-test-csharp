namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient benchmark manifests by code.</summary>
public interface ILenientBenchmarkManifestStore
{
    LenientBenchmarkManifest? Find(string code);
    void Save(LenientBenchmarkManifest item);
    bool Remove(string code);
    List<LenientBenchmarkManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBenchmarkManifestStore(Dictionary<string, LenientBenchmarkManifest> items, List<string> order) : ILenientBenchmarkManifestStore
{
    public Dictionary<string, LenientBenchmarkManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBenchmarkManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBenchmarkManifest item)
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

    public List<LenientBenchmarkManifest> List()
    {
        var outValue = new List<LenientBenchmarkManifest>();
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
public sealed class CachedLenientBenchmarkManifestStore(MemoryLenientBenchmarkManifestStore inner, LenientBenchmarkManifestCache cache, int hits, int misses) : ILenientBenchmarkManifestStore
{
    public MemoryLenientBenchmarkManifestStore Inner { get; set; } = inner;
    public LenientBenchmarkManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBenchmarkManifest? Find(string code)
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

    public void Save(LenientBenchmarkManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBenchmarkManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient benchmark manifests. Empty fields match all.</summary>
public sealed record LenientBenchmarkManifestQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBenchmarkManifest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBenchmarkManifest> Apply(List<LenientBenchmarkManifest> items)
    {
        var matched = Collections.Filter(items, (LenientBenchmarkManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient benchmark manifest store.</summary>
public static class LenientBenchmarkManifestStore
{
    public static ILenientBenchmarkManifestStore NewMemoryLenientBenchmarkManifestStore()
    {
        return new MemoryLenientBenchmarkManifestStore(
            new Dictionary<string, LenientBenchmarkManifest>(),
            new List<string>());
    }

    public static ILenientBenchmarkManifestStore NewCachedLenientBenchmarkManifestStore(MemoryLenientBenchmarkManifestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBenchmarkManifestStore(
            inner,
            LenientBenchmarkManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientBenchmarkManifest> FindLenientBenchmarkManifests(ILenientBenchmarkManifestStore store, LenientBenchmarkManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
