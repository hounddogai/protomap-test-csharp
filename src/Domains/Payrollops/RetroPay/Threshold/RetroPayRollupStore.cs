namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay rollups by code.</summary>
public interface IRetroPayRollupStore
{
    RetroPayRollup? Find(string code);
    void Save(RetroPayRollup item);
    bool Remove(string code);
    List<RetroPayRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayRollupStore(Dictionary<string, RetroPayRollup> items, List<string> order) : IRetroPayRollupStore
{
    public Dictionary<string, RetroPayRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayRollup item)
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

    public List<RetroPayRollup> List()
    {
        var outValue = new List<RetroPayRollup>();
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
public sealed class CachedRetroPayRollupStore(MemoryRetroPayRollupStore inner, RetroPayRollupCache cache, int hits, int misses) : IRetroPayRollupStore
{
    public MemoryRetroPayRollupStore Inner { get; set; } = inner;
    public RetroPayRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayRollup? Find(string code)
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

    public void Save(RetroPayRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay rollups. Empty fields match all.</summary>
public sealed record RetroPayRollupQuery(string Source, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayRollup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayRollup> Apply(List<RetroPayRollup> items)
    {
        var matched = Collections.Filter(items, (RetroPayRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay rollup store.</summary>
public static class RetroPayRollupStore
{
    public static IRetroPayRollupStore NewMemoryRetroPayRollupStore()
    {
        return new MemoryRetroPayRollupStore(new Dictionary<string, RetroPayRollup>(), new List<string>());
    }

    public static IRetroPayRollupStore NewCachedRetroPayRollupStore(MemoryRetroPayRollupStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayRollupStore(inner, RetroPayRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayRollup> FindRetroPayRollups(IRetroPayRollupStore store, RetroPayRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
