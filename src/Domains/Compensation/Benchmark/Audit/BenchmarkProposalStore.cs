namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark proposals by code.</summary>
public interface IBenchmarkProposalStore
{
    BenchmarkProposal? Find(string code);
    void Save(BenchmarkProposal item);
    bool Remove(string code);
    List<BenchmarkProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkProposalStore(Dictionary<string, BenchmarkProposal> items, List<string> order) : IBenchmarkProposalStore
{
    public Dictionary<string, BenchmarkProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkProposal item)
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

    public List<BenchmarkProposal> List()
    {
        var outValue = new List<BenchmarkProposal>();
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
public sealed class CachedBenchmarkProposalStore(MemoryBenchmarkProposalStore inner, BenchmarkProposalCache cache, int hits, int misses) : IBenchmarkProposalStore
{
    public MemoryBenchmarkProposalStore Inner { get; set; } = inner;
    public BenchmarkProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkProposal? Find(string code)
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

    public void Save(BenchmarkProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark proposals. Empty fields match all.</summary>
public sealed record BenchmarkProposalQuery(string Locale, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkProposal item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkProposal> Apply(List<BenchmarkProposal> items)
    {
        var matched = Collections.Filter(items, (BenchmarkProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark proposal store.</summary>
public static class BenchmarkProposalStore
{
    public static IBenchmarkProposalStore NewMemoryBenchmarkProposalStore()
    {
        return new MemoryBenchmarkProposalStore(new Dictionary<string, BenchmarkProposal>(), new List<string>());
    }

    public static IBenchmarkProposalStore NewCachedBenchmarkProposalStore(MemoryBenchmarkProposalStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkProposalStore(inner, BenchmarkProposalCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BenchmarkProposal> FindBenchmarkProposals(IBenchmarkProposalStore store, BenchmarkProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
