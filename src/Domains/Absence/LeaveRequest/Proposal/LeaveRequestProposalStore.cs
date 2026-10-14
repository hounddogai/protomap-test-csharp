namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request proposals by code.</summary>
public interface ILeaveRequestProposalStore
{
    LeaveRequestProposal? Find(string code);
    void Save(LeaveRequestProposal item);
    bool Remove(string code);
    List<LeaveRequestProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestProposalStore(Dictionary<string, LeaveRequestProposal> items, List<string> order) : ILeaveRequestProposalStore
{
    public Dictionary<string, LeaveRequestProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestProposal item)
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

    public List<LeaveRequestProposal> List()
    {
        var outValue = new List<LeaveRequestProposal>();
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
public sealed class CachedLeaveRequestProposalStore(MemoryLeaveRequestProposalStore inner, LeaveRequestProposalCache cache, int hits, int misses) : ILeaveRequestProposalStore
{
    public MemoryLeaveRequestProposalStore Inner { get; set; } = inner;
    public LeaveRequestProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestProposal? Find(string code)
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

    public void Save(LeaveRequestProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request proposals. Empty fields match all.</summary>
public sealed record LeaveRequestProposalQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestProposal item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LeaveRequestProposal> Apply(List<LeaveRequestProposal> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request proposal store.</summary>
public static class LeaveRequestProposalStore
{
    public static ILeaveRequestProposalStore NewMemoryLeaveRequestProposalStore()
    {
        return new MemoryLeaveRequestProposalStore(
            new Dictionary<string, LeaveRequestProposal>(),
            new List<string>());
    }

    public static ILeaveRequestProposalStore NewCachedLeaveRequestProposalStore(MemoryLeaveRequestProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestProposalStore(inner, LeaveRequestProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestProposal> FindLeaveRequestProposals(ILeaveRequestProposalStore store, LeaveRequestProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
