namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default bonus pool cycles by code.</summary>
public interface IDefaultBonusPoolCycleStore
{
    DefaultBonusPoolCycle? Find(string code);
    void Save(DefaultBonusPoolCycle item);
    bool Remove(string code);
    List<DefaultBonusPoolCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBonusPoolCycleStore(Dictionary<string, DefaultBonusPoolCycle> items, List<string> order) : IDefaultBonusPoolCycleStore
{
    public Dictionary<string, DefaultBonusPoolCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBonusPoolCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBonusPoolCycle item)
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

    public List<DefaultBonusPoolCycle> List()
    {
        var outValue = new List<DefaultBonusPoolCycle>();
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
public sealed class CachedDefaultBonusPoolCycleStore(MemoryDefaultBonusPoolCycleStore inner, DefaultBonusPoolCycleCache cache, int hits, int misses) : IDefaultBonusPoolCycleStore
{
    public MemoryDefaultBonusPoolCycleStore Inner { get; set; } = inner;
    public DefaultBonusPoolCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBonusPoolCycle? Find(string code)
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

    public void Save(DefaultBonusPoolCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBonusPoolCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default bonus pool cycles. Empty fields match all.</summary>
public sealed record DefaultBonusPoolCycleQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBonusPoolCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBonusPoolCycle> Apply(List<DefaultBonusPoolCycle> items)
    {
        var matched = Collections.Filter(items, (DefaultBonusPoolCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default bonus pool cycle store.</summary>
public static class DefaultBonusPoolCycleStore
{
    public static IDefaultBonusPoolCycleStore NewMemoryDefaultBonusPoolCycleStore()
    {
        return new MemoryDefaultBonusPoolCycleStore(
            new Dictionary<string, DefaultBonusPoolCycle>(),
            new List<string>());
    }

    public static IDefaultBonusPoolCycleStore NewCachedDefaultBonusPoolCycleStore(MemoryDefaultBonusPoolCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBonusPoolCycleStore(inner, DefaultBonusPoolCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultBonusPoolCycle> FindDefaultBonusPoolCycles(IDefaultBonusPoolCycleStore store, DefaultBonusPoolCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
