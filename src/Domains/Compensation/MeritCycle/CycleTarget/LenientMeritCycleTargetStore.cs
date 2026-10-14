namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient merit cycle targets by code.</summary>
public interface ILenientMeritCycleTargetStore
{
    LenientMeritCycleTarget? Find(string code);
    void Save(LenientMeritCycleTarget item);
    bool Remove(string code);
    List<LenientMeritCycleTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientMeritCycleTargetStore(Dictionary<string, LenientMeritCycleTarget> items, List<string> order) : ILenientMeritCycleTargetStore
{
    public Dictionary<string, LenientMeritCycleTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientMeritCycleTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientMeritCycleTarget item)
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

    public List<LenientMeritCycleTarget> List()
    {
        var outValue = new List<LenientMeritCycleTarget>();
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
public sealed class CachedLenientMeritCycleTargetStore(MemoryLenientMeritCycleTargetStore inner, LenientMeritCycleTargetCache cache, int hits, int misses) : ILenientMeritCycleTargetStore
{
    public MemoryLenientMeritCycleTargetStore Inner { get; set; } = inner;
    public LenientMeritCycleTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientMeritCycleTarget? Find(string code)
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

    public void Save(LenientMeritCycleTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientMeritCycleTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient merit cycle targets. Empty fields match all.</summary>
public sealed record LenientMeritCycleTargetQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LenientMeritCycleTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientMeritCycleTarget> Apply(List<LenientMeritCycleTarget> items)
    {
        var matched = Collections.Filter(items, (LenientMeritCycleTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient merit cycle target store.</summary>
public static class LenientMeritCycleTargetStore
{
    public static ILenientMeritCycleTargetStore NewMemoryLenientMeritCycleTargetStore()
    {
        return new MemoryLenientMeritCycleTargetStore(
            new Dictionary<string, LenientMeritCycleTarget>(),
            new List<string>());
    }

    public static ILenientMeritCycleTargetStore NewCachedLenientMeritCycleTargetStore(MemoryLenientMeritCycleTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLenientMeritCycleTargetStore(
            inner,
            LenientMeritCycleTargetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientMeritCycleTarget> FindLenientMeritCycleTargets(ILenientMeritCycleTargetStore store, LenientMeritCycleTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
