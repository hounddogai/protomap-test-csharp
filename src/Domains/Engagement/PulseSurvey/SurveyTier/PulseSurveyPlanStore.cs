namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey plans by code.</summary>
public interface IPulseSurveyPlanStore
{
    PulseSurveyPlan? Find(string code);
    void Save(PulseSurveyPlan item);
    bool Remove(string code);
    List<PulseSurveyPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyPlanStore(Dictionary<string, PulseSurveyPlan> items, List<string> order) : IPulseSurveyPlanStore
{
    public Dictionary<string, PulseSurveyPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyPlan item)
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

    public List<PulseSurveyPlan> List()
    {
        var outValue = new List<PulseSurveyPlan>();
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
public sealed class CachedPulseSurveyPlanStore(MemoryPulseSurveyPlanStore inner, PulseSurveyPlanCache cache, int hits, int misses) : IPulseSurveyPlanStore
{
    public MemoryPulseSurveyPlanStore Inner { get; set; } = inner;
    public PulseSurveyPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyPlan? Find(string code)
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

    public void Save(PulseSurveyPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey plans. Empty fields match all.</summary>
public sealed record PulseSurveyPlanQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PulseSurveyPlan> Apply(List<PulseSurveyPlan> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey plan store.</summary>
public static class PulseSurveyPlanStore
{
    public static IPulseSurveyPlanStore NewMemoryPulseSurveyPlanStore()
    {
        return new MemoryPulseSurveyPlanStore(new Dictionary<string, PulseSurveyPlan>(), new List<string>());
    }

    public static IPulseSurveyPlanStore NewCachedPulseSurveyPlanStore(MemoryPulseSurveyPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyPlanStore(inner, PulseSurveyPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyPlan> FindPulseSurveyPlans(IPulseSurveyPlanStore store, PulseSurveyPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
