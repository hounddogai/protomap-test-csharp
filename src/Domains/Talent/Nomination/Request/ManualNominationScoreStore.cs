namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual nomination scores by code.</summary>
public interface IManualNominationScoreStore
{
    ManualNominationScore? Find(string code);
    void Save(ManualNominationScore item);
    bool Remove(string code);
    List<ManualNominationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualNominationScoreStore(Dictionary<string, ManualNominationScore> items, List<string> order) : IManualNominationScoreStore
{
    public Dictionary<string, ManualNominationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualNominationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualNominationScore item)
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

    public List<ManualNominationScore> List()
    {
        var outValue = new List<ManualNominationScore>();
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
public sealed class CachedManualNominationScoreStore(MemoryManualNominationScoreStore inner, ManualNominationScoreCache cache, int hits, int misses) : IManualNominationScoreStore
{
    public MemoryManualNominationScoreStore Inner { get; set; } = inner;
    public ManualNominationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualNominationScore? Find(string code)
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

    public void Save(ManualNominationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualNominationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual nomination scores. Empty fields match all.</summary>
public sealed record ManualNominationScoreQuery(string Country, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ManualNominationScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualNominationScore> Apply(List<ManualNominationScore> items)
    {
        var matched = Collections.Filter(items, (ManualNominationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual nomination score store.</summary>
public static class ManualNominationScoreStore
{
    public static IManualNominationScoreStore NewMemoryManualNominationScoreStore()
    {
        return new MemoryManualNominationScoreStore(
            new Dictionary<string, ManualNominationScore>(),
            new List<string>());
    }

    public static IManualNominationScoreStore NewCachedManualNominationScoreStore(MemoryManualNominationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedManualNominationScoreStore(inner, ManualNominationScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualNominationScore> FindManualNominationScores(IManualNominationScoreStore store, ManualNominationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
