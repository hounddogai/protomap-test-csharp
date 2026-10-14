namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pay band scores by code.</summary>
public interface ITieredPayBandScoreStore
{
    TieredPayBandScore? Find(string code);
    void Save(TieredPayBandScore item);
    bool Remove(string code);
    List<TieredPayBandScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPayBandScoreStore(Dictionary<string, TieredPayBandScore> items, List<string> order) : ITieredPayBandScoreStore
{
    public Dictionary<string, TieredPayBandScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPayBandScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPayBandScore item)
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

    public List<TieredPayBandScore> List()
    {
        var outValue = new List<TieredPayBandScore>();
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
public sealed class CachedTieredPayBandScoreStore(MemoryTieredPayBandScoreStore inner, TieredPayBandScoreCache cache, int hits, int misses) : ITieredPayBandScoreStore
{
    public MemoryTieredPayBandScoreStore Inner { get; set; } = inner;
    public TieredPayBandScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPayBandScore? Find(string code)
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

    public void Save(TieredPayBandScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPayBandScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pay band scores. Empty fields match all.</summary>
public sealed record TieredPayBandScoreQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPayBandScore item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPayBandScore> Apply(List<TieredPayBandScore> items)
    {
        var matched = Collections.Filter(items, (TieredPayBandScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pay band score store.</summary>
public static class TieredPayBandScoreStore
{
    public static ITieredPayBandScoreStore NewMemoryTieredPayBandScoreStore()
    {
        return new MemoryTieredPayBandScoreStore(new Dictionary<string, TieredPayBandScore>(), new List<string>());
    }

    public static ITieredPayBandScoreStore NewCachedTieredPayBandScoreStore(MemoryTieredPayBandScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPayBandScoreStore(inner, TieredPayBandScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredPayBandScore> FindTieredPayBandScores(ITieredPayBandScoreStore store, TieredPayBandScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
