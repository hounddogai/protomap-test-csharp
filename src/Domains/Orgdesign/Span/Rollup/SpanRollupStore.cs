namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span rollups by code.</summary>
public interface ISpanRollupStore
{
    SpanRollup? Find(string code);
    void Save(SpanRollup item);
    bool Remove(string code);
    List<SpanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanRollupStore(Dictionary<string, SpanRollup> items, List<string> order) : ISpanRollupStore
{
    public Dictionary<string, SpanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanRollup item)
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

    public List<SpanRollup> List()
    {
        var outValue = new List<SpanRollup>();
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
public sealed class CachedSpanRollupStore(MemorySpanRollupStore inner, SpanRollupCache cache, int hits, int misses) : ISpanRollupStore
{
    public MemorySpanRollupStore Inner { get; set; } = inner;
    public SpanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanRollup? Find(string code)
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

    public void Save(SpanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span rollups. Empty fields match all.</summary>
public sealed record SpanRollupQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SpanRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanRollup> Apply(List<SpanRollup> items)
    {
        var matched = Collections.Filter(items, (SpanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span rollup store.</summary>
public static class SpanRollupStore
{
    public static ISpanRollupStore NewMemorySpanRollupStore()
    {
        return new MemorySpanRollupStore(new Dictionary<string, SpanRollup>(), new List<string>());
    }

    public static ISpanRollupStore NewCachedSpanRollupStore(MemorySpanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSpanRollupStore(inner, SpanRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanRollup> FindSpanRollups(ISpanRollupStore store, SpanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
