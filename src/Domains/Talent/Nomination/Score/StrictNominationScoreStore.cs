namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict nomination scores by code.</summary>
public interface IStrictNominationScoreStore
{
    StrictNominationScore? Find(string code);
    void Save(StrictNominationScore item);
    bool Remove(string code);
    List<StrictNominationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictNominationScoreStore(Dictionary<string, StrictNominationScore> items, List<string> order) : IStrictNominationScoreStore
{
    public Dictionary<string, StrictNominationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictNominationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictNominationScore item)
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

    public List<StrictNominationScore> List()
    {
        var outValue = new List<StrictNominationScore>();
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
public sealed class CachedStrictNominationScoreStore(MemoryStrictNominationScoreStore inner, StrictNominationScoreCache cache, int hits, int misses) : IStrictNominationScoreStore
{
    public MemoryStrictNominationScoreStore Inner { get; set; } = inner;
    public StrictNominationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictNominationScore? Find(string code)
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

    public void Save(StrictNominationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictNominationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict nomination scores. Empty fields match all.</summary>
public sealed record StrictNominationScoreQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictNominationScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictNominationScore> Apply(List<StrictNominationScore> items)
    {
        var matched = Collections.Filter(items, (StrictNominationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict nomination score store.</summary>
public static class StrictNominationScoreStore
{
    public static IStrictNominationScoreStore NewMemoryStrictNominationScoreStore()
    {
        return new MemoryStrictNominationScoreStore(
            new Dictionary<string, StrictNominationScore>(),
            new List<string>());
    }

    public static IStrictNominationScoreStore NewCachedStrictNominationScoreStore(MemoryStrictNominationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedStrictNominationScoreStore(
            inner,
            StrictNominationScoreCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictNominationScore> FindStrictNominationScores(IStrictNominationScoreStore store, StrictNominationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
