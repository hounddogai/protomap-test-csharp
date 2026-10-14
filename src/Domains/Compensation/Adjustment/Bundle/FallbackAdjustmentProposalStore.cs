namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback adjustment proposals by code.</summary>
public interface IFallbackAdjustmentProposalStore
{
    FallbackAdjustmentProposal? Find(string code);
    void Save(FallbackAdjustmentProposal item);
    bool Remove(string code);
    List<FallbackAdjustmentProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAdjustmentProposalStore(Dictionary<string, FallbackAdjustmentProposal> items, List<string> order) : IFallbackAdjustmentProposalStore
{
    public Dictionary<string, FallbackAdjustmentProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAdjustmentProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAdjustmentProposal item)
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

    public List<FallbackAdjustmentProposal> List()
    {
        var outValue = new List<FallbackAdjustmentProposal>();
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
public sealed class CachedFallbackAdjustmentProposalStore(MemoryFallbackAdjustmentProposalStore inner, FallbackAdjustmentProposalCache cache, int hits, int misses) : IFallbackAdjustmentProposalStore
{
    public MemoryFallbackAdjustmentProposalStore Inner { get; set; } = inner;
    public FallbackAdjustmentProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAdjustmentProposal? Find(string code)
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

    public void Save(FallbackAdjustmentProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAdjustmentProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback adjustment proposals. Empty fields match all.</summary>
public sealed record FallbackAdjustmentProposalQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAdjustmentProposal item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAdjustmentProposal> Apply(List<FallbackAdjustmentProposal> items)
    {
        var matched = Collections.Filter(items, (FallbackAdjustmentProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback adjustment proposal store.</summary>
public static class FallbackAdjustmentProposalStore
{
    public static IFallbackAdjustmentProposalStore NewMemoryFallbackAdjustmentProposalStore()
    {
        return new MemoryFallbackAdjustmentProposalStore(
            new Dictionary<string, FallbackAdjustmentProposal>(),
            new List<string>());
    }

    public static IFallbackAdjustmentProposalStore NewCachedFallbackAdjustmentProposalStore(MemoryFallbackAdjustmentProposalStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAdjustmentProposalStore(
            inner,
            FallbackAdjustmentProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAdjustmentProposal> FindFallbackAdjustmentProposals(IFallbackAdjustmentProposalStore store, FallbackAdjustmentProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
