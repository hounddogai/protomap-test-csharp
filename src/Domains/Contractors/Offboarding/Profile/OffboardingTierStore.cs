namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding tiers by code.</summary>
public interface IOffboardingTierStore
{
    OffboardingTier? Find(string code);
    void Save(OffboardingTier item);
    bool Remove(string code);
    List<OffboardingTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingTierStore(Dictionary<string, OffboardingTier> items, List<string> order) : IOffboardingTierStore
{
    public Dictionary<string, OffboardingTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingTier item)
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

    public List<OffboardingTier> List()
    {
        var outValue = new List<OffboardingTier>();
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
public sealed class CachedOffboardingTierStore(MemoryOffboardingTierStore inner, OffboardingTierCache cache, int hits, int misses) : IOffboardingTierStore
{
    public MemoryOffboardingTierStore Inner { get; set; } = inner;
    public OffboardingTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingTier? Find(string code)
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

    public void Save(OffboardingTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding tiers. Empty fields match all.</summary>
public sealed record OffboardingTierQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<OffboardingTier> Apply(List<OffboardingTier> items)
    {
        var matched = Collections.Filter(items, (OffboardingTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding tier store.</summary>
public static class OffboardingTierStore
{
    public static IOffboardingTierStore NewMemoryOffboardingTierStore()
    {
        return new MemoryOffboardingTierStore(new Dictionary<string, OffboardingTier>(), new List<string>());
    }

    public static IOffboardingTierStore NewCachedOffboardingTierStore(MemoryOffboardingTierStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingTierStore(inner, OffboardingTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OffboardingTier> FindOffboardingTiers(IOffboardingTierStore store, OffboardingTierQuery query)
    {
        return query.Apply(store.List());
    }
}
