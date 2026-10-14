namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding cycles by code.</summary>
public interface IOffboardingCycleStore
{
    OffboardingCycle? Find(string code);
    void Save(OffboardingCycle item);
    bool Remove(string code);
    List<OffboardingCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingCycleStore(Dictionary<string, OffboardingCycle> items, List<string> order) : IOffboardingCycleStore
{
    public Dictionary<string, OffboardingCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingCycle item)
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

    public List<OffboardingCycle> List()
    {
        var outValue = new List<OffboardingCycle>();
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
public sealed class CachedOffboardingCycleStore(MemoryOffboardingCycleStore inner, OffboardingCycleCache cache, int hits, int misses) : IOffboardingCycleStore
{
    public MemoryOffboardingCycleStore Inner { get; set; } = inner;
    public OffboardingCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingCycle? Find(string code)
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

    public void Save(OffboardingCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding cycles. Empty fields match all.</summary>
public sealed record OffboardingCycleQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingCycle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<OffboardingCycle> Apply(List<OffboardingCycle> items)
    {
        var matched = Collections.Filter(items, (OffboardingCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding cycle store.</summary>
public static class OffboardingCycleStore
{
    public static IOffboardingCycleStore NewMemoryOffboardingCycleStore()
    {
        return new MemoryOffboardingCycleStore(new Dictionary<string, OffboardingCycle>(), new List<string>());
    }

    public static IOffboardingCycleStore NewCachedOffboardingCycleStore(MemoryOffboardingCycleStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingCycleStore(inner, OffboardingCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OffboardingCycle> FindOffboardingCycles(IOffboardingCycleStore store, OffboardingCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
