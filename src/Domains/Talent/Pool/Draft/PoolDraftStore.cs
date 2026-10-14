namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool drafts by code.</summary>
public interface IPoolDraftStore
{
    PoolDraft? Find(string code);
    void Save(PoolDraft item);
    bool Remove(string code);
    List<PoolDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolDraftStore(Dictionary<string, PoolDraft> items, List<string> order) : IPoolDraftStore
{
    public Dictionary<string, PoolDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolDraft item)
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

    public List<PoolDraft> List()
    {
        var outValue = new List<PoolDraft>();
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
public sealed class CachedPoolDraftStore(MemoryPoolDraftStore inner, PoolDraftCache cache, int hits, int misses) : IPoolDraftStore
{
    public MemoryPoolDraftStore Inner { get; set; } = inner;
    public PoolDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolDraft? Find(string code)
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

    public void Save(PoolDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool drafts. Empty fields match all.</summary>
public sealed record PoolDraftQuery(string Locale, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PoolDraft item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolDraft> Apply(List<PoolDraft> items)
    {
        var matched = Collections.Filter(items, (PoolDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool draft store.</summary>
public static class PoolDraftStore
{
    public static IPoolDraftStore NewMemoryPoolDraftStore()
    {
        return new MemoryPoolDraftStore(new Dictionary<string, PoolDraft>(), new List<string>());
    }

    public static IPoolDraftStore NewCachedPoolDraftStore(MemoryPoolDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPoolDraftStore(inner, PoolDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolDraft> FindPoolDrafts(IPoolDraftStore store, PoolDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
