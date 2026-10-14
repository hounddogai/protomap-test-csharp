namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal benchmark digests by code.</summary>
public interface ISeasonalBenchmarkDigestStore
{
    SeasonalBenchmarkDigest? Find(string code);
    void Save(SeasonalBenchmarkDigest item);
    bool Remove(string code);
    List<SeasonalBenchmarkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalBenchmarkDigestStore(Dictionary<string, SeasonalBenchmarkDigest> items, List<string> order) : ISeasonalBenchmarkDigestStore
{
    public Dictionary<string, SeasonalBenchmarkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalBenchmarkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalBenchmarkDigest item)
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

    public List<SeasonalBenchmarkDigest> List()
    {
        var outValue = new List<SeasonalBenchmarkDigest>();
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
public sealed class CachedSeasonalBenchmarkDigestStore(MemorySeasonalBenchmarkDigestStore inner, SeasonalBenchmarkDigestCache cache, int hits, int misses) : ISeasonalBenchmarkDigestStore
{
    public MemorySeasonalBenchmarkDigestStore Inner { get; set; } = inner;
    public SeasonalBenchmarkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalBenchmarkDigest? Find(string code)
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

    public void Save(SeasonalBenchmarkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalBenchmarkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal benchmark digests. Empty fields match all.</summary>
public sealed record SeasonalBenchmarkDigestQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalBenchmarkDigest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalBenchmarkDigest> Apply(List<SeasonalBenchmarkDigest> items)
    {
        var matched = Collections.Filter(items, (SeasonalBenchmarkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal benchmark digest store.</summary>
public static class SeasonalBenchmarkDigestStore
{
    public static ISeasonalBenchmarkDigestStore NewMemorySeasonalBenchmarkDigestStore()
    {
        return new MemorySeasonalBenchmarkDigestStore(
            new Dictionary<string, SeasonalBenchmarkDigest>(),
            new List<string>());
    }

    public static ISeasonalBenchmarkDigestStore NewCachedSeasonalBenchmarkDigestStore(MemorySeasonalBenchmarkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalBenchmarkDigestStore(
            inner,
            SeasonalBenchmarkDigestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalBenchmarkDigest> FindSeasonalBenchmarkDigests(ISeasonalBenchmarkDigestStore store, SeasonalBenchmarkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
