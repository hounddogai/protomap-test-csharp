namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling career path cycles by code.</summary>
public interface IRollingCareerPathCycleStore
{
    RollingCareerPathCycle? Find(string code);
    void Save(RollingCareerPathCycle item);
    bool Remove(string code);
    List<RollingCareerPathCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingCareerPathCycleStore(Dictionary<string, RollingCareerPathCycle> items, List<string> order) : IRollingCareerPathCycleStore
{
    public Dictionary<string, RollingCareerPathCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingCareerPathCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingCareerPathCycle item)
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

    public List<RollingCareerPathCycle> List()
    {
        var outValue = new List<RollingCareerPathCycle>();
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
public sealed class CachedRollingCareerPathCycleStore(MemoryRollingCareerPathCycleStore inner, RollingCareerPathCycleCache cache, int hits, int misses) : IRollingCareerPathCycleStore
{
    public MemoryRollingCareerPathCycleStore Inner { get; set; } = inner;
    public RollingCareerPathCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingCareerPathCycle? Find(string code)
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

    public void Save(RollingCareerPathCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingCareerPathCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling career path cycles. Empty fields match all.</summary>
public sealed record RollingCareerPathCycleQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingCareerPathCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingCareerPathCycle> Apply(List<RollingCareerPathCycle> items)
    {
        var matched = Collections.Filter(items, (RollingCareerPathCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling career path cycle store.</summary>
public static class RollingCareerPathCycleStore
{
    public static IRollingCareerPathCycleStore NewMemoryRollingCareerPathCycleStore()
    {
        return new MemoryRollingCareerPathCycleStore(
            new Dictionary<string, RollingCareerPathCycle>(),
            new List<string>());
    }

    public static IRollingCareerPathCycleStore NewCachedRollingCareerPathCycleStore(MemoryRollingCareerPathCycleStore inner, int ttl, IClock clock)
    {
        return new CachedRollingCareerPathCycleStore(
            inner,
            RollingCareerPathCycleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingCareerPathCycle> FindRollingCareerPathCycles(IRollingCareerPathCycleStore store, RollingCareerPathCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
