namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted refresh scores by code.</summary>
public interface IWeightedRefreshScoreStore
{
    WeightedRefreshScore? Find(string code);
    void Save(WeightedRefreshScore item);
    bool Remove(string code);
    List<WeightedRefreshScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRefreshScoreStore(Dictionary<string, WeightedRefreshScore> items, List<string> order) : IWeightedRefreshScoreStore
{
    public Dictionary<string, WeightedRefreshScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRefreshScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRefreshScore item)
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

    public List<WeightedRefreshScore> List()
    {
        var outValue = new List<WeightedRefreshScore>();
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
public sealed class CachedWeightedRefreshScoreStore(MemoryWeightedRefreshScoreStore inner, WeightedRefreshScoreCache cache, int hits, int misses) : IWeightedRefreshScoreStore
{
    public MemoryWeightedRefreshScoreStore Inner { get; set; } = inner;
    public WeightedRefreshScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRefreshScore? Find(string code)
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

    public void Save(WeightedRefreshScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRefreshScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted refresh scores. Empty fields match all.</summary>
public sealed record WeightedRefreshScoreQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRefreshScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<WeightedRefreshScore> Apply(List<WeightedRefreshScore> items)
    {
        var matched = Collections.Filter(items, (WeightedRefreshScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted refresh score store.</summary>
public static class WeightedRefreshScoreStore
{
    public static IWeightedRefreshScoreStore NewMemoryWeightedRefreshScoreStore()
    {
        return new MemoryWeightedRefreshScoreStore(
            new Dictionary<string, WeightedRefreshScore>(),
            new List<string>());
    }

    public static IWeightedRefreshScoreStore NewCachedWeightedRefreshScoreStore(MemoryWeightedRefreshScoreStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRefreshScoreStore(inner, WeightedRefreshScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WeightedRefreshScore> FindWeightedRefreshScores(IWeightedRefreshScoreStore store, WeightedRefreshScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
