namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pay band scores by code.</summary>
public interface IFallbackPayBandScoreStore
{
    FallbackPayBandScore? Find(string code);
    void Save(FallbackPayBandScore item);
    bool Remove(string code);
    List<FallbackPayBandScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPayBandScoreStore(Dictionary<string, FallbackPayBandScore> items, List<string> order) : IFallbackPayBandScoreStore
{
    public Dictionary<string, FallbackPayBandScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPayBandScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPayBandScore item)
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

    public List<FallbackPayBandScore> List()
    {
        var outValue = new List<FallbackPayBandScore>();
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
public sealed class CachedFallbackPayBandScoreStore(MemoryFallbackPayBandScoreStore inner, FallbackPayBandScoreCache cache, int hits, int misses) : IFallbackPayBandScoreStore
{
    public MemoryFallbackPayBandScoreStore Inner { get; set; } = inner;
    public FallbackPayBandScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPayBandScore? Find(string code)
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

    public void Save(FallbackPayBandScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPayBandScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pay band scores. Empty fields match all.</summary>
public sealed record FallbackPayBandScoreQuery(string Segment, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPayBandScore item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<FallbackPayBandScore> Apply(List<FallbackPayBandScore> items)
    {
        var matched = Collections.Filter(items, (FallbackPayBandScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pay band score store.</summary>
public static class FallbackPayBandScoreStore
{
    public static IFallbackPayBandScoreStore NewMemoryFallbackPayBandScoreStore()
    {
        return new MemoryFallbackPayBandScoreStore(
            new Dictionary<string, FallbackPayBandScore>(),
            new List<string>());
    }

    public static IFallbackPayBandScoreStore NewCachedFallbackPayBandScoreStore(MemoryFallbackPayBandScoreStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPayBandScoreStore(inner, FallbackPayBandScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackPayBandScore> FindFallbackPayBandScores(IFallbackPayBandScoreStore store, FallbackPayBandScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
