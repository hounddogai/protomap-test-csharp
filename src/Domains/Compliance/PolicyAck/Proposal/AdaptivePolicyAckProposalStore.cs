namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive policy ack proposals by code.</summary>
public interface IAdaptivePolicyAckProposalStore
{
    AdaptivePolicyAckProposal? Find(string code);
    void Save(AdaptivePolicyAckProposal item);
    bool Remove(string code);
    List<AdaptivePolicyAckProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePolicyAckProposalStore(Dictionary<string, AdaptivePolicyAckProposal> items, List<string> order) : IAdaptivePolicyAckProposalStore
{
    public Dictionary<string, AdaptivePolicyAckProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePolicyAckProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePolicyAckProposal item)
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

    public List<AdaptivePolicyAckProposal> List()
    {
        var outValue = new List<AdaptivePolicyAckProposal>();
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
public sealed class CachedAdaptivePolicyAckProposalStore(MemoryAdaptivePolicyAckProposalStore inner, AdaptivePolicyAckProposalCache cache, int hits, int misses) : IAdaptivePolicyAckProposalStore
{
    public MemoryAdaptivePolicyAckProposalStore Inner { get; set; } = inner;
    public AdaptivePolicyAckProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePolicyAckProposal? Find(string code)
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

    public void Save(AdaptivePolicyAckProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePolicyAckProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive policy ack proposals. Empty fields match all.</summary>
public sealed record AdaptivePolicyAckProposalQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePolicyAckProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePolicyAckProposal> Apply(List<AdaptivePolicyAckProposal> items)
    {
        var matched = Collections.Filter(items, (AdaptivePolicyAckProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive policy ack proposal store.</summary>
public static class AdaptivePolicyAckProposalStore
{
    public static IAdaptivePolicyAckProposalStore NewMemoryAdaptivePolicyAckProposalStore()
    {
        return new MemoryAdaptivePolicyAckProposalStore(
            new Dictionary<string, AdaptivePolicyAckProposal>(),
            new List<string>());
    }

    public static IAdaptivePolicyAckProposalStore NewCachedAdaptivePolicyAckProposalStore(MemoryAdaptivePolicyAckProposalStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePolicyAckProposalStore(
            inner,
            AdaptivePolicyAckProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptivePolicyAckProposal> FindAdaptivePolicyAckProposals(IAdaptivePolicyAckProposalStore store, AdaptivePolicyAckProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
