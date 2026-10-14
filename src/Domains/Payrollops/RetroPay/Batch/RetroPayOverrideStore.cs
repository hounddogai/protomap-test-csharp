namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay overrides by code.</summary>
public interface IRetroPayOverrideStore
{
    RetroPayOverride? Find(string code);
    void Save(RetroPayOverride item);
    bool Remove(string code);
    List<RetroPayOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayOverrideStore(Dictionary<string, RetroPayOverride> items, List<string> order) : IRetroPayOverrideStore
{
    public Dictionary<string, RetroPayOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayOverride item)
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

    public List<RetroPayOverride> List()
    {
        var outValue = new List<RetroPayOverride>();
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
public sealed class CachedRetroPayOverrideStore(MemoryRetroPayOverrideStore inner, RetroPayOverrideCache cache, int hits, int misses) : IRetroPayOverrideStore
{
    public MemoryRetroPayOverrideStore Inner { get; set; } = inner;
    public RetroPayOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayOverride? Find(string code)
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

    public void Save(RetroPayOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay overrides. Empty fields match all.</summary>
public sealed record RetroPayOverrideQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayOverride item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayOverride> Apply(List<RetroPayOverride> items)
    {
        var matched = Collections.Filter(items, (RetroPayOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay override store.</summary>
public static class RetroPayOverrideStore
{
    public static IRetroPayOverrideStore NewMemoryRetroPayOverrideStore()
    {
        return new MemoryRetroPayOverrideStore(new Dictionary<string, RetroPayOverride>(), new List<string>());
    }

    public static IRetroPayOverrideStore NewCachedRetroPayOverrideStore(MemoryRetroPayOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayOverrideStore(inner, RetroPayOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayOverride> FindRetroPayOverrides(IRetroPayOverrideStore store, RetroPayOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
