namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pool drafts by code.</summary>
public interface IFallbackPoolDraftStore
{
    FallbackPoolDraft? Find(string code);
    void Save(FallbackPoolDraft item);
    bool Remove(string code);
    List<FallbackPoolDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPoolDraftStore(Dictionary<string, FallbackPoolDraft> items, List<string> order) : IFallbackPoolDraftStore
{
    public Dictionary<string, FallbackPoolDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPoolDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPoolDraft item)
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

    public List<FallbackPoolDraft> List()
    {
        var outValue = new List<FallbackPoolDraft>();
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
public sealed class CachedFallbackPoolDraftStore(MemoryFallbackPoolDraftStore inner, FallbackPoolDraftCache cache, int hits, int misses) : IFallbackPoolDraftStore
{
    public MemoryFallbackPoolDraftStore Inner { get; set; } = inner;
    public FallbackPoolDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPoolDraft? Find(string code)
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

    public void Save(FallbackPoolDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPoolDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pool drafts. Empty fields match all.</summary>
public sealed record FallbackPoolDraftQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPoolDraft item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPoolDraft> Apply(List<FallbackPoolDraft> items)
    {
        var matched = Collections.Filter(items, (FallbackPoolDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pool draft store.</summary>
public static class FallbackPoolDraftStore
{
    public static IFallbackPoolDraftStore NewMemoryFallbackPoolDraftStore()
    {
        return new MemoryFallbackPoolDraftStore(new Dictionary<string, FallbackPoolDraft>(), new List<string>());
    }

    public static IFallbackPoolDraftStore NewCachedFallbackPoolDraftStore(MemoryFallbackPoolDraftStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPoolDraftStore(inner, FallbackPoolDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FallbackPoolDraft> FindFallbackPoolDrafts(IFallbackPoolDraftStore store, FallbackPoolDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
