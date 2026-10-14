namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict rate proposals by code.</summary>
public interface IStrictRateProposalStore
{
    StrictRateProposal? Find(string code);
    void Save(StrictRateProposal item);
    bool Remove(string code);
    List<StrictRateProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRateProposalStore(Dictionary<string, StrictRateProposal> items, List<string> order) : IStrictRateProposalStore
{
    public Dictionary<string, StrictRateProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRateProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRateProposal item)
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

    public List<StrictRateProposal> List()
    {
        var outValue = new List<StrictRateProposal>();
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
public sealed class CachedStrictRateProposalStore(MemoryStrictRateProposalStore inner, StrictRateProposalCache cache, int hits, int misses) : IStrictRateProposalStore
{
    public MemoryStrictRateProposalStore Inner { get; set; } = inner;
    public StrictRateProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRateProposal? Find(string code)
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

    public void Save(StrictRateProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRateProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict rate proposals. Empty fields match all.</summary>
public sealed record StrictRateProposalQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRateProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictRateProposal> Apply(List<StrictRateProposal> items)
    {
        var matched = Collections.Filter(items, (StrictRateProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict rate proposal store.</summary>
public static class StrictRateProposalStore
{
    public static IStrictRateProposalStore NewMemoryStrictRateProposalStore()
    {
        return new MemoryStrictRateProposalStore(new Dictionary<string, StrictRateProposal>(), new List<string>());
    }

    public static IStrictRateProposalStore NewCachedStrictRateProposalStore(MemoryStrictRateProposalStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRateProposalStore(inner, StrictRateProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictRateProposal> FindStrictRateProposals(IStrictRateProposalStore store, StrictRateProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
