namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey overrides by code.</summary>
public interface IPulseSurveyOverrideStore
{
    PulseSurveyOverride? Find(string code);
    void Save(PulseSurveyOverride item);
    bool Remove(string code);
    List<PulseSurveyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyOverrideStore(Dictionary<string, PulseSurveyOverride> items, List<string> order) : IPulseSurveyOverrideStore
{
    public Dictionary<string, PulseSurveyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyOverride item)
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

    public List<PulseSurveyOverride> List()
    {
        var outValue = new List<PulseSurveyOverride>();
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
public sealed class CachedPulseSurveyOverrideStore(MemoryPulseSurveyOverrideStore inner, PulseSurveyOverrideCache cache, int hits, int misses) : IPulseSurveyOverrideStore
{
    public MemoryPulseSurveyOverrideStore Inner { get; set; } = inner;
    public PulseSurveyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyOverride? Find(string code)
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

    public void Save(PulseSurveyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey overrides. Empty fields match all.</summary>
public sealed record PulseSurveyOverrideQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyOverride item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyOverride> Apply(List<PulseSurveyOverride> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey override store.</summary>
public static class PulseSurveyOverrideStore
{
    public static IPulseSurveyOverrideStore NewMemoryPulseSurveyOverrideStore()
    {
        return new MemoryPulseSurveyOverrideStore(new Dictionary<string, PulseSurveyOverride>(), new List<string>());
    }

    public static IPulseSurveyOverrideStore NewCachedPulseSurveyOverrideStore(MemoryPulseSurveyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyOverrideStore(inner, PulseSurveyOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyOverride> FindPulseSurveyOverrides(IPulseSurveyOverrideStore store, PulseSurveyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
