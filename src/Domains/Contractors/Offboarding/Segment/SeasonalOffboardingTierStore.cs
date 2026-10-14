namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal offboarding tiers by code.</summary>
public interface ISeasonalOffboardingTierStore
{
    SeasonalOffboardingTier? Find(string code);
    void Save(SeasonalOffboardingTier item);
    bool Remove(string code);
    List<SeasonalOffboardingTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalOffboardingTierStore(Dictionary<string, SeasonalOffboardingTier> items, List<string> order) : ISeasonalOffboardingTierStore
{
    public Dictionary<string, SeasonalOffboardingTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalOffboardingTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalOffboardingTier item)
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

    public List<SeasonalOffboardingTier> List()
    {
        var outValue = new List<SeasonalOffboardingTier>();
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
public sealed class CachedSeasonalOffboardingTierStore(MemorySeasonalOffboardingTierStore inner, SeasonalOffboardingTierCache cache, int hits, int misses) : ISeasonalOffboardingTierStore
{
    public MemorySeasonalOffboardingTierStore Inner { get; set; } = inner;
    public SeasonalOffboardingTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalOffboardingTier? Find(string code)
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

    public void Save(SeasonalOffboardingTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalOffboardingTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal offboarding tiers. Empty fields match all.</summary>
public sealed record SeasonalOffboardingTierQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalOffboardingTier item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalOffboardingTier> Apply(List<SeasonalOffboardingTier> items)
    {
        var matched = Collections.Filter(items, (SeasonalOffboardingTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal offboarding tier store.</summary>
public static class SeasonalOffboardingTierStore
{
    public static ISeasonalOffboardingTierStore NewMemorySeasonalOffboardingTierStore()
    {
        return new MemorySeasonalOffboardingTierStore(
            new Dictionary<string, SeasonalOffboardingTier>(),
            new List<string>());
    }

    public static ISeasonalOffboardingTierStore NewCachedSeasonalOffboardingTierStore(MemorySeasonalOffboardingTierStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalOffboardingTierStore(
            inner,
            SeasonalOffboardingTierCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalOffboardingTier> FindSeasonalOffboardingTiers(ISeasonalOffboardingTierStore store, SeasonalOffboardingTierQuery query)
    {
        return query.Apply(store.List());
    }
}
