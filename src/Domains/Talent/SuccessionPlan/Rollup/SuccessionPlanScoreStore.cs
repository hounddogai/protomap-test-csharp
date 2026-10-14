namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan scores by code.</summary>
public interface ISuccessionPlanScoreStore
{
    SuccessionPlanScore? Find(string code);
    void Save(SuccessionPlanScore item);
    bool Remove(string code);
    List<SuccessionPlanScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanScoreStore(Dictionary<string, SuccessionPlanScore> items, List<string> order) : ISuccessionPlanScoreStore
{
    public Dictionary<string, SuccessionPlanScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanScore item)
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

    public List<SuccessionPlanScore> List()
    {
        var outValue = new List<SuccessionPlanScore>();
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
public sealed class CachedSuccessionPlanScoreStore(MemorySuccessionPlanScoreStore inner, SuccessionPlanScoreCache cache, int hits, int misses) : ISuccessionPlanScoreStore
{
    public MemorySuccessionPlanScoreStore Inner { get; set; } = inner;
    public SuccessionPlanScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanScore? Find(string code)
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

    public void Save(SuccessionPlanScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan scores. Empty fields match all.</summary>
public sealed record SuccessionPlanScoreQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanScore> Apply(List<SuccessionPlanScore> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan score store.</summary>
public static class SuccessionPlanScoreStore
{
    public static ISuccessionPlanScoreStore NewMemorySuccessionPlanScoreStore()
    {
        return new MemorySuccessionPlanScoreStore(new Dictionary<string, SuccessionPlanScore>(), new List<string>());
    }

    public static ISuccessionPlanScoreStore NewCachedSuccessionPlanScoreStore(MemorySuccessionPlanScoreStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanScoreStore(inner, SuccessionPlanScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanScore> FindSuccessionPlanScores(ISuccessionPlanScoreStore store, SuccessionPlanScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
