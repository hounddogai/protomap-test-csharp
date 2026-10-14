namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate drafts by code.</summary>
public interface IRateDraftStore
{
    RateDraft? Find(string code);
    void Save(RateDraft item);
    bool Remove(string code);
    List<RateDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateDraftStore(Dictionary<string, RateDraft> items, List<string> order) : IRateDraftStore
{
    public Dictionary<string, RateDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateDraft item)
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

    public List<RateDraft> List()
    {
        var outValue = new List<RateDraft>();
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
public sealed class CachedRateDraftStore(MemoryRateDraftStore inner, RateDraftCache cache, int hits, int misses) : IRateDraftStore
{
    public MemoryRateDraftStore Inner { get; set; } = inner;
    public RateDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateDraft? Find(string code)
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

    public void Save(RateDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate drafts. Empty fields match all.</summary>
public sealed record RateDraftQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RateDraft item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<RateDraft> Apply(List<RateDraft> items)
    {
        var matched = Collections.Filter(items, (RateDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate draft store.</summary>
public static class RateDraftStore
{
    public static IRateDraftStore NewMemoryRateDraftStore()
    {
        return new MemoryRateDraftStore(new Dictionary<string, RateDraft>(), new List<string>());
    }

    public static IRateDraftStore NewCachedRateDraftStore(MemoryRateDraftStore inner, int ttl, IClock clock)
    {
        return new CachedRateDraftStore(inner, RateDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateDraft> FindRateDrafts(IRateDraftStore store, RateDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
