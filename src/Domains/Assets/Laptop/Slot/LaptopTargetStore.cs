namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop targets by code.</summary>
public interface ILaptopTargetStore
{
    LaptopTarget? Find(string code);
    void Save(LaptopTarget item);
    bool Remove(string code);
    List<LaptopTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopTargetStore(Dictionary<string, LaptopTarget> items, List<string> order) : ILaptopTargetStore
{
    public Dictionary<string, LaptopTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopTarget item)
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

    public List<LaptopTarget> List()
    {
        var outValue = new List<LaptopTarget>();
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
public sealed class CachedLaptopTargetStore(MemoryLaptopTargetStore inner, LaptopTargetCache cache, int hits, int misses) : ILaptopTargetStore
{
    public MemoryLaptopTargetStore Inner { get; set; } = inner;
    public LaptopTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopTarget? Find(string code)
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

    public void Save(LaptopTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop targets. Empty fields match all.</summary>
public sealed record LaptopTargetQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopTarget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopTarget> Apply(List<LaptopTarget> items)
    {
        var matched = Collections.Filter(items, (LaptopTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop target store.</summary>
public static class LaptopTargetStore
{
    public static ILaptopTargetStore NewMemoryLaptopTargetStore()
    {
        return new MemoryLaptopTargetStore(new Dictionary<string, LaptopTarget>(), new List<string>());
    }

    public static ILaptopTargetStore NewCachedLaptopTargetStore(MemoryLaptopTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopTargetStore(inner, LaptopTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopTarget> FindLaptopTargets(ILaptopTargetStore store, LaptopTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
