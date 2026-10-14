namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance proposals by code.</summary>
public interface IBalanceProposalStore
{
    BalanceProposal? Find(string code);
    void Save(BalanceProposal item);
    bool Remove(string code);
    List<BalanceProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceProposalStore(Dictionary<string, BalanceProposal> items, List<string> order) : IBalanceProposalStore
{
    public Dictionary<string, BalanceProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceProposal item)
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

    public List<BalanceProposal> List()
    {
        var outValue = new List<BalanceProposal>();
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
public sealed class CachedBalanceProposalStore(MemoryBalanceProposalStore inner, BalanceProposalCache cache, int hits, int misses) : IBalanceProposalStore
{
    public MemoryBalanceProposalStore Inner { get; set; } = inner;
    public BalanceProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceProposal? Find(string code)
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

    public void Save(BalanceProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance proposals. Empty fields match all.</summary>
public sealed record BalanceProposalQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceProposal> Apply(List<BalanceProposal> items)
    {
        var matched = Collections.Filter(items, (BalanceProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance proposal store.</summary>
public static class BalanceProposalStore
{
    public static IBalanceProposalStore NewMemoryBalanceProposalStore()
    {
        return new MemoryBalanceProposalStore(new Dictionary<string, BalanceProposal>(), new List<string>());
    }

    public static IBalanceProposalStore NewCachedBalanceProposalStore(MemoryBalanceProposalStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceProposalStore(inner, BalanceProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BalanceProposal> FindBalanceProposals(IBalanceProposalStore store, BalanceProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
