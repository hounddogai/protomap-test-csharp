namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient span rollups by code.</summary>
public interface ILenientSpanRollupStore
{
    LenientSpanRollup? Find(string code);
    void Save(LenientSpanRollup item);
    bool Remove(string code);
    List<LenientSpanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSpanRollupStore(Dictionary<string, LenientSpanRollup> items, List<string> order) : ILenientSpanRollupStore
{
    public Dictionary<string, LenientSpanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSpanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSpanRollup item)
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

    public List<LenientSpanRollup> List()
    {
        var outValue = new List<LenientSpanRollup>();
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
public sealed class CachedLenientSpanRollupStore(MemoryLenientSpanRollupStore inner, LenientSpanRollupCache cache, int hits, int misses) : ILenientSpanRollupStore
{
    public MemoryLenientSpanRollupStore Inner { get; set; } = inner;
    public LenientSpanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSpanRollup? Find(string code)
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

    public void Save(LenientSpanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSpanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient span rollups. Empty fields match all.</summary>
public sealed record LenientSpanRollupQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSpanRollup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSpanRollup> Apply(List<LenientSpanRollup> items)
    {
        var matched = Collections.Filter(items, (LenientSpanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient span rollup store.</summary>
public static class LenientSpanRollupStore
{
    public static ILenientSpanRollupStore NewMemoryLenientSpanRollupStore()
    {
        return new MemoryLenientSpanRollupStore(new Dictionary<string, LenientSpanRollup>(), new List<string>());
    }

    public static ILenientSpanRollupStore NewCachedLenientSpanRollupStore(MemoryLenientSpanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSpanRollupStore(inner, LenientSpanRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientSpanRollup> FindLenientSpanRollups(ILenientSpanRollupStore store, LenientSpanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
