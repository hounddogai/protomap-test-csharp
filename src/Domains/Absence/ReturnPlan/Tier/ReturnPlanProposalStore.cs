namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan proposals by code.</summary>
public interface IReturnPlanProposalStore
{
    ReturnPlanProposal? Find(string code);
    void Save(ReturnPlanProposal item);
    bool Remove(string code);
    List<ReturnPlanProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanProposalStore(Dictionary<string, ReturnPlanProposal> items, List<string> order) : IReturnPlanProposalStore
{
    public Dictionary<string, ReturnPlanProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanProposal item)
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

    public List<ReturnPlanProposal> List()
    {
        var outValue = new List<ReturnPlanProposal>();
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
public sealed class CachedReturnPlanProposalStore(MemoryReturnPlanProposalStore inner, ReturnPlanProposalCache cache, int hits, int misses) : IReturnPlanProposalStore
{
    public MemoryReturnPlanProposalStore Inner { get; set; } = inner;
    public ReturnPlanProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanProposal? Find(string code)
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

    public void Save(ReturnPlanProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan proposals. Empty fields match all.</summary>
public sealed record ReturnPlanProposalQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanProposal item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanProposal> Apply(List<ReturnPlanProposal> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan proposal store.</summary>
public static class ReturnPlanProposalStore
{
    public static IReturnPlanProposalStore NewMemoryReturnPlanProposalStore()
    {
        return new MemoryReturnPlanProposalStore(new Dictionary<string, ReturnPlanProposal>(), new List<string>());
    }

    public static IReturnPlanProposalStore NewCachedReturnPlanProposalStore(MemoryReturnPlanProposalStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanProposalStore(inner, ReturnPlanProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanProposal> FindReturnPlanProposals(IReturnPlanProposalStore store, ReturnPlanProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
