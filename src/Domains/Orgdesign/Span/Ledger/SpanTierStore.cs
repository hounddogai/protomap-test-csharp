namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span tiers by code.</summary>
public interface ISpanTierStore
{
    SpanTier? Find(string code);
    void Save(SpanTier item);
    bool Remove(string code);
    List<SpanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanTierStore(Dictionary<string, SpanTier> items, List<string> order) : ISpanTierStore
{
    public Dictionary<string, SpanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanTier item)
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

    public List<SpanTier> List()
    {
        var outValue = new List<SpanTier>();
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
public sealed class CachedSpanTierStore(MemorySpanTierStore inner, SpanTierCache cache, int hits, int misses) : ISpanTierStore
{
    public MemorySpanTierStore Inner { get; set; } = inner;
    public SpanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanTier? Find(string code)
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

    public void Save(SpanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span tiers. Empty fields match all.</summary>
public sealed record SpanTierQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(SpanTier item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanTier> Apply(List<SpanTier> items)
    {
        var matched = Collections.Filter(items, (SpanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span tier store.</summary>
public static class SpanTierStore
{
    public static ISpanTierStore NewMemorySpanTierStore()
    {
        return new MemorySpanTierStore(new Dictionary<string, SpanTier>(), new List<string>());
    }

    public static ISpanTierStore NewCachedSpanTierStore(MemorySpanTierStore inner, int ttl, IClock clock)
    {
        return new CachedSpanTierStore(inner, SpanTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SpanTier> FindSpanTiers(ISpanTierStore store, SpanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
