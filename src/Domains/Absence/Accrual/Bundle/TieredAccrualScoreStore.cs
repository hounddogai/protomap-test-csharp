namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered accrual scores by code.</summary>
public interface ITieredAccrualScoreStore
{
    TieredAccrualScore? Find(string code);
    void Save(TieredAccrualScore item);
    bool Remove(string code);
    List<TieredAccrualScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAccrualScoreStore(Dictionary<string, TieredAccrualScore> items, List<string> order) : ITieredAccrualScoreStore
{
    public Dictionary<string, TieredAccrualScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAccrualScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAccrualScore item)
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

    public List<TieredAccrualScore> List()
    {
        var outValue = new List<TieredAccrualScore>();
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
public sealed class CachedTieredAccrualScoreStore(MemoryTieredAccrualScoreStore inner, TieredAccrualScoreCache cache, int hits, int misses) : ITieredAccrualScoreStore
{
    public MemoryTieredAccrualScoreStore Inner { get; set; } = inner;
    public TieredAccrualScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAccrualScore? Find(string code)
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

    public void Save(TieredAccrualScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAccrualScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered accrual scores. Empty fields match all.</summary>
public sealed record TieredAccrualScoreQuery(string Department, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAccrualScore item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAccrualScore> Apply(List<TieredAccrualScore> items)
    {
        var matched = Collections.Filter(items, (TieredAccrualScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered accrual score store.</summary>
public static class TieredAccrualScoreStore
{
    public static ITieredAccrualScoreStore NewMemoryTieredAccrualScoreStore()
    {
        return new MemoryTieredAccrualScoreStore(new Dictionary<string, TieredAccrualScore>(), new List<string>());
    }

    public static ITieredAccrualScoreStore NewCachedTieredAccrualScoreStore(MemoryTieredAccrualScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAccrualScoreStore(inner, TieredAccrualScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TieredAccrualScore> FindTieredAccrualScores(ITieredAccrualScoreStore store, TieredAccrualScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
