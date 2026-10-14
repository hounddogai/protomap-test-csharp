namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive mobility proposals by code.</summary>
public interface IAdaptiveMobilityProposalStore
{
    AdaptiveMobilityProposal? Find(string code);
    void Save(AdaptiveMobilityProposal item);
    bool Remove(string code);
    List<AdaptiveMobilityProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveMobilityProposalStore(Dictionary<string, AdaptiveMobilityProposal> items, List<string> order) : IAdaptiveMobilityProposalStore
{
    public Dictionary<string, AdaptiveMobilityProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveMobilityProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveMobilityProposal item)
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

    public List<AdaptiveMobilityProposal> List()
    {
        var outValue = new List<AdaptiveMobilityProposal>();
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
public sealed class CachedAdaptiveMobilityProposalStore(MemoryAdaptiveMobilityProposalStore inner, AdaptiveMobilityProposalCache cache, int hits, int misses) : IAdaptiveMobilityProposalStore
{
    public MemoryAdaptiveMobilityProposalStore Inner { get; set; } = inner;
    public AdaptiveMobilityProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveMobilityProposal? Find(string code)
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

    public void Save(AdaptiveMobilityProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveMobilityProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive mobility proposals. Empty fields match all.</summary>
public sealed record AdaptiveMobilityProposalQuery(string Country, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveMobilityProposal item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveMobilityProposal> Apply(List<AdaptiveMobilityProposal> items)
    {
        var matched = Collections.Filter(items, (AdaptiveMobilityProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive mobility proposal store.</summary>
public static class AdaptiveMobilityProposalStore
{
    public static IAdaptiveMobilityProposalStore NewMemoryAdaptiveMobilityProposalStore()
    {
        return new MemoryAdaptiveMobilityProposalStore(
            new Dictionary<string, AdaptiveMobilityProposal>(),
            new List<string>());
    }

    public static IAdaptiveMobilityProposalStore NewCachedAdaptiveMobilityProposalStore(MemoryAdaptiveMobilityProposalStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveMobilityProposalStore(
            inner,
            AdaptiveMobilityProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveMobilityProposal> FindAdaptiveMobilityProposals(IAdaptiveMobilityProposalStore store, AdaptiveMobilityProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
