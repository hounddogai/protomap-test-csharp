namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient benchmark proposals by code.</summary>
public interface ILenientBenchmarkProposalStore
{
    LenientBenchmarkProposal? Find(string code);
    void Save(LenientBenchmarkProposal item);
    bool Remove(string code);
    List<LenientBenchmarkProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBenchmarkProposalStore(Dictionary<string, LenientBenchmarkProposal> items, List<string> order) : ILenientBenchmarkProposalStore
{
    public Dictionary<string, LenientBenchmarkProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBenchmarkProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBenchmarkProposal item)
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

    public List<LenientBenchmarkProposal> List()
    {
        var outValue = new List<LenientBenchmarkProposal>();
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
public sealed class CachedLenientBenchmarkProposalStore(MemoryLenientBenchmarkProposalStore inner, LenientBenchmarkProposalCache cache, int hits, int misses) : ILenientBenchmarkProposalStore
{
    public MemoryLenientBenchmarkProposalStore Inner { get; set; } = inner;
    public LenientBenchmarkProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBenchmarkProposal? Find(string code)
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

    public void Save(LenientBenchmarkProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBenchmarkProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient benchmark proposals. Empty fields match all.</summary>
public sealed record LenientBenchmarkProposalQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBenchmarkProposal item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBenchmarkProposal> Apply(List<LenientBenchmarkProposal> items)
    {
        var matched = Collections.Filter(items, (LenientBenchmarkProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient benchmark proposal store.</summary>
public static class LenientBenchmarkProposalStore
{
    public static ILenientBenchmarkProposalStore NewMemoryLenientBenchmarkProposalStore()
    {
        return new MemoryLenientBenchmarkProposalStore(
            new Dictionary<string, LenientBenchmarkProposal>(),
            new List<string>());
    }

    public static ILenientBenchmarkProposalStore NewCachedLenientBenchmarkProposalStore(MemoryLenientBenchmarkProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBenchmarkProposalStore(
            inner,
            LenientBenchmarkProposalCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientBenchmarkProposal> FindLenientBenchmarkProposals(ILenientBenchmarkProposalStore store, LenientBenchmarkProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
