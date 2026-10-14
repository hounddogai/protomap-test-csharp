namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive visitor cycles by code.</summary>
public interface IAdaptiveVisitorCycleStore
{
    AdaptiveVisitorCycle? Find(string code);
    void Save(AdaptiveVisitorCycle item);
    bool Remove(string code);
    List<AdaptiveVisitorCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveVisitorCycleStore(Dictionary<string, AdaptiveVisitorCycle> items, List<string> order) : IAdaptiveVisitorCycleStore
{
    public Dictionary<string, AdaptiveVisitorCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveVisitorCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveVisitorCycle item)
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

    public List<AdaptiveVisitorCycle> List()
    {
        var outValue = new List<AdaptiveVisitorCycle>();
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
public sealed class CachedAdaptiveVisitorCycleStore(MemoryAdaptiveVisitorCycleStore inner, AdaptiveVisitorCycleCache cache, int hits, int misses) : IAdaptiveVisitorCycleStore
{
    public MemoryAdaptiveVisitorCycleStore Inner { get; set; } = inner;
    public AdaptiveVisitorCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveVisitorCycle? Find(string code)
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

    public void Save(AdaptiveVisitorCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveVisitorCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive visitor cycles. Empty fields match all.</summary>
public sealed record AdaptiveVisitorCycleQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveVisitorCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveVisitorCycle> Apply(List<AdaptiveVisitorCycle> items)
    {
        var matched = Collections.Filter(items, (AdaptiveVisitorCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive visitor cycle store.</summary>
public static class AdaptiveVisitorCycleStore
{
    public static IAdaptiveVisitorCycleStore NewMemoryAdaptiveVisitorCycleStore()
    {
        return new MemoryAdaptiveVisitorCycleStore(
            new Dictionary<string, AdaptiveVisitorCycle>(),
            new List<string>());
    }

    public static IAdaptiveVisitorCycleStore NewCachedAdaptiveVisitorCycleStore(MemoryAdaptiveVisitorCycleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveVisitorCycleStore(inner, AdaptiveVisitorCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptiveVisitorCycle> FindAdaptiveVisitorCycles(IAdaptiveVisitorCycleStore store, AdaptiveVisitorCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
