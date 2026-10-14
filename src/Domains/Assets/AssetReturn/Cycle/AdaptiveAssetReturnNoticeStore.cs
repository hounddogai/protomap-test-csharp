namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive asset return notices by code.</summary>
public interface IAdaptiveAssetReturnNoticeStore
{
    AdaptiveAssetReturnNotice? Find(string code);
    void Save(AdaptiveAssetReturnNotice item);
    bool Remove(string code);
    List<AdaptiveAssetReturnNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAssetReturnNoticeStore(Dictionary<string, AdaptiveAssetReturnNotice> items, List<string> order) : IAdaptiveAssetReturnNoticeStore
{
    public Dictionary<string, AdaptiveAssetReturnNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAssetReturnNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAssetReturnNotice item)
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

    public List<AdaptiveAssetReturnNotice> List()
    {
        var outValue = new List<AdaptiveAssetReturnNotice>();
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
public sealed class CachedAdaptiveAssetReturnNoticeStore(MemoryAdaptiveAssetReturnNoticeStore inner, AdaptiveAssetReturnNoticeCache cache, int hits, int misses) : IAdaptiveAssetReturnNoticeStore
{
    public MemoryAdaptiveAssetReturnNoticeStore Inner { get; set; } = inner;
    public AdaptiveAssetReturnNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAssetReturnNotice? Find(string code)
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

    public void Save(AdaptiveAssetReturnNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAssetReturnNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive asset return notices. Empty fields match all.</summary>
public sealed record AdaptiveAssetReturnNoticeQuery(string Category, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAssetReturnNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<AdaptiveAssetReturnNotice> Apply(List<AdaptiveAssetReturnNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAssetReturnNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive asset return notice store.</summary>
public static class AdaptiveAssetReturnNoticeStore
{
    public static IAdaptiveAssetReturnNoticeStore NewMemoryAdaptiveAssetReturnNoticeStore()
    {
        return new MemoryAdaptiveAssetReturnNoticeStore(
            new Dictionary<string, AdaptiveAssetReturnNotice>(),
            new List<string>());
    }

    public static IAdaptiveAssetReturnNoticeStore NewCachedAdaptiveAssetReturnNoticeStore(MemoryAdaptiveAssetReturnNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAssetReturnNoticeStore(
            inner,
            AdaptiveAssetReturnNoticeCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAssetReturnNotice> FindAdaptiveAssetReturnNotices(IAdaptiveAssetReturnNoticeStore store, AdaptiveAssetReturnNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
