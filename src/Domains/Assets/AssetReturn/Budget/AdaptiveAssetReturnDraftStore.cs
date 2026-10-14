namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive asset return drafts by code.</summary>
public interface IAdaptiveAssetReturnDraftStore
{
    AdaptiveAssetReturnDraft? Find(string code);
    void Save(AdaptiveAssetReturnDraft item);
    bool Remove(string code);
    List<AdaptiveAssetReturnDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAssetReturnDraftStore(Dictionary<string, AdaptiveAssetReturnDraft> items, List<string> order) : IAdaptiveAssetReturnDraftStore
{
    public Dictionary<string, AdaptiveAssetReturnDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAssetReturnDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAssetReturnDraft item)
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

    public List<AdaptiveAssetReturnDraft> List()
    {
        var outValue = new List<AdaptiveAssetReturnDraft>();
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
public sealed class CachedAdaptiveAssetReturnDraftStore(MemoryAdaptiveAssetReturnDraftStore inner, AdaptiveAssetReturnDraftCache cache, int hits, int misses) : IAdaptiveAssetReturnDraftStore
{
    public MemoryAdaptiveAssetReturnDraftStore Inner { get; set; } = inner;
    public AdaptiveAssetReturnDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAssetReturnDraft? Find(string code)
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

    public void Save(AdaptiveAssetReturnDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAssetReturnDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive asset return drafts. Empty fields match all.</summary>
public sealed record AdaptiveAssetReturnDraftQuery(string Segment, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAssetReturnDraft item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAssetReturnDraft> Apply(List<AdaptiveAssetReturnDraft> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAssetReturnDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive asset return draft store.</summary>
public static class AdaptiveAssetReturnDraftStore
{
    public static IAdaptiveAssetReturnDraftStore NewMemoryAdaptiveAssetReturnDraftStore()
    {
        return new MemoryAdaptiveAssetReturnDraftStore(
            new Dictionary<string, AdaptiveAssetReturnDraft>(),
            new List<string>());
    }

    public static IAdaptiveAssetReturnDraftStore NewCachedAdaptiveAssetReturnDraftStore(MemoryAdaptiveAssetReturnDraftStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAssetReturnDraftStore(
            inner,
            AdaptiveAssetReturnDraftCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAssetReturnDraft> FindAdaptiveAssetReturnDrafts(IAdaptiveAssetReturnDraftStore store, AdaptiveAssetReturnDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
