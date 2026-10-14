namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback asset return drafts by code.</summary>
public interface IFallbackAssetReturnDraftStore
{
    FallbackAssetReturnDraft? Find(string code);
    void Save(FallbackAssetReturnDraft item);
    bool Remove(string code);
    List<FallbackAssetReturnDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAssetReturnDraftStore(Dictionary<string, FallbackAssetReturnDraft> items, List<string> order) : IFallbackAssetReturnDraftStore
{
    public Dictionary<string, FallbackAssetReturnDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAssetReturnDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAssetReturnDraft item)
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

    public List<FallbackAssetReturnDraft> List()
    {
        var outValue = new List<FallbackAssetReturnDraft>();
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
public sealed class CachedFallbackAssetReturnDraftStore(MemoryFallbackAssetReturnDraftStore inner, FallbackAssetReturnDraftCache cache, int hits, int misses) : IFallbackAssetReturnDraftStore
{
    public MemoryFallbackAssetReturnDraftStore Inner { get; set; } = inner;
    public FallbackAssetReturnDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAssetReturnDraft? Find(string code)
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

    public void Save(FallbackAssetReturnDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAssetReturnDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback asset return drafts. Empty fields match all.</summary>
public sealed record FallbackAssetReturnDraftQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAssetReturnDraft item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAssetReturnDraft> Apply(List<FallbackAssetReturnDraft> items)
    {
        var matched = Collections.Filter(items, (FallbackAssetReturnDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback asset return draft store.</summary>
public static class FallbackAssetReturnDraftStore
{
    public static IFallbackAssetReturnDraftStore NewMemoryFallbackAssetReturnDraftStore()
    {
        return new MemoryFallbackAssetReturnDraftStore(
            new Dictionary<string, FallbackAssetReturnDraft>(),
            new List<string>());
    }

    public static IFallbackAssetReturnDraftStore NewCachedFallbackAssetReturnDraftStore(MemoryFallbackAssetReturnDraftStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAssetReturnDraftStore(
            inner,
            FallbackAssetReturnDraftCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAssetReturnDraft> FindFallbackAssetReturnDrafts(IFallbackAssetReturnDraftStore store, FallbackAssetReturnDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
