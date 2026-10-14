namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional asset return proposals by code.</summary>
public interface IRegionalAssetReturnProposalStore
{
    RegionalAssetReturnProposal? Find(string code);
    void Save(RegionalAssetReturnProposal item);
    bool Remove(string code);
    List<RegionalAssetReturnProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAssetReturnProposalStore(Dictionary<string, RegionalAssetReturnProposal> items, List<string> order) : IRegionalAssetReturnProposalStore
{
    public Dictionary<string, RegionalAssetReturnProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAssetReturnProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAssetReturnProposal item)
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

    public List<RegionalAssetReturnProposal> List()
    {
        var outValue = new List<RegionalAssetReturnProposal>();
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
public sealed class CachedRegionalAssetReturnProposalStore(MemoryRegionalAssetReturnProposalStore inner, RegionalAssetReturnProposalCache cache, int hits, int misses) : IRegionalAssetReturnProposalStore
{
    public MemoryRegionalAssetReturnProposalStore Inner { get; set; } = inner;
    public RegionalAssetReturnProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAssetReturnProposal? Find(string code)
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

    public void Save(RegionalAssetReturnProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAssetReturnProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional asset return proposals. Empty fields match all.</summary>
public sealed record RegionalAssetReturnProposalQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAssetReturnProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAssetReturnProposal> Apply(List<RegionalAssetReturnProposal> items)
    {
        var matched = Collections.Filter(items, (RegionalAssetReturnProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional asset return proposal store.</summary>
public static class RegionalAssetReturnProposalStore
{
    public static IRegionalAssetReturnProposalStore NewMemoryRegionalAssetReturnProposalStore()
    {
        return new MemoryRegionalAssetReturnProposalStore(
            new Dictionary<string, RegionalAssetReturnProposal>(),
            new List<string>());
    }

    public static IRegionalAssetReturnProposalStore NewCachedRegionalAssetReturnProposalStore(MemoryRegionalAssetReturnProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAssetReturnProposalStore(
            inner,
            RegionalAssetReturnProposalCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAssetReturnProposal> FindRegionalAssetReturnProposals(IRegionalAssetReturnProposalStore store, RegionalAssetReturnProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
