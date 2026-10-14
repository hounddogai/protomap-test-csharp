namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant proposals by code.</summary>
public interface IEquityGrantProposalStore
{
    EquityGrantProposal? Find(string code);
    void Save(EquityGrantProposal item);
    bool Remove(string code);
    List<EquityGrantProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantProposalStore(Dictionary<string, EquityGrantProposal> items, List<string> order) : IEquityGrantProposalStore
{
    public Dictionary<string, EquityGrantProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantProposal item)
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

    public List<EquityGrantProposal> List()
    {
        var outValue = new List<EquityGrantProposal>();
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
public sealed class CachedEquityGrantProposalStore(MemoryEquityGrantProposalStore inner, EquityGrantProposalCache cache, int hits, int misses) : IEquityGrantProposalStore
{
    public MemoryEquityGrantProposalStore Inner { get; set; } = inner;
    public EquityGrantProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantProposal? Find(string code)
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

    public void Save(EquityGrantProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant proposals. Empty fields match all.</summary>
public sealed record EquityGrantProposalQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantProposal item)
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

    public List<EquityGrantProposal> Apply(List<EquityGrantProposal> items)
    {
        var matched = Collections.Filter(items, (EquityGrantProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant proposal store.</summary>
public static class EquityGrantProposalStore
{
    public static IEquityGrantProposalStore NewMemoryEquityGrantProposalStore()
    {
        return new MemoryEquityGrantProposalStore(new Dictionary<string, EquityGrantProposal>(), new List<string>());
    }

    public static IEquityGrantProposalStore NewCachedEquityGrantProposalStore(MemoryEquityGrantProposalStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantProposalStore(inner, EquityGrantProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantProposal> FindEquityGrantProposals(IEquityGrantProposalStore store, EquityGrantProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
