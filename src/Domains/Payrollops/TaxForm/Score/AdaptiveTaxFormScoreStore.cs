namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive tax form scores by code.</summary>
public interface IAdaptiveTaxFormScoreStore
{
    AdaptiveTaxFormScore? Find(string code);
    void Save(AdaptiveTaxFormScore item);
    bool Remove(string code);
    List<AdaptiveTaxFormScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTaxFormScoreStore(Dictionary<string, AdaptiveTaxFormScore> items, List<string> order) : IAdaptiveTaxFormScoreStore
{
    public Dictionary<string, AdaptiveTaxFormScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTaxFormScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTaxFormScore item)
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

    public List<AdaptiveTaxFormScore> List()
    {
        var outValue = new List<AdaptiveTaxFormScore>();
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
public sealed class CachedAdaptiveTaxFormScoreStore(MemoryAdaptiveTaxFormScoreStore inner, AdaptiveTaxFormScoreCache cache, int hits, int misses) : IAdaptiveTaxFormScoreStore
{
    public MemoryAdaptiveTaxFormScoreStore Inner { get; set; } = inner;
    public AdaptiveTaxFormScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTaxFormScore? Find(string code)
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

    public void Save(AdaptiveTaxFormScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTaxFormScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive tax form scores. Empty fields match all.</summary>
public sealed record AdaptiveTaxFormScoreQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTaxFormScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTaxFormScore> Apply(List<AdaptiveTaxFormScore> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTaxFormScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive tax form score store.</summary>
public static class AdaptiveTaxFormScoreStore
{
    public static IAdaptiveTaxFormScoreStore NewMemoryAdaptiveTaxFormScoreStore()
    {
        return new MemoryAdaptiveTaxFormScoreStore(
            new Dictionary<string, AdaptiveTaxFormScore>(),
            new List<string>());
    }

    public static IAdaptiveTaxFormScoreStore NewCachedAdaptiveTaxFormScoreStore(MemoryAdaptiveTaxFormScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTaxFormScoreStore(inner, AdaptiveTaxFormScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdaptiveTaxFormScore> FindAdaptiveTaxFormScores(IAdaptiveTaxFormScoreStore store, AdaptiveTaxFormScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
