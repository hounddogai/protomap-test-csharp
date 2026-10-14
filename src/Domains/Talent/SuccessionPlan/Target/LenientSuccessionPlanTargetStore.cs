namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient succession plan targets by code.</summary>
public interface ILenientSuccessionPlanTargetStore
{
    LenientSuccessionPlanTarget? Find(string code);
    void Save(LenientSuccessionPlanTarget item);
    bool Remove(string code);
    List<LenientSuccessionPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSuccessionPlanTargetStore(Dictionary<string, LenientSuccessionPlanTarget> items, List<string> order) : ILenientSuccessionPlanTargetStore
{
    public Dictionary<string, LenientSuccessionPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSuccessionPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSuccessionPlanTarget item)
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

    public List<LenientSuccessionPlanTarget> List()
    {
        var outValue = new List<LenientSuccessionPlanTarget>();
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
public sealed class CachedLenientSuccessionPlanTargetStore(MemoryLenientSuccessionPlanTargetStore inner, LenientSuccessionPlanTargetCache cache, int hits, int misses) : ILenientSuccessionPlanTargetStore
{
    public MemoryLenientSuccessionPlanTargetStore Inner { get; set; } = inner;
    public LenientSuccessionPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSuccessionPlanTarget? Find(string code)
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

    public void Save(LenientSuccessionPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSuccessionPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient succession plan targets. Empty fields match all.</summary>
public sealed record LenientSuccessionPlanTargetQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSuccessionPlanTarget item)
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

    public List<LenientSuccessionPlanTarget> Apply(List<LenientSuccessionPlanTarget> items)
    {
        var matched = Collections.Filter(items, (LenientSuccessionPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient succession plan target store.</summary>
public static class LenientSuccessionPlanTargetStore
{
    public static ILenientSuccessionPlanTargetStore NewMemoryLenientSuccessionPlanTargetStore()
    {
        return new MemoryLenientSuccessionPlanTargetStore(
            new Dictionary<string, LenientSuccessionPlanTarget>(),
            new List<string>());
    }

    public static ILenientSuccessionPlanTargetStore NewCachedLenientSuccessionPlanTargetStore(MemoryLenientSuccessionPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSuccessionPlanTargetStore(
            inner,
            LenientSuccessionPlanTargetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientSuccessionPlanTarget> FindLenientSuccessionPlanTargets(ILenientSuccessionPlanTargetStore store, LenientSuccessionPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
