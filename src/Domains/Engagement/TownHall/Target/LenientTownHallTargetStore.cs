namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient town hall targets by code.</summary>
public interface ILenientTownHallTargetStore
{
    LenientTownHallTarget? Find(string code);
    void Save(LenientTownHallTarget item);
    bool Remove(string code);
    List<LenientTownHallTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTownHallTargetStore(Dictionary<string, LenientTownHallTarget> items, List<string> order) : ILenientTownHallTargetStore
{
    public Dictionary<string, LenientTownHallTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTownHallTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTownHallTarget item)
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

    public List<LenientTownHallTarget> List()
    {
        var outValue = new List<LenientTownHallTarget>();
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
public sealed class CachedLenientTownHallTargetStore(MemoryLenientTownHallTargetStore inner, LenientTownHallTargetCache cache, int hits, int misses) : ILenientTownHallTargetStore
{
    public MemoryLenientTownHallTargetStore Inner { get; set; } = inner;
    public LenientTownHallTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTownHallTarget? Find(string code)
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

    public void Save(LenientTownHallTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTownHallTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient town hall targets. Empty fields match all.</summary>
public sealed record LenientTownHallTargetQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTownHallTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTownHallTarget> Apply(List<LenientTownHallTarget> items)
    {
        var matched = Collections.Filter(items, (LenientTownHallTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient town hall target store.</summary>
public static class LenientTownHallTargetStore
{
    public static ILenientTownHallTargetStore NewMemoryLenientTownHallTargetStore()
    {
        return new MemoryLenientTownHallTargetStore(
            new Dictionary<string, LenientTownHallTarget>(),
            new List<string>());
    }

    public static ILenientTownHallTargetStore NewCachedLenientTownHallTargetStore(MemoryLenientTownHallTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTownHallTargetStore(inner, LenientTownHallTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientTownHallTarget> FindLenientTownHallTargets(ILenientTownHallTargetStore store, LenientTownHallTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
