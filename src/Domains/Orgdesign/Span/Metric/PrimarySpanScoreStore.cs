namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary span scores by code.</summary>
public interface IPrimarySpanScoreStore
{
    PrimarySpanScore? Find(string code);
    void Save(PrimarySpanScore item);
    bool Remove(string code);
    List<PrimarySpanScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimarySpanScoreStore(Dictionary<string, PrimarySpanScore> items, List<string> order) : IPrimarySpanScoreStore
{
    public Dictionary<string, PrimarySpanScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimarySpanScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimarySpanScore item)
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

    public List<PrimarySpanScore> List()
    {
        var outValue = new List<PrimarySpanScore>();
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
public sealed class CachedPrimarySpanScoreStore(MemoryPrimarySpanScoreStore inner, PrimarySpanScoreCache cache, int hits, int misses) : IPrimarySpanScoreStore
{
    public MemoryPrimarySpanScoreStore Inner { get; set; } = inner;
    public PrimarySpanScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimarySpanScore? Find(string code)
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

    public void Save(PrimarySpanScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimarySpanScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary span scores. Empty fields match all.</summary>
public sealed record PrimarySpanScoreQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimarySpanScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimarySpanScore> Apply(List<PrimarySpanScore> items)
    {
        var matched = Collections.Filter(items, (PrimarySpanScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary span score store.</summary>
public static class PrimarySpanScoreStore
{
    public static IPrimarySpanScoreStore NewMemoryPrimarySpanScoreStore()
    {
        return new MemoryPrimarySpanScoreStore(new Dictionary<string, PrimarySpanScore>(), new List<string>());
    }

    public static IPrimarySpanScoreStore NewCachedPrimarySpanScoreStore(MemoryPrimarySpanScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPrimarySpanScoreStore(inner, PrimarySpanScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimarySpanScore> FindPrimarySpanScores(IPrimarySpanScoreStore store, PrimarySpanScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
