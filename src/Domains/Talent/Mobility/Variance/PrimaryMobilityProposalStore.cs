namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary mobility proposals by code.</summary>
public interface IPrimaryMobilityProposalStore
{
    PrimaryMobilityProposal? Find(string code);
    void Save(PrimaryMobilityProposal item);
    bool Remove(string code);
    List<PrimaryMobilityProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryMobilityProposalStore(Dictionary<string, PrimaryMobilityProposal> items, List<string> order) : IPrimaryMobilityProposalStore
{
    public Dictionary<string, PrimaryMobilityProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryMobilityProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryMobilityProposal item)
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

    public List<PrimaryMobilityProposal> List()
    {
        var outValue = new List<PrimaryMobilityProposal>();
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
public sealed class CachedPrimaryMobilityProposalStore(MemoryPrimaryMobilityProposalStore inner, PrimaryMobilityProposalCache cache, int hits, int misses) : IPrimaryMobilityProposalStore
{
    public MemoryPrimaryMobilityProposalStore Inner { get; set; } = inner;
    public PrimaryMobilityProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryMobilityProposal? Find(string code)
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

    public void Save(PrimaryMobilityProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryMobilityProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary mobility proposals. Empty fields match all.</summary>
public sealed record PrimaryMobilityProposalQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryMobilityProposal item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryMobilityProposal> Apply(List<PrimaryMobilityProposal> items)
    {
        var matched = Collections.Filter(items, (PrimaryMobilityProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary mobility proposal store.</summary>
public static class PrimaryMobilityProposalStore
{
    public static IPrimaryMobilityProposalStore NewMemoryPrimaryMobilityProposalStore()
    {
        return new MemoryPrimaryMobilityProposalStore(
            new Dictionary<string, PrimaryMobilityProposal>(),
            new List<string>());
    }

    public static IPrimaryMobilityProposalStore NewCachedPrimaryMobilityProposalStore(MemoryPrimaryMobilityProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryMobilityProposalStore(
            inner,
            PrimaryMobilityProposalCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryMobilityProposal> FindPrimaryMobilityProposals(IPrimaryMobilityProposalStore store, PrimaryMobilityProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
