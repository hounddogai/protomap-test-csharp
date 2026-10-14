namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled benchmark proposals by code.</summary>
public interface IScheduledBenchmarkProposalStore
{
    ScheduledBenchmarkProposal? Find(string code);
    void Save(ScheduledBenchmarkProposal item);
    bool Remove(string code);
    List<ScheduledBenchmarkProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBenchmarkProposalStore(Dictionary<string, ScheduledBenchmarkProposal> items, List<string> order) : IScheduledBenchmarkProposalStore
{
    public Dictionary<string, ScheduledBenchmarkProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBenchmarkProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBenchmarkProposal item)
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

    public List<ScheduledBenchmarkProposal> List()
    {
        var outValue = new List<ScheduledBenchmarkProposal>();
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
public sealed class CachedScheduledBenchmarkProposalStore(MemoryScheduledBenchmarkProposalStore inner, ScheduledBenchmarkProposalCache cache, int hits, int misses) : IScheduledBenchmarkProposalStore
{
    public MemoryScheduledBenchmarkProposalStore Inner { get; set; } = inner;
    public ScheduledBenchmarkProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBenchmarkProposal? Find(string code)
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

    public void Save(ScheduledBenchmarkProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBenchmarkProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled benchmark proposals. Empty fields match all.</summary>
public sealed record ScheduledBenchmarkProposalQuery(string Region, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBenchmarkProposal item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBenchmarkProposal> Apply(List<ScheduledBenchmarkProposal> items)
    {
        var matched = Collections.Filter(items, (ScheduledBenchmarkProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled benchmark proposal store.</summary>
public static class ScheduledBenchmarkProposalStore
{
    public static IScheduledBenchmarkProposalStore NewMemoryScheduledBenchmarkProposalStore()
    {
        return new MemoryScheduledBenchmarkProposalStore(
            new Dictionary<string, ScheduledBenchmarkProposal>(),
            new List<string>());
    }

    public static IScheduledBenchmarkProposalStore NewCachedScheduledBenchmarkProposalStore(MemoryScheduledBenchmarkProposalStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBenchmarkProposalStore(
            inner,
            ScheduledBenchmarkProposalCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBenchmarkProposal> FindScheduledBenchmarkProposals(IScheduledBenchmarkProposalStore store, ScheduledBenchmarkProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
