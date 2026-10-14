namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle targets by code.</summary>
public interface IMeritCycleTargetStore
{
    MeritCycleTarget? Find(string code);
    void Save(MeritCycleTarget item);
    bool Remove(string code);
    List<MeritCycleTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleTargetStore(Dictionary<string, MeritCycleTarget> items, List<string> order) : IMeritCycleTargetStore
{
    public Dictionary<string, MeritCycleTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleTarget item)
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

    public List<MeritCycleTarget> List()
    {
        var outValue = new List<MeritCycleTarget>();
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
public sealed class CachedMeritCycleTargetStore(MemoryMeritCycleTargetStore inner, MeritCycleTargetCache cache, int hits, int misses) : IMeritCycleTargetStore
{
    public MemoryMeritCycleTargetStore Inner { get; set; } = inner;
    public MeritCycleTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleTarget? Find(string code)
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

    public void Save(MeritCycleTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle targets. Empty fields match all.</summary>
public sealed record MeritCycleTargetQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleTarget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleTarget> Apply(List<MeritCycleTarget> items)
    {
        var matched = Collections.Filter(items, (MeritCycleTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle target store.</summary>
public static class MeritCycleTargetStore
{
    public static IMeritCycleTargetStore NewMemoryMeritCycleTargetStore()
    {
        return new MemoryMeritCycleTargetStore(new Dictionary<string, MeritCycleTarget>(), new List<string>());
    }

    public static IMeritCycleTargetStore NewCachedMeritCycleTargetStore(MemoryMeritCycleTargetStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleTargetStore(inner, MeritCycleTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleTarget> FindMeritCycleTargets(IMeritCycleTargetStore store, MeritCycleTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
