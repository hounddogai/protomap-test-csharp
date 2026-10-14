namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay cycles by code.</summary>
public interface IRetroPayCycleStore
{
    RetroPayCycle? Find(string code);
    void Save(RetroPayCycle item);
    bool Remove(string code);
    List<RetroPayCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayCycleStore(Dictionary<string, RetroPayCycle> items, List<string> order) : IRetroPayCycleStore
{
    public Dictionary<string, RetroPayCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayCycle item)
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

    public List<RetroPayCycle> List()
    {
        var outValue = new List<RetroPayCycle>();
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
public sealed class CachedRetroPayCycleStore(MemoryRetroPayCycleStore inner, RetroPayCycleCache cache, int hits, int misses) : IRetroPayCycleStore
{
    public MemoryRetroPayCycleStore Inner { get; set; } = inner;
    public RetroPayCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayCycle? Find(string code)
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

    public void Save(RetroPayCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay cycles. Empty fields match all.</summary>
public sealed record RetroPayCycleQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayCycle item)
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

    public List<RetroPayCycle> Apply(List<RetroPayCycle> items)
    {
        var matched = Collections.Filter(items, (RetroPayCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay cycle store.</summary>
public static class RetroPayCycleStore
{
    public static IRetroPayCycleStore NewMemoryRetroPayCycleStore()
    {
        return new MemoryRetroPayCycleStore(new Dictionary<string, RetroPayCycle>(), new List<string>());
    }

    public static IRetroPayCycleStore NewCachedRetroPayCycleStore(MemoryRetroPayCycleStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayCycleStore(inner, RetroPayCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayCycle> FindRetroPayCycles(IRetroPayCycleStore store, RetroPayCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
