namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment proposals by code.</summary>
public interface IAssignmentProposalStore
{
    AssignmentProposal? Find(string code);
    void Save(AssignmentProposal item);
    bool Remove(string code);
    List<AssignmentProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentProposalStore(Dictionary<string, AssignmentProposal> items, List<string> order) : IAssignmentProposalStore
{
    public Dictionary<string, AssignmentProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentProposal item)
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

    public List<AssignmentProposal> List()
    {
        var outValue = new List<AssignmentProposal>();
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
public sealed class CachedAssignmentProposalStore(MemoryAssignmentProposalStore inner, AssignmentProposalCache cache, int hits, int misses) : IAssignmentProposalStore
{
    public MemoryAssignmentProposalStore Inner { get; set; } = inner;
    public AssignmentProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentProposal? Find(string code)
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

    public void Save(AssignmentProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment proposals. Empty fields match all.</summary>
public sealed record AssignmentProposalQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentProposal> Apply(List<AssignmentProposal> items)
    {
        var matched = Collections.Filter(items, (AssignmentProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment proposal store.</summary>
public static class AssignmentProposalStore
{
    public static IAssignmentProposalStore NewMemoryAssignmentProposalStore()
    {
        return new MemoryAssignmentProposalStore(new Dictionary<string, AssignmentProposal>(), new List<string>());
    }

    public static IAssignmentProposalStore NewCachedAssignmentProposalStore(MemoryAssignmentProposalStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentProposalStore(inner, AssignmentProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentProposal> FindAssignmentProposals(IAssignmentProposalStore store, AssignmentProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
