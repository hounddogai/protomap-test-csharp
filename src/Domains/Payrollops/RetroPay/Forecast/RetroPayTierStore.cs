namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay tiers by code.</summary>
public interface IRetroPayTierStore
{
    RetroPayTier? Find(string code);
    void Save(RetroPayTier item);
    bool Remove(string code);
    List<RetroPayTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayTierStore(Dictionary<string, RetroPayTier> items, List<string> order) : IRetroPayTierStore
{
    public Dictionary<string, RetroPayTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayTier item)
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

    public List<RetroPayTier> List()
    {
        var outValue = new List<RetroPayTier>();
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
public sealed class CachedRetroPayTierStore(MemoryRetroPayTierStore inner, RetroPayTierCache cache, int hits, int misses) : IRetroPayTierStore
{
    public MemoryRetroPayTierStore Inner { get; set; } = inner;
    public RetroPayTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayTier? Find(string code)
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

    public void Save(RetroPayTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay tiers. Empty fields match all.</summary>
public sealed record RetroPayTierQuery(string Unit, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayTier item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayTier> Apply(List<RetroPayTier> items)
    {
        var matched = Collections.Filter(items, (RetroPayTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay tier store.</summary>
public static class RetroPayTierStore
{
    public static IRetroPayTierStore NewMemoryRetroPayTierStore()
    {
        return new MemoryRetroPayTierStore(new Dictionary<string, RetroPayTier>(), new List<string>());
    }

    public static IRetroPayTierStore NewCachedRetroPayTierStore(MemoryRetroPayTierStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayTierStore(inner, RetroPayTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayTier> FindRetroPayTiers(IRetroPayTierStore store, RetroPayTierQuery query)
    {
        return query.Apply(store.List());
    }
}
