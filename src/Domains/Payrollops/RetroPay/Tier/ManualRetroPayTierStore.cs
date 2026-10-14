namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual retro pay tiers by code.</summary>
public interface IManualRetroPayTierStore
{
    ManualRetroPayTier? Find(string code);
    void Save(ManualRetroPayTier item);
    bool Remove(string code);
    List<ManualRetroPayTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualRetroPayTierStore(Dictionary<string, ManualRetroPayTier> items, List<string> order) : IManualRetroPayTierStore
{
    public Dictionary<string, ManualRetroPayTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualRetroPayTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualRetroPayTier item)
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

    public List<ManualRetroPayTier> List()
    {
        var outValue = new List<ManualRetroPayTier>();
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
public sealed class CachedManualRetroPayTierStore(MemoryManualRetroPayTierStore inner, ManualRetroPayTierCache cache, int hits, int misses) : IManualRetroPayTierStore
{
    public MemoryManualRetroPayTierStore Inner { get; set; } = inner;
    public ManualRetroPayTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualRetroPayTier? Find(string code)
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

    public void Save(ManualRetroPayTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualRetroPayTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual retro pay tiers. Empty fields match all.</summary>
public sealed record ManualRetroPayTierQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ManualRetroPayTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualRetroPayTier> Apply(List<ManualRetroPayTier> items)
    {
        var matched = Collections.Filter(items, (ManualRetroPayTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual retro pay tier store.</summary>
public static class ManualRetroPayTierStore
{
    public static IManualRetroPayTierStore NewMemoryManualRetroPayTierStore()
    {
        return new MemoryManualRetroPayTierStore(new Dictionary<string, ManualRetroPayTier>(), new List<string>());
    }

    public static IManualRetroPayTierStore NewCachedManualRetroPayTierStore(MemoryManualRetroPayTierStore inner, int ttl, IClock clock)
    {
        return new CachedManualRetroPayTierStore(inner, ManualRetroPayTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualRetroPayTier> FindManualRetroPayTiers(IManualRetroPayTierStore store, ManualRetroPayTierQuery query)
    {
        return query.Apply(store.List());
    }
}
