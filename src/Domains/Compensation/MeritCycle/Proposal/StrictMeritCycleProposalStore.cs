namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict merit cycle proposals by code.</summary>
public interface IStrictMeritCycleProposalStore
{
    StrictMeritCycleProposal? Find(string code);
    void Save(StrictMeritCycleProposal item);
    bool Remove(string code);
    List<StrictMeritCycleProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictMeritCycleProposalStore(Dictionary<string, StrictMeritCycleProposal> items, List<string> order) : IStrictMeritCycleProposalStore
{
    public Dictionary<string, StrictMeritCycleProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictMeritCycleProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictMeritCycleProposal item)
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

    public List<StrictMeritCycleProposal> List()
    {
        var outValue = new List<StrictMeritCycleProposal>();
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
public sealed class CachedStrictMeritCycleProposalStore(MemoryStrictMeritCycleProposalStore inner, StrictMeritCycleProposalCache cache, int hits, int misses) : IStrictMeritCycleProposalStore
{
    public MemoryStrictMeritCycleProposalStore Inner { get; set; } = inner;
    public StrictMeritCycleProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictMeritCycleProposal? Find(string code)
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

    public void Save(StrictMeritCycleProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictMeritCycleProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict merit cycle proposals. Empty fields match all.</summary>
public sealed record StrictMeritCycleProposalQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(StrictMeritCycleProposal item)
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

    public List<StrictMeritCycleProposal> Apply(List<StrictMeritCycleProposal> items)
    {
        var matched = Collections.Filter(items, (StrictMeritCycleProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict merit cycle proposal store.</summary>
public static class StrictMeritCycleProposalStore
{
    public static IStrictMeritCycleProposalStore NewMemoryStrictMeritCycleProposalStore()
    {
        return new MemoryStrictMeritCycleProposalStore(
            new Dictionary<string, StrictMeritCycleProposal>(),
            new List<string>());
    }

    public static IStrictMeritCycleProposalStore NewCachedStrictMeritCycleProposalStore(MemoryStrictMeritCycleProposalStore inner, int ttl, IClock clock)
    {
        return new CachedStrictMeritCycleProposalStore(
            inner,
            StrictMeritCycleProposalCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictMeritCycleProposal> FindStrictMeritCycleProposals(IStrictMeritCycleProposalStore store, StrictMeritCycleProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
