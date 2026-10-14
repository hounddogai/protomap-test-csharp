namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict desk proposals by code.</summary>
public interface IStrictDeskProposalStore
{
    StrictDeskProposal? Find(string code);
    void Save(StrictDeskProposal item);
    bool Remove(string code);
    List<StrictDeskProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictDeskProposalStore(Dictionary<string, StrictDeskProposal> items, List<string> order) : IStrictDeskProposalStore
{
    public Dictionary<string, StrictDeskProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictDeskProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictDeskProposal item)
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

    public List<StrictDeskProposal> List()
    {
        var outValue = new List<StrictDeskProposal>();
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
public sealed class CachedStrictDeskProposalStore(MemoryStrictDeskProposalStore inner, StrictDeskProposalCache cache, int hits, int misses) : IStrictDeskProposalStore
{
    public MemoryStrictDeskProposalStore Inner { get; set; } = inner;
    public StrictDeskProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictDeskProposal? Find(string code)
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

    public void Save(StrictDeskProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictDeskProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict desk proposals. Empty fields match all.</summary>
public sealed record StrictDeskProposalQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(StrictDeskProposal item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictDeskProposal> Apply(List<StrictDeskProposal> items)
    {
        var matched = Collections.Filter(items, (StrictDeskProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict desk proposal store.</summary>
public static class StrictDeskProposalStore
{
    public static IStrictDeskProposalStore NewMemoryStrictDeskProposalStore()
    {
        return new MemoryStrictDeskProposalStore(new Dictionary<string, StrictDeskProposal>(), new List<string>());
    }

    public static IStrictDeskProposalStore NewCachedStrictDeskProposalStore(MemoryStrictDeskProposalStore inner, int ttl, IClock clock)
    {
        return new CachedStrictDeskProposalStore(inner, StrictDeskProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictDeskProposal> FindStrictDeskProposals(IStrictDeskProposalStore store, StrictDeskProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
