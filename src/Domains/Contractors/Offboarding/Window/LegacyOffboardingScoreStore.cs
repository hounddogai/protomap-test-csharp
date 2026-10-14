namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy offboarding scores by code.</summary>
public interface ILegacyOffboardingScoreStore
{
    LegacyOffboardingScore? Find(string code);
    void Save(LegacyOffboardingScore item);
    bool Remove(string code);
    List<LegacyOffboardingScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOffboardingScoreStore(Dictionary<string, LegacyOffboardingScore> items, List<string> order) : ILegacyOffboardingScoreStore
{
    public Dictionary<string, LegacyOffboardingScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOffboardingScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOffboardingScore item)
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

    public List<LegacyOffboardingScore> List()
    {
        var outValue = new List<LegacyOffboardingScore>();
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
public sealed class CachedLegacyOffboardingScoreStore(MemoryLegacyOffboardingScoreStore inner, LegacyOffboardingScoreCache cache, int hits, int misses) : ILegacyOffboardingScoreStore
{
    public MemoryLegacyOffboardingScoreStore Inner { get; set; } = inner;
    public LegacyOffboardingScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOffboardingScore? Find(string code)
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

    public void Save(LegacyOffboardingScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOffboardingScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy offboarding scores. Empty fields match all.</summary>
public sealed record LegacyOffboardingScoreQuery(string Country, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOffboardingScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyOffboardingScore> Apply(List<LegacyOffboardingScore> items)
    {
        var matched = Collections.Filter(items, (LegacyOffboardingScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy offboarding score store.</summary>
public static class LegacyOffboardingScoreStore
{
    public static ILegacyOffboardingScoreStore NewMemoryLegacyOffboardingScoreStore()
    {
        return new MemoryLegacyOffboardingScoreStore(
            new Dictionary<string, LegacyOffboardingScore>(),
            new List<string>());
    }

    public static ILegacyOffboardingScoreStore NewCachedLegacyOffboardingScoreStore(MemoryLegacyOffboardingScoreStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOffboardingScoreStore(
            inner,
            LegacyOffboardingScoreCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyOffboardingScore> FindLegacyOffboardingScores(ILegacyOffboardingScoreStore store, LegacyOffboardingScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
