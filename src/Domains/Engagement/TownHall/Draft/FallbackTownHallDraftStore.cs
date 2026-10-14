namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback town hall drafts by code.</summary>
public interface IFallbackTownHallDraftStore
{
    FallbackTownHallDraft? Find(string code);
    void Save(FallbackTownHallDraft item);
    bool Remove(string code);
    List<FallbackTownHallDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTownHallDraftStore(Dictionary<string, FallbackTownHallDraft> items, List<string> order) : IFallbackTownHallDraftStore
{
    public Dictionary<string, FallbackTownHallDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTownHallDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTownHallDraft item)
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

    public List<FallbackTownHallDraft> List()
    {
        var outValue = new List<FallbackTownHallDraft>();
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
public sealed class CachedFallbackTownHallDraftStore(MemoryFallbackTownHallDraftStore inner, FallbackTownHallDraftCache cache, int hits, int misses) : IFallbackTownHallDraftStore
{
    public MemoryFallbackTownHallDraftStore Inner { get; set; } = inner;
    public FallbackTownHallDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTownHallDraft? Find(string code)
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

    public void Save(FallbackTownHallDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTownHallDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback town hall drafts. Empty fields match all.</summary>
public sealed record FallbackTownHallDraftQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTownHallDraft item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackTownHallDraft> Apply(List<FallbackTownHallDraft> items)
    {
        var matched = Collections.Filter(items, (FallbackTownHallDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback town hall draft store.</summary>
public static class FallbackTownHallDraftStore
{
    public static IFallbackTownHallDraftStore NewMemoryFallbackTownHallDraftStore()
    {
        return new MemoryFallbackTownHallDraftStore(
            new Dictionary<string, FallbackTownHallDraft>(),
            new List<string>());
    }

    public static IFallbackTownHallDraftStore NewCachedFallbackTownHallDraftStore(MemoryFallbackTownHallDraftStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTownHallDraftStore(inner, FallbackTownHallDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FallbackTownHallDraft> FindFallbackTownHallDrafts(IFallbackTownHallDraftStore store, FallbackTownHallDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
