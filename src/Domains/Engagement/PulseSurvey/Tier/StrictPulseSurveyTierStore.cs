namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pulse survey tiers by code.</summary>
public interface IStrictPulseSurveyTierStore
{
    StrictPulseSurveyTier? Find(string code);
    void Save(StrictPulseSurveyTier item);
    bool Remove(string code);
    List<StrictPulseSurveyTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPulseSurveyTierStore(Dictionary<string, StrictPulseSurveyTier> items, List<string> order) : IStrictPulseSurveyTierStore
{
    public Dictionary<string, StrictPulseSurveyTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPulseSurveyTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPulseSurveyTier item)
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

    public List<StrictPulseSurveyTier> List()
    {
        var outValue = new List<StrictPulseSurveyTier>();
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
public sealed class CachedStrictPulseSurveyTierStore(MemoryStrictPulseSurveyTierStore inner, StrictPulseSurveyTierCache cache, int hits, int misses) : IStrictPulseSurveyTierStore
{
    public MemoryStrictPulseSurveyTierStore Inner { get; set; } = inner;
    public StrictPulseSurveyTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPulseSurveyTier? Find(string code)
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

    public void Save(StrictPulseSurveyTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPulseSurveyTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pulse survey tiers. Empty fields match all.</summary>
public sealed record StrictPulseSurveyTierQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPulseSurveyTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPulseSurveyTier> Apply(List<StrictPulseSurveyTier> items)
    {
        var matched = Collections.Filter(items, (StrictPulseSurveyTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pulse survey tier store.</summary>
public static class StrictPulseSurveyTierStore
{
    public static IStrictPulseSurveyTierStore NewMemoryStrictPulseSurveyTierStore()
    {
        return new MemoryStrictPulseSurveyTierStore(
            new Dictionary<string, StrictPulseSurveyTier>(),
            new List<string>());
    }

    public static IStrictPulseSurveyTierStore NewCachedStrictPulseSurveyTierStore(MemoryStrictPulseSurveyTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPulseSurveyTierStore(
            inner,
            StrictPulseSurveyTierCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictPulseSurveyTier> FindStrictPulseSurveyTiers(IStrictPulseSurveyTierStore store, StrictPulseSurveyTierQuery query)
    {
        return query.Apply(store.List());
    }
}
