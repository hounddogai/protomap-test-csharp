namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pulse survey overrides by code.</summary>
public interface IStrictPulseSurveyOverrideStore
{
    StrictPulseSurveyOverride? Find(string code);
    void Save(StrictPulseSurveyOverride item);
    bool Remove(string code);
    List<StrictPulseSurveyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPulseSurveyOverrideStore(Dictionary<string, StrictPulseSurveyOverride> items, List<string> order) : IStrictPulseSurveyOverrideStore
{
    public Dictionary<string, StrictPulseSurveyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPulseSurveyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPulseSurveyOverride item)
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

    public List<StrictPulseSurveyOverride> List()
    {
        var outValue = new List<StrictPulseSurveyOverride>();
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
public sealed class CachedStrictPulseSurveyOverrideStore(MemoryStrictPulseSurveyOverrideStore inner, StrictPulseSurveyOverrideCache cache, int hits, int misses) : IStrictPulseSurveyOverrideStore
{
    public MemoryStrictPulseSurveyOverrideStore Inner { get; set; } = inner;
    public StrictPulseSurveyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPulseSurveyOverride? Find(string code)
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

    public void Save(StrictPulseSurveyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPulseSurveyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pulse survey overrides. Empty fields match all.</summary>
public sealed record StrictPulseSurveyOverrideQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPulseSurveyOverride item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPulseSurveyOverride> Apply(List<StrictPulseSurveyOverride> items)
    {
        var matched = Collections.Filter(items, (StrictPulseSurveyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pulse survey override store.</summary>
public static class StrictPulseSurveyOverrideStore
{
    public static IStrictPulseSurveyOverrideStore NewMemoryStrictPulseSurveyOverrideStore()
    {
        return new MemoryStrictPulseSurveyOverrideStore(
            new Dictionary<string, StrictPulseSurveyOverride>(),
            new List<string>());
    }

    public static IStrictPulseSurveyOverrideStore NewCachedStrictPulseSurveyOverrideStore(MemoryStrictPulseSurveyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPulseSurveyOverrideStore(
            inner,
            StrictPulseSurveyOverrideCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictPulseSurveyOverride> FindStrictPulseSurveyOverrides(IStrictPulseSurveyOverrideStore store, StrictPulseSurveyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
