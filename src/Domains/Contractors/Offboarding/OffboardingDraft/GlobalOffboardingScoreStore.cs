namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global offboarding scores by code.</summary>
public interface IGlobalOffboardingScoreStore
{
    GlobalOffboardingScore? Find(string code);
    void Save(GlobalOffboardingScore item);
    bool Remove(string code);
    List<GlobalOffboardingScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalOffboardingScoreStore(Dictionary<string, GlobalOffboardingScore> items, List<string> order) : IGlobalOffboardingScoreStore
{
    public Dictionary<string, GlobalOffboardingScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalOffboardingScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalOffboardingScore item)
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

    public List<GlobalOffboardingScore> List()
    {
        var outValue = new List<GlobalOffboardingScore>();
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
public sealed class CachedGlobalOffboardingScoreStore(MemoryGlobalOffboardingScoreStore inner, GlobalOffboardingScoreCache cache, int hits, int misses) : IGlobalOffboardingScoreStore
{
    public MemoryGlobalOffboardingScoreStore Inner { get; set; } = inner;
    public GlobalOffboardingScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalOffboardingScore? Find(string code)
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

    public void Save(GlobalOffboardingScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalOffboardingScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global offboarding scores. Empty fields match all.</summary>
public sealed record GlobalOffboardingScoreQuery(string Department, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalOffboardingScore item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<GlobalOffboardingScore> Apply(List<GlobalOffboardingScore> items)
    {
        var matched = Collections.Filter(items, (GlobalOffboardingScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global offboarding score store.</summary>
public static class GlobalOffboardingScoreStore
{
    public static IGlobalOffboardingScoreStore NewMemoryGlobalOffboardingScoreStore()
    {
        return new MemoryGlobalOffboardingScoreStore(
            new Dictionary<string, GlobalOffboardingScore>(),
            new List<string>());
    }

    public static IGlobalOffboardingScoreStore NewCachedGlobalOffboardingScoreStore(MemoryGlobalOffboardingScoreStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalOffboardingScoreStore(
            inner,
            GlobalOffboardingScoreCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalOffboardingScore> FindGlobalOffboardingScores(IGlobalOffboardingScoreStore store, GlobalOffboardingScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
