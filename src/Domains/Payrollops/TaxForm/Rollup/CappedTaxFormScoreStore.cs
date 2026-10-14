namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped tax form scores by code.</summary>
public interface ICappedTaxFormScoreStore
{
    CappedTaxFormScore? Find(string code);
    void Save(CappedTaxFormScore item);
    bool Remove(string code);
    List<CappedTaxFormScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedTaxFormScoreStore(Dictionary<string, CappedTaxFormScore> items, List<string> order) : ICappedTaxFormScoreStore
{
    public Dictionary<string, CappedTaxFormScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedTaxFormScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedTaxFormScore item)
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

    public List<CappedTaxFormScore> List()
    {
        var outValue = new List<CappedTaxFormScore>();
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
public sealed class CachedCappedTaxFormScoreStore(MemoryCappedTaxFormScoreStore inner, CappedTaxFormScoreCache cache, int hits, int misses) : ICappedTaxFormScoreStore
{
    public MemoryCappedTaxFormScoreStore Inner { get; set; } = inner;
    public CappedTaxFormScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedTaxFormScore? Find(string code)
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

    public void Save(CappedTaxFormScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedTaxFormScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped tax form scores. Empty fields match all.</summary>
public sealed record CappedTaxFormScoreQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CappedTaxFormScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedTaxFormScore> Apply(List<CappedTaxFormScore> items)
    {
        var matched = Collections.Filter(items, (CappedTaxFormScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped tax form score store.</summary>
public static class CappedTaxFormScoreStore
{
    public static ICappedTaxFormScoreStore NewMemoryCappedTaxFormScoreStore()
    {
        return new MemoryCappedTaxFormScoreStore(new Dictionary<string, CappedTaxFormScore>(), new List<string>());
    }

    public static ICappedTaxFormScoreStore NewCachedCappedTaxFormScoreStore(MemoryCappedTaxFormScoreStore inner, int ttl, IClock clock)
    {
        return new CachedCappedTaxFormScoreStore(inner, CappedTaxFormScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedTaxFormScore> FindCappedTaxFormScores(ICappedTaxFormScoreStore store, CappedTaxFormScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
