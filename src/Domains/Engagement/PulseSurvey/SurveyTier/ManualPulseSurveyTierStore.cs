namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey tiers by code.</summary>
public interface IManualPulseSurveyTierStore
{
    ManualPulseSurveyTier? Find(string code);
    void Save(ManualPulseSurveyTier item);
    bool Remove(string code);
    List<ManualPulseSurveyTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveyTierStore(Dictionary<string, ManualPulseSurveyTier> items, List<string> order) : IManualPulseSurveyTierStore
{
    public Dictionary<string, ManualPulseSurveyTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveyTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveyTier item)
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

    public List<ManualPulseSurveyTier> List()
    {
        var outValue = new List<ManualPulseSurveyTier>();
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
public sealed class CachedManualPulseSurveyTierStore(MemoryManualPulseSurveyTierStore inner, ManualPulseSurveyTierCache cache, int hits, int misses) : IManualPulseSurveyTierStore
{
    public MemoryManualPulseSurveyTierStore Inner { get; set; } = inner;
    public ManualPulseSurveyTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveyTier? Find(string code)
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

    public void Save(ManualPulseSurveyTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveyTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey tiers. Empty fields match all.</summary>
public sealed record ManualPulseSurveyTierQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveyTier item)
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

    public List<ManualPulseSurveyTier> Apply(List<ManualPulseSurveyTier> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveyTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey tier store.</summary>
public static class ManualPulseSurveyTierStore
{
    public static IManualPulseSurveyTierStore NewMemoryManualPulseSurveyTierStore()
    {
        return new MemoryManualPulseSurveyTierStore(
            new Dictionary<string, ManualPulseSurveyTier>(),
            new List<string>());
    }

    public static IManualPulseSurveyTierStore NewCachedManualPulseSurveyTierStore(MemoryManualPulseSurveyTierStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveyTierStore(inner, ManualPulseSurveyTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualPulseSurveyTier> FindManualPulseSurveyTiers(IManualPulseSurveyTierStore store, ManualPulseSurveyTierQuery query)
    {
        return query.Apply(store.List());
    }
}
