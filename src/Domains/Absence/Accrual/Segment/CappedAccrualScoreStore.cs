namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped accrual scores by code.</summary>
public interface ICappedAccrualScoreStore
{
    CappedAccrualScore? Find(string code);
    void Save(CappedAccrualScore item);
    bool Remove(string code);
    List<CappedAccrualScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAccrualScoreStore(Dictionary<string, CappedAccrualScore> items, List<string> order) : ICappedAccrualScoreStore
{
    public Dictionary<string, CappedAccrualScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAccrualScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAccrualScore item)
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

    public List<CappedAccrualScore> List()
    {
        var outValue = new List<CappedAccrualScore>();
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
public sealed class CachedCappedAccrualScoreStore(MemoryCappedAccrualScoreStore inner, CappedAccrualScoreCache cache, int hits, int misses) : ICappedAccrualScoreStore
{
    public MemoryCappedAccrualScoreStore Inner { get; set; } = inner;
    public CappedAccrualScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAccrualScore? Find(string code)
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

    public void Save(CappedAccrualScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAccrualScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped accrual scores. Empty fields match all.</summary>
public sealed record CappedAccrualScoreQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAccrualScore item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAccrualScore> Apply(List<CappedAccrualScore> items)
    {
        var matched = Collections.Filter(items, (CappedAccrualScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped accrual score store.</summary>
public static class CappedAccrualScoreStore
{
    public static ICappedAccrualScoreStore NewMemoryCappedAccrualScoreStore()
    {
        return new MemoryCappedAccrualScoreStore(new Dictionary<string, CappedAccrualScore>(), new List<string>());
    }

    public static ICappedAccrualScoreStore NewCachedCappedAccrualScoreStore(MemoryCappedAccrualScoreStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAccrualScoreStore(inner, CappedAccrualScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedAccrualScore> FindCappedAccrualScores(ICappedAccrualScoreStore store, CappedAccrualScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
