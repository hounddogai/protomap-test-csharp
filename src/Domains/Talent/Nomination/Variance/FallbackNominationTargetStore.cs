namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback nomination targets by code.</summary>
public interface IFallbackNominationTargetStore
{
    FallbackNominationTarget? Find(string code);
    void Save(FallbackNominationTarget item);
    bool Remove(string code);
    List<FallbackNominationTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackNominationTargetStore(Dictionary<string, FallbackNominationTarget> items, List<string> order) : IFallbackNominationTargetStore
{
    public Dictionary<string, FallbackNominationTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackNominationTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackNominationTarget item)
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

    public List<FallbackNominationTarget> List()
    {
        var outValue = new List<FallbackNominationTarget>();
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
public sealed class CachedFallbackNominationTargetStore(MemoryFallbackNominationTargetStore inner, FallbackNominationTargetCache cache, int hits, int misses) : IFallbackNominationTargetStore
{
    public MemoryFallbackNominationTargetStore Inner { get; set; } = inner;
    public FallbackNominationTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackNominationTarget? Find(string code)
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

    public void Save(FallbackNominationTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackNominationTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback nomination targets. Empty fields match all.</summary>
public sealed record FallbackNominationTargetQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackNominationTarget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<FallbackNominationTarget> Apply(List<FallbackNominationTarget> items)
    {
        var matched = Collections.Filter(items, (FallbackNominationTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback nomination target store.</summary>
public static class FallbackNominationTargetStore
{
    public static IFallbackNominationTargetStore NewMemoryFallbackNominationTargetStore()
    {
        return new MemoryFallbackNominationTargetStore(
            new Dictionary<string, FallbackNominationTarget>(),
            new List<string>());
    }

    public static IFallbackNominationTargetStore NewCachedFallbackNominationTargetStore(MemoryFallbackNominationTargetStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackNominationTargetStore(
            inner,
            FallbackNominationTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackNominationTarget> FindFallbackNominationTargets(IFallbackNominationTargetStore store, FallbackNominationTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
