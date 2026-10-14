namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped merit cycle scores by code.</summary>
public interface ICappedMeritCycleScoreStore
{
    CappedMeritCycleScore? Find(string code);
    void Save(CappedMeritCycleScore item);
    bool Remove(string code);
    List<CappedMeritCycleScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedMeritCycleScoreStore(Dictionary<string, CappedMeritCycleScore> items, List<string> order) : ICappedMeritCycleScoreStore
{
    public Dictionary<string, CappedMeritCycleScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedMeritCycleScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedMeritCycleScore item)
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

    public List<CappedMeritCycleScore> List()
    {
        var outValue = new List<CappedMeritCycleScore>();
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
public sealed class CachedCappedMeritCycleScoreStore(MemoryCappedMeritCycleScoreStore inner, CappedMeritCycleScoreCache cache, int hits, int misses) : ICappedMeritCycleScoreStore
{
    public MemoryCappedMeritCycleScoreStore Inner { get; set; } = inner;
    public CappedMeritCycleScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedMeritCycleScore? Find(string code)
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

    public void Save(CappedMeritCycleScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedMeritCycleScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped merit cycle scores. Empty fields match all.</summary>
public sealed record CappedMeritCycleScoreQuery(string Region, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedMeritCycleScore item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<CappedMeritCycleScore> Apply(List<CappedMeritCycleScore> items)
    {
        var matched = Collections.Filter(items, (CappedMeritCycleScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped merit cycle score store.</summary>
public static class CappedMeritCycleScoreStore
{
    public static ICappedMeritCycleScoreStore NewMemoryCappedMeritCycleScoreStore()
    {
        return new MemoryCappedMeritCycleScoreStore(
            new Dictionary<string, CappedMeritCycleScore>(),
            new List<string>());
    }

    public static ICappedMeritCycleScoreStore NewCachedCappedMeritCycleScoreStore(MemoryCappedMeritCycleScoreStore inner, int ttl, IClock clock)
    {
        return new CachedCappedMeritCycleScoreStore(
            inner,
            CappedMeritCycleScoreCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedMeritCycleScore> FindCappedMeritCycleScores(ICappedMeritCycleScoreStore store, CappedMeritCycleScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
