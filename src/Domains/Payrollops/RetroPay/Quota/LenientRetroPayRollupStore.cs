namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient retro pay rollups by code.</summary>
public interface ILenientRetroPayRollupStore
{
    LenientRetroPayRollup? Find(string code);
    void Save(LenientRetroPayRollup item);
    bool Remove(string code);
    List<LenientRetroPayRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRetroPayRollupStore(Dictionary<string, LenientRetroPayRollup> items, List<string> order) : ILenientRetroPayRollupStore
{
    public Dictionary<string, LenientRetroPayRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRetroPayRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRetroPayRollup item)
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

    public List<LenientRetroPayRollup> List()
    {
        var outValue = new List<LenientRetroPayRollup>();
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
public sealed class CachedLenientRetroPayRollupStore(MemoryLenientRetroPayRollupStore inner, LenientRetroPayRollupCache cache, int hits, int misses) : ILenientRetroPayRollupStore
{
    public MemoryLenientRetroPayRollupStore Inner { get; set; } = inner;
    public LenientRetroPayRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRetroPayRollup? Find(string code)
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

    public void Save(LenientRetroPayRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRetroPayRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient retro pay rollups. Empty fields match all.</summary>
public sealed record LenientRetroPayRollupQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRetroPayRollup item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientRetroPayRollup> Apply(List<LenientRetroPayRollup> items)
    {
        var matched = Collections.Filter(items, (LenientRetroPayRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient retro pay rollup store.</summary>
public static class LenientRetroPayRollupStore
{
    public static ILenientRetroPayRollupStore NewMemoryLenientRetroPayRollupStore()
    {
        return new MemoryLenientRetroPayRollupStore(
            new Dictionary<string, LenientRetroPayRollup>(),
            new List<string>());
    }

    public static ILenientRetroPayRollupStore NewCachedLenientRetroPayRollupStore(MemoryLenientRetroPayRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRetroPayRollupStore(inner, LenientRetroPayRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientRetroPayRollup> FindLenientRetroPayRollups(ILenientRetroPayRollupStore store, LenientRetroPayRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
