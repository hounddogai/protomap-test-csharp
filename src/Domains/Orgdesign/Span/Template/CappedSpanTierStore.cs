namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped span tiers by code.</summary>
public interface ICappedSpanTierStore
{
    CappedSpanTier? Find(string code);
    void Save(CappedSpanTier item);
    bool Remove(string code);
    List<CappedSpanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSpanTierStore(Dictionary<string, CappedSpanTier> items, List<string> order) : ICappedSpanTierStore
{
    public Dictionary<string, CappedSpanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSpanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSpanTier item)
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

    public List<CappedSpanTier> List()
    {
        var outValue = new List<CappedSpanTier>();
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
public sealed class CachedCappedSpanTierStore(MemoryCappedSpanTierStore inner, CappedSpanTierCache cache, int hits, int misses) : ICappedSpanTierStore
{
    public MemoryCappedSpanTierStore Inner { get; set; } = inner;
    public CappedSpanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSpanTier? Find(string code)
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

    public void Save(CappedSpanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSpanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped span tiers. Empty fields match all.</summary>
public sealed record CappedSpanTierQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSpanTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSpanTier> Apply(List<CappedSpanTier> items)
    {
        var matched = Collections.Filter(items, (CappedSpanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped span tier store.</summary>
public static class CappedSpanTierStore
{
    public static ICappedSpanTierStore NewMemoryCappedSpanTierStore()
    {
        return new MemoryCappedSpanTierStore(new Dictionary<string, CappedSpanTier>(), new List<string>());
    }

    public static ICappedSpanTierStore NewCachedCappedSpanTierStore(MemoryCappedSpanTierStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSpanTierStore(inner, CappedSpanTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedSpanTier> FindCappedSpanTiers(ICappedSpanTierStore store, CappedSpanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
