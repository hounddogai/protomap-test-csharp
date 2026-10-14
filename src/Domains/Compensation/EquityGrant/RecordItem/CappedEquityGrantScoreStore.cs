namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped equity grant scores by code.</summary>
public interface ICappedEquityGrantScoreStore
{
    CappedEquityGrantScore? Find(string code);
    void Save(CappedEquityGrantScore item);
    bool Remove(string code);
    List<CappedEquityGrantScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedEquityGrantScoreStore(Dictionary<string, CappedEquityGrantScore> items, List<string> order) : ICappedEquityGrantScoreStore
{
    public Dictionary<string, CappedEquityGrantScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedEquityGrantScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedEquityGrantScore item)
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

    public List<CappedEquityGrantScore> List()
    {
        var outValue = new List<CappedEquityGrantScore>();
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
public sealed class CachedCappedEquityGrantScoreStore(MemoryCappedEquityGrantScoreStore inner, CappedEquityGrantScoreCache cache, int hits, int misses) : ICappedEquityGrantScoreStore
{
    public MemoryCappedEquityGrantScoreStore Inner { get; set; } = inner;
    public CappedEquityGrantScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedEquityGrantScore? Find(string code)
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

    public void Save(CappedEquityGrantScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedEquityGrantScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped equity grant scores. Empty fields match all.</summary>
public sealed record CappedEquityGrantScoreQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedEquityGrantScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedEquityGrantScore> Apply(List<CappedEquityGrantScore> items)
    {
        var matched = Collections.Filter(items, (CappedEquityGrantScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped equity grant score store.</summary>
public static class CappedEquityGrantScoreStore
{
    public static ICappedEquityGrantScoreStore NewMemoryCappedEquityGrantScoreStore()
    {
        return new MemoryCappedEquityGrantScoreStore(
            new Dictionary<string, CappedEquityGrantScore>(),
            new List<string>());
    }

    public static ICappedEquityGrantScoreStore NewCachedCappedEquityGrantScoreStore(MemoryCappedEquityGrantScoreStore inner, int ttl, IClock clock)
    {
        return new CachedCappedEquityGrantScoreStore(
            inner,
            CappedEquityGrantScoreCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedEquityGrantScore> FindCappedEquityGrantScores(ICappedEquityGrantScoreStore store, CappedEquityGrantScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
