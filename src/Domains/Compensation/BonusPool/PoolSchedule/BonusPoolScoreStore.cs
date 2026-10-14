namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool scores by code.</summary>
public interface IBonusPoolScoreStore
{
    BonusPoolScore? Find(string code);
    void Save(BonusPoolScore item);
    bool Remove(string code);
    List<BonusPoolScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolScoreStore(Dictionary<string, BonusPoolScore> items, List<string> order) : IBonusPoolScoreStore
{
    public Dictionary<string, BonusPoolScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolScore item)
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

    public List<BonusPoolScore> List()
    {
        var outValue = new List<BonusPoolScore>();
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
public sealed class CachedBonusPoolScoreStore(MemoryBonusPoolScoreStore inner, BonusPoolScoreCache cache, int hits, int misses) : IBonusPoolScoreStore
{
    public MemoryBonusPoolScoreStore Inner { get; set; } = inner;
    public BonusPoolScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolScore? Find(string code)
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

    public void Save(BonusPoolScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool scores. Empty fields match all.</summary>
public sealed record BonusPoolScoreQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolScore> Apply(List<BonusPoolScore> items)
    {
        var matched = Collections.Filter(items, (BonusPoolScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool score store.</summary>
public static class BonusPoolScoreStore
{
    public static IBonusPoolScoreStore NewMemoryBonusPoolScoreStore()
    {
        return new MemoryBonusPoolScoreStore(new Dictionary<string, BonusPoolScore>(), new List<string>());
    }

    public static IBonusPoolScoreStore NewCachedBonusPoolScoreStore(MemoryBonusPoolScoreStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolScoreStore(inner, BonusPoolScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolScore> FindBonusPoolScores(IBonusPoolScoreStore store, BonusPoolScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
