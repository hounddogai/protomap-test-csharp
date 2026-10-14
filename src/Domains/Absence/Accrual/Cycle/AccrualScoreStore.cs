namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual scores by code.</summary>
public interface IAccrualScoreStore
{
    AccrualScore? Find(string code);
    void Save(AccrualScore item);
    bool Remove(string code);
    List<AccrualScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualScoreStore(Dictionary<string, AccrualScore> items, List<string> order) : IAccrualScoreStore
{
    public Dictionary<string, AccrualScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualScore item)
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

    public List<AccrualScore> List()
    {
        var outValue = new List<AccrualScore>();
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
public sealed class CachedAccrualScoreStore(MemoryAccrualScoreStore inner, AccrualScoreCache cache, int hits, int misses) : IAccrualScoreStore
{
    public MemoryAccrualScoreStore Inner { get; set; } = inner;
    public AccrualScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualScore? Find(string code)
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

    public void Save(AccrualScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual scores. Empty fields match all.</summary>
public sealed record AccrualScoreQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualScore item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualScore> Apply(List<AccrualScore> items)
    {
        var matched = Collections.Filter(items, (AccrualScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual score store.</summary>
public static class AccrualScoreStore
{
    public static IAccrualScoreStore NewMemoryAccrualScoreStore()
    {
        return new MemoryAccrualScoreStore(new Dictionary<string, AccrualScore>(), new List<string>());
    }

    public static IAccrualScoreStore NewCachedAccrualScoreStore(MemoryAccrualScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualScoreStore(inner, AccrualScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualScore> FindAccrualScores(IAccrualScoreStore store, AccrualScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
