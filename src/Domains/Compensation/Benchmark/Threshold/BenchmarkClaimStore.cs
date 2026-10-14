namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark claims by code.</summary>
public interface IBenchmarkClaimStore
{
    BenchmarkClaim? Find(string code);
    void Save(BenchmarkClaim item);
    bool Remove(string code);
    List<BenchmarkClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkClaimStore(Dictionary<string, BenchmarkClaim> items, List<string> order) : IBenchmarkClaimStore
{
    public Dictionary<string, BenchmarkClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkClaim item)
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

    public List<BenchmarkClaim> List()
    {
        var outValue = new List<BenchmarkClaim>();
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
public sealed class CachedBenchmarkClaimStore(MemoryBenchmarkClaimStore inner, BenchmarkClaimCache cache, int hits, int misses) : IBenchmarkClaimStore
{
    public MemoryBenchmarkClaimStore Inner { get; set; } = inner;
    public BenchmarkClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkClaim? Find(string code)
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

    public void Save(BenchmarkClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark claims. Empty fields match all.</summary>
public sealed record BenchmarkClaimQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkClaim item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<BenchmarkClaim> Apply(List<BenchmarkClaim> items)
    {
        var matched = Collections.Filter(items, (BenchmarkClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark claim store.</summary>
public static class BenchmarkClaimStore
{
    public static IBenchmarkClaimStore NewMemoryBenchmarkClaimStore()
    {
        return new MemoryBenchmarkClaimStore(new Dictionary<string, BenchmarkClaim>(), new List<string>());
    }

    public static IBenchmarkClaimStore NewCachedBenchmarkClaimStore(MemoryBenchmarkClaimStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkClaimStore(inner, BenchmarkClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkClaim> FindBenchmarkClaims(IBenchmarkClaimStore store, BenchmarkClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
