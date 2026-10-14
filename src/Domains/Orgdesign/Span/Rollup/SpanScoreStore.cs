namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span scores by code.</summary>
public interface ISpanScoreStore
{
    SpanScore? Find(string code);
    void Save(SpanScore item);
    bool Remove(string code);
    List<SpanScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanScoreStore(Dictionary<string, SpanScore> items, List<string> order) : ISpanScoreStore
{
    public Dictionary<string, SpanScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanScore item)
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

    public List<SpanScore> List()
    {
        var outValue = new List<SpanScore>();
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
public sealed class CachedSpanScoreStore(MemorySpanScoreStore inner, SpanScoreCache cache, int hits, int misses) : ISpanScoreStore
{
    public MemorySpanScoreStore Inner { get; set; } = inner;
    public SpanScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanScore? Find(string code)
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

    public void Save(SpanScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span scores. Empty fields match all.</summary>
public sealed record SpanScoreQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanScore> Apply(List<SpanScore> items)
    {
        var matched = Collections.Filter(items, (SpanScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span score store.</summary>
public static class SpanScoreStore
{
    public static ISpanScoreStore NewMemorySpanScoreStore()
    {
        return new MemorySpanScoreStore(new Dictionary<string, SpanScore>(), new List<string>());
    }

    public static ISpanScoreStore NewCachedSpanScoreStore(MemorySpanScoreStore inner, int ttl, IClock clock)
    {
        return new CachedSpanScoreStore(inner, SpanScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanScore> FindSpanScores(ISpanScoreStore store, SpanScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
