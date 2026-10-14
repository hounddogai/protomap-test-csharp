namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate scores by code.</summary>
public interface IRateScoreStore
{
    RateScore? Find(string code);
    void Save(RateScore item);
    bool Remove(string code);
    List<RateScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateScoreStore(Dictionary<string, RateScore> items, List<string> order) : IRateScoreStore
{
    public Dictionary<string, RateScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateScore item)
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

    public List<RateScore> List()
    {
        var outValue = new List<RateScore>();
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
public sealed class CachedRateScoreStore(MemoryRateScoreStore inner, RateScoreCache cache, int hits, int misses) : IRateScoreStore
{
    public MemoryRateScoreStore Inner { get; set; } = inner;
    public RateScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateScore? Find(string code)
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

    public void Save(RateScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate scores. Empty fields match all.</summary>
public sealed record RateScoreQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RateScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateScore> Apply(List<RateScore> items)
    {
        var matched = Collections.Filter(items, (RateScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate score store.</summary>
public static class RateScoreStore
{
    public static IRateScoreStore NewMemoryRateScoreStore()
    {
        return new MemoryRateScoreStore(new Dictionary<string, RateScore>(), new List<string>());
    }

    public static IRateScoreStore NewCachedRateScoreStore(MemoryRateScoreStore inner, int ttl, IClock clock)
    {
        return new CachedRateScoreStore(inner, RateScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RateScore> FindRateScores(IRateScoreStore store, RateScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
