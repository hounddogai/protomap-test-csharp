namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered benchmark scores by code.</summary>
public interface ITieredBenchmarkScoreStore
{
    TieredBenchmarkScore? Find(string code);
    void Save(TieredBenchmarkScore item);
    bool Remove(string code);
    List<TieredBenchmarkScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBenchmarkScoreStore(Dictionary<string, TieredBenchmarkScore> items, List<string> order) : ITieredBenchmarkScoreStore
{
    public Dictionary<string, TieredBenchmarkScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBenchmarkScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBenchmarkScore item)
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

    public List<TieredBenchmarkScore> List()
    {
        var outValue = new List<TieredBenchmarkScore>();
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
public sealed class CachedTieredBenchmarkScoreStore(MemoryTieredBenchmarkScoreStore inner, TieredBenchmarkScoreCache cache, int hits, int misses) : ITieredBenchmarkScoreStore
{
    public MemoryTieredBenchmarkScoreStore Inner { get; set; } = inner;
    public TieredBenchmarkScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBenchmarkScore? Find(string code)
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

    public void Save(TieredBenchmarkScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBenchmarkScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered benchmark scores. Empty fields match all.</summary>
public sealed record TieredBenchmarkScoreQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBenchmarkScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBenchmarkScore> Apply(List<TieredBenchmarkScore> items)
    {
        var matched = Collections.Filter(items, (TieredBenchmarkScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered benchmark score store.</summary>
public static class TieredBenchmarkScoreStore
{
    public static ITieredBenchmarkScoreStore NewMemoryTieredBenchmarkScoreStore()
    {
        return new MemoryTieredBenchmarkScoreStore(
            new Dictionary<string, TieredBenchmarkScore>(),
            new List<string>());
    }

    public static ITieredBenchmarkScoreStore NewCachedTieredBenchmarkScoreStore(MemoryTieredBenchmarkScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBenchmarkScoreStore(inner, TieredBenchmarkScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredBenchmarkScore> FindTieredBenchmarkScores(ITieredBenchmarkScoreStore store, TieredBenchmarkScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
