namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pay run cycles by code.</summary>
public interface IDefaultPayRunCycleStore
{
    DefaultPayRunCycle? Find(string code);
    void Save(DefaultPayRunCycle item);
    bool Remove(string code);
    List<DefaultPayRunCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayRunCycleStore(Dictionary<string, DefaultPayRunCycle> items, List<string> order) : IDefaultPayRunCycleStore
{
    public Dictionary<string, DefaultPayRunCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayRunCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayRunCycle item)
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

    public List<DefaultPayRunCycle> List()
    {
        var outValue = new List<DefaultPayRunCycle>();
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
public sealed class CachedDefaultPayRunCycleStore(MemoryDefaultPayRunCycleStore inner, DefaultPayRunCycleCache cache, int hits, int misses) : IDefaultPayRunCycleStore
{
    public MemoryDefaultPayRunCycleStore Inner { get; set; } = inner;
    public DefaultPayRunCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayRunCycle? Find(string code)
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

    public void Save(DefaultPayRunCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayRunCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pay run cycles. Empty fields match all.</summary>
public sealed record DefaultPayRunCycleQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayRunCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DefaultPayRunCycle> Apply(List<DefaultPayRunCycle> items)
    {
        var matched = Collections.Filter(items, (DefaultPayRunCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pay run cycle store.</summary>
public static class DefaultPayRunCycleStore
{
    public static IDefaultPayRunCycleStore NewMemoryDefaultPayRunCycleStore()
    {
        return new MemoryDefaultPayRunCycleStore(new Dictionary<string, DefaultPayRunCycle>(), new List<string>());
    }

    public static IDefaultPayRunCycleStore NewCachedDefaultPayRunCycleStore(MemoryDefaultPayRunCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayRunCycleStore(inner, DefaultPayRunCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultPayRunCycle> FindDefaultPayRunCycles(IDefaultPayRunCycleStore store, DefaultPayRunCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
