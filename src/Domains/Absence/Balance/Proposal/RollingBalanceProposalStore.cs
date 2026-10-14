namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling balance proposals by code.</summary>
public interface IRollingBalanceProposalStore
{
    RollingBalanceProposal? Find(string code);
    void Save(RollingBalanceProposal item);
    bool Remove(string code);
    List<RollingBalanceProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingBalanceProposalStore(Dictionary<string, RollingBalanceProposal> items, List<string> order) : IRollingBalanceProposalStore
{
    public Dictionary<string, RollingBalanceProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingBalanceProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingBalanceProposal item)
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

    public List<RollingBalanceProposal> List()
    {
        var outValue = new List<RollingBalanceProposal>();
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
public sealed class CachedRollingBalanceProposalStore(MemoryRollingBalanceProposalStore inner, RollingBalanceProposalCache cache, int hits, int misses) : IRollingBalanceProposalStore
{
    public MemoryRollingBalanceProposalStore Inner { get; set; } = inner;
    public RollingBalanceProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingBalanceProposal? Find(string code)
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

    public void Save(RollingBalanceProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingBalanceProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling balance proposals. Empty fields match all.</summary>
public sealed record RollingBalanceProposalQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RollingBalanceProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingBalanceProposal> Apply(List<RollingBalanceProposal> items)
    {
        var matched = Collections.Filter(items, (RollingBalanceProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling balance proposal store.</summary>
public static class RollingBalanceProposalStore
{
    public static IRollingBalanceProposalStore NewMemoryRollingBalanceProposalStore()
    {
        return new MemoryRollingBalanceProposalStore(
            new Dictionary<string, RollingBalanceProposal>(),
            new List<string>());
    }

    public static IRollingBalanceProposalStore NewCachedRollingBalanceProposalStore(MemoryRollingBalanceProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRollingBalanceProposalStore(
            inner,
            RollingBalanceProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingBalanceProposal> FindRollingBalanceProposals(IRollingBalanceProposalStore store, RollingBalanceProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
