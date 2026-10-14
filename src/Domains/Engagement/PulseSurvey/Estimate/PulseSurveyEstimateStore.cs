namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey estimates by code.</summary>
public interface IPulseSurveyEstimateStore
{
    PulseSurveyEstimate? Find(string code);
    void Save(PulseSurveyEstimate item);
    bool Remove(string code);
    List<PulseSurveyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyEstimateStore(Dictionary<string, PulseSurveyEstimate> items, List<string> order) : IPulseSurveyEstimateStore
{
    public Dictionary<string, PulseSurveyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyEstimate item)
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

    public List<PulseSurveyEstimate> List()
    {
        var outValue = new List<PulseSurveyEstimate>();
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
public sealed class CachedPulseSurveyEstimateStore(MemoryPulseSurveyEstimateStore inner, PulseSurveyEstimateCache cache, int hits, int misses) : IPulseSurveyEstimateStore
{
    public MemoryPulseSurveyEstimateStore Inner { get; set; } = inner;
    public PulseSurveyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyEstimate? Find(string code)
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

    public void Save(PulseSurveyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey estimates. Empty fields match all.</summary>
public sealed record PulseSurveyEstimateQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyEstimate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<PulseSurveyEstimate> Apply(List<PulseSurveyEstimate> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey estimate store.</summary>
public static class PulseSurveyEstimateStore
{
    public static IPulseSurveyEstimateStore NewMemoryPulseSurveyEstimateStore()
    {
        return new MemoryPulseSurveyEstimateStore(new Dictionary<string, PulseSurveyEstimate>(), new List<string>());
    }

    public static IPulseSurveyEstimateStore NewCachedPulseSurveyEstimateStore(MemoryPulseSurveyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyEstimateStore(inner, PulseSurveyEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyEstimate> FindPulseSurveyEstimates(IPulseSurveyEstimateStore store, PulseSurveyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
