namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return proposals by code.</summary>
public interface IAssetReturnProposalStore
{
    AssetReturnProposal? Find(string code);
    void Save(AssetReturnProposal item);
    bool Remove(string code);
    List<AssetReturnProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnProposalStore(Dictionary<string, AssetReturnProposal> items, List<string> order) : IAssetReturnProposalStore
{
    public Dictionary<string, AssetReturnProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnProposal item)
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

    public List<AssetReturnProposal> List()
    {
        var outValue = new List<AssetReturnProposal>();
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
public sealed class CachedAssetReturnProposalStore(MemoryAssetReturnProposalStore inner, AssetReturnProposalCache cache, int hits, int misses) : IAssetReturnProposalStore
{
    public MemoryAssetReturnProposalStore Inner { get; set; } = inner;
    public AssetReturnProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnProposal? Find(string code)
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

    public void Save(AssetReturnProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return proposals. Empty fields match all.</summary>
public sealed record AssetReturnProposalQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnProposal> Apply(List<AssetReturnProposal> items)
    {
        var matched = Collections.Filter(items, (AssetReturnProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return proposal store.</summary>
public static class AssetReturnProposalStore
{
    public static IAssetReturnProposalStore NewMemoryAssetReturnProposalStore()
    {
        return new MemoryAssetReturnProposalStore(new Dictionary<string, AssetReturnProposal>(), new List<string>());
    }

    public static IAssetReturnProposalStore NewCachedAssetReturnProposalStore(MemoryAssetReturnProposalStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnProposalStore(inner, AssetReturnProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssetReturnProposal> FindAssetReturnProposals(IAssetReturnProposalStore store, AssetReturnProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
