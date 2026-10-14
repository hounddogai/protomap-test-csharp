namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction proposals by code.</summary>
public interface IDeductionProposalStore
{
    DeductionProposal? Find(string code);
    void Save(DeductionProposal item);
    bool Remove(string code);
    List<DeductionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionProposalStore(Dictionary<string, DeductionProposal> items, List<string> order) : IDeductionProposalStore
{
    public Dictionary<string, DeductionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionProposal item)
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

    public List<DeductionProposal> List()
    {
        var outValue = new List<DeductionProposal>();
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
public sealed class CachedDeductionProposalStore(MemoryDeductionProposalStore inner, DeductionProposalCache cache, int hits, int misses) : IDeductionProposalStore
{
    public MemoryDeductionProposalStore Inner { get; set; } = inner;
    public DeductionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionProposal? Find(string code)
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

    public void Save(DeductionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction proposals. Empty fields match all.</summary>
public sealed record DeductionProposalQuery(string Team, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionProposal item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<DeductionProposal> Apply(List<DeductionProposal> items)
    {
        var matched = Collections.Filter(items, (DeductionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction proposal store.</summary>
public static class DeductionProposalStore
{
    public static IDeductionProposalStore NewMemoryDeductionProposalStore()
    {
        return new MemoryDeductionProposalStore(new Dictionary<string, DeductionProposal>(), new List<string>());
    }

    public static IDeductionProposalStore NewCachedDeductionProposalStore(MemoryDeductionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionProposalStore(inner, DeductionProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionProposal> FindDeductionProposals(IDeductionProposalStore store, DeductionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
