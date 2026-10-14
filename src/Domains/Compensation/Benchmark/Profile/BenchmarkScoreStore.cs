namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark scores by code.</summary>
public interface IBenchmarkScoreStore
{
    BenchmarkScore? Find(string code);
    void Save(BenchmarkScore item);
    bool Remove(string code);
    List<BenchmarkScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkScoreStore(Dictionary<string, BenchmarkScore> items, List<string> order) : IBenchmarkScoreStore
{
    public Dictionary<string, BenchmarkScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkScore item)
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

    public List<BenchmarkScore> List()
    {
        var outValue = new List<BenchmarkScore>();
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
public sealed class CachedBenchmarkScoreStore(MemoryBenchmarkScoreStore inner, BenchmarkScoreCache cache, int hits, int misses) : IBenchmarkScoreStore
{
    public MemoryBenchmarkScoreStore Inner { get; set; } = inner;
    public BenchmarkScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkScore? Find(string code)
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

    public void Save(BenchmarkScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark scores. Empty fields match all.</summary>
public sealed record BenchmarkScoreQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkScore item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkScore> Apply(List<BenchmarkScore> items)
    {
        var matched = Collections.Filter(items, (BenchmarkScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark score store.</summary>
public static class BenchmarkScoreStore
{
    public static IBenchmarkScoreStore NewMemoryBenchmarkScoreStore()
    {
        return new MemoryBenchmarkScoreStore(new Dictionary<string, BenchmarkScore>(), new List<string>());
    }

    public static IBenchmarkScoreStore NewCachedBenchmarkScoreStore(MemoryBenchmarkScoreStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkScoreStore(inner, BenchmarkScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkScore> FindBenchmarkScores(IBenchmarkScoreStore store, BenchmarkScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
