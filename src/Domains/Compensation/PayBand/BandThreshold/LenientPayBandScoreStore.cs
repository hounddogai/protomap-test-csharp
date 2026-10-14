namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pay band scores by code.</summary>
public interface ILenientPayBandScoreStore
{
    LenientPayBandScore? Find(string code);
    void Save(LenientPayBandScore item);
    bool Remove(string code);
    List<LenientPayBandScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayBandScoreStore(Dictionary<string, LenientPayBandScore> items, List<string> order) : ILenientPayBandScoreStore
{
    public Dictionary<string, LenientPayBandScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayBandScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayBandScore item)
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

    public List<LenientPayBandScore> List()
    {
        var outValue = new List<LenientPayBandScore>();
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
public sealed class CachedLenientPayBandScoreStore(MemoryLenientPayBandScoreStore inner, LenientPayBandScoreCache cache, int hits, int misses) : ILenientPayBandScoreStore
{
    public MemoryLenientPayBandScoreStore Inner { get; set; } = inner;
    public LenientPayBandScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayBandScore? Find(string code)
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

    public void Save(LenientPayBandScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayBandScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pay band scores. Empty fields match all.</summary>
public sealed record LenientPayBandScoreQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayBandScore item)
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

    public List<LenientPayBandScore> Apply(List<LenientPayBandScore> items)
    {
        var matched = Collections.Filter(items, (LenientPayBandScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pay band score store.</summary>
public static class LenientPayBandScoreStore
{
    public static ILenientPayBandScoreStore NewMemoryLenientPayBandScoreStore()
    {
        return new MemoryLenientPayBandScoreStore(new Dictionary<string, LenientPayBandScore>(), new List<string>());
    }

    public static ILenientPayBandScoreStore NewCachedLenientPayBandScoreStore(MemoryLenientPayBandScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayBandScoreStore(inner, LenientPayBandScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientPayBandScore> FindLenientPayBandScores(ILenientPayBandScoreStore store, LenientPayBandScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
