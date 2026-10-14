namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict succession plan scores by code.</summary>
public interface IStrictSuccessionPlanScoreStore
{
    StrictSuccessionPlanScore? Find(string code);
    void Save(StrictSuccessionPlanScore item);
    bool Remove(string code);
    List<StrictSuccessionPlanScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSuccessionPlanScoreStore(Dictionary<string, StrictSuccessionPlanScore> items, List<string> order) : IStrictSuccessionPlanScoreStore
{
    public Dictionary<string, StrictSuccessionPlanScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSuccessionPlanScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSuccessionPlanScore item)
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

    public List<StrictSuccessionPlanScore> List()
    {
        var outValue = new List<StrictSuccessionPlanScore>();
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
public sealed class CachedStrictSuccessionPlanScoreStore(MemoryStrictSuccessionPlanScoreStore inner, StrictSuccessionPlanScoreCache cache, int hits, int misses) : IStrictSuccessionPlanScoreStore
{
    public MemoryStrictSuccessionPlanScoreStore Inner { get; set; } = inner;
    public StrictSuccessionPlanScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSuccessionPlanScore? Find(string code)
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

    public void Save(StrictSuccessionPlanScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSuccessionPlanScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict succession plan scores. Empty fields match all.</summary>
public sealed record StrictSuccessionPlanScoreQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSuccessionPlanScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictSuccessionPlanScore> Apply(List<StrictSuccessionPlanScore> items)
    {
        var matched = Collections.Filter(items, (StrictSuccessionPlanScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict succession plan score store.</summary>
public static class StrictSuccessionPlanScoreStore
{
    public static IStrictSuccessionPlanScoreStore NewMemoryStrictSuccessionPlanScoreStore()
    {
        return new MemoryStrictSuccessionPlanScoreStore(
            new Dictionary<string, StrictSuccessionPlanScore>(),
            new List<string>());
    }

    public static IStrictSuccessionPlanScoreStore NewCachedStrictSuccessionPlanScoreStore(MemoryStrictSuccessionPlanScoreStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSuccessionPlanScoreStore(
            inner,
            StrictSuccessionPlanScoreCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictSuccessionPlanScore> FindStrictSuccessionPlanScores(IStrictSuccessionPlanScoreStore store, StrictSuccessionPlanScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
