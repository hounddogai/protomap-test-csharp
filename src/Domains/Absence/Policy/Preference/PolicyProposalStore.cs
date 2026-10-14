namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy proposals by code.</summary>
public interface IPolicyProposalStore
{
    PolicyProposal? Find(string code);
    void Save(PolicyProposal item);
    bool Remove(string code);
    List<PolicyProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyProposalStore(Dictionary<string, PolicyProposal> items, List<string> order) : IPolicyProposalStore
{
    public Dictionary<string, PolicyProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyProposal item)
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

    public List<PolicyProposal> List()
    {
        var outValue = new List<PolicyProposal>();
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
public sealed class CachedPolicyProposalStore(MemoryPolicyProposalStore inner, PolicyProposalCache cache, int hits, int misses) : IPolicyProposalStore
{
    public MemoryPolicyProposalStore Inner { get; set; } = inner;
    public PolicyProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyProposal? Find(string code)
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

    public void Save(PolicyProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy proposals. Empty fields match all.</summary>
public sealed record PolicyProposalQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyProposal> Apply(List<PolicyProposal> items)
    {
        var matched = Collections.Filter(items, (PolicyProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy proposal store.</summary>
public static class PolicyProposalStore
{
    public static IPolicyProposalStore NewMemoryPolicyProposalStore()
    {
        return new MemoryPolicyProposalStore(new Dictionary<string, PolicyProposal>(), new List<string>());
    }

    public static IPolicyProposalStore NewCachedPolicyProposalStore(MemoryPolicyProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyProposalStore(inner, PolicyProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyProposal> FindPolicyProposals(IPolicyProposalStore store, PolicyProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
