namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary benchmark claims by code.</summary>
public interface IPrimaryBenchmarkClaimStore
{
    PrimaryBenchmarkClaim? Find(string code);
    void Save(PrimaryBenchmarkClaim item);
    bool Remove(string code);
    List<PrimaryBenchmarkClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryBenchmarkClaimStore(Dictionary<string, PrimaryBenchmarkClaim> items, List<string> order) : IPrimaryBenchmarkClaimStore
{
    public Dictionary<string, PrimaryBenchmarkClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryBenchmarkClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryBenchmarkClaim item)
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

    public List<PrimaryBenchmarkClaim> List()
    {
        var outValue = new List<PrimaryBenchmarkClaim>();
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
public sealed class CachedPrimaryBenchmarkClaimStore(MemoryPrimaryBenchmarkClaimStore inner, PrimaryBenchmarkClaimCache cache, int hits, int misses) : IPrimaryBenchmarkClaimStore
{
    public MemoryPrimaryBenchmarkClaimStore Inner { get; set; } = inner;
    public PrimaryBenchmarkClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryBenchmarkClaim? Find(string code)
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

    public void Save(PrimaryBenchmarkClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryBenchmarkClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary benchmark claims. Empty fields match all.</summary>
public sealed record PrimaryBenchmarkClaimQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryBenchmarkClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PrimaryBenchmarkClaim> Apply(List<PrimaryBenchmarkClaim> items)
    {
        var matched = Collections.Filter(items, (PrimaryBenchmarkClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary benchmark claim store.</summary>
public static class PrimaryBenchmarkClaimStore
{
    public static IPrimaryBenchmarkClaimStore NewMemoryPrimaryBenchmarkClaimStore()
    {
        return new MemoryPrimaryBenchmarkClaimStore(
            new Dictionary<string, PrimaryBenchmarkClaim>(),
            new List<string>());
    }

    public static IPrimaryBenchmarkClaimStore NewCachedPrimaryBenchmarkClaimStore(MemoryPrimaryBenchmarkClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryBenchmarkClaimStore(
            inner,
            PrimaryBenchmarkClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryBenchmarkClaim> FindPrimaryBenchmarkClaims(IPrimaryBenchmarkClaimStore store, PrimaryBenchmarkClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
