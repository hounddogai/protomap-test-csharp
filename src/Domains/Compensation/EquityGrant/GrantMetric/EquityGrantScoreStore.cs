namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant scores by code.</summary>
public interface IEquityGrantScoreStore
{
    EquityGrantScore? Find(string code);
    void Save(EquityGrantScore item);
    bool Remove(string code);
    List<EquityGrantScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantScoreStore(Dictionary<string, EquityGrantScore> items, List<string> order) : IEquityGrantScoreStore
{
    public Dictionary<string, EquityGrantScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantScore item)
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

    public List<EquityGrantScore> List()
    {
        var outValue = new List<EquityGrantScore>();
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
public sealed class CachedEquityGrantScoreStore(MemoryEquityGrantScoreStore inner, EquityGrantScoreCache cache, int hits, int misses) : IEquityGrantScoreStore
{
    public MemoryEquityGrantScoreStore Inner { get; set; } = inner;
    public EquityGrantScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantScore? Find(string code)
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

    public void Save(EquityGrantScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant scores. Empty fields match all.</summary>
public sealed record EquityGrantScoreQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantScore> Apply(List<EquityGrantScore> items)
    {
        var matched = Collections.Filter(items, (EquityGrantScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant score store.</summary>
public static class EquityGrantScoreStore
{
    public static IEquityGrantScoreStore NewMemoryEquityGrantScoreStore()
    {
        return new MemoryEquityGrantScoreStore(new Dictionary<string, EquityGrantScore>(), new List<string>());
    }

    public static IEquityGrantScoreStore NewCachedEquityGrantScoreStore(MemoryEquityGrantScoreStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantScoreStore(inner, EquityGrantScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<EquityGrantScore> FindEquityGrantScores(IEquityGrantScoreStore store, EquityGrantScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
