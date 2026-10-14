namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey targets by code.</summary>
public interface IPulseSurveyTargetStore
{
    PulseSurveyTarget? Find(string code);
    void Save(PulseSurveyTarget item);
    bool Remove(string code);
    List<PulseSurveyTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyTargetStore(Dictionary<string, PulseSurveyTarget> items, List<string> order) : IPulseSurveyTargetStore
{
    public Dictionary<string, PulseSurveyTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyTarget item)
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

    public List<PulseSurveyTarget> List()
    {
        var outValue = new List<PulseSurveyTarget>();
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
public sealed class CachedPulseSurveyTargetStore(MemoryPulseSurveyTargetStore inner, PulseSurveyTargetCache cache, int hits, int misses) : IPulseSurveyTargetStore
{
    public MemoryPulseSurveyTargetStore Inner { get; set; } = inner;
    public PulseSurveyTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyTarget? Find(string code)
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

    public void Save(PulseSurveyTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey targets. Empty fields match all.</summary>
public sealed record PulseSurveyTargetQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyTarget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyTarget> Apply(List<PulseSurveyTarget> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey target store.</summary>
public static class PulseSurveyTargetStore
{
    public static IPulseSurveyTargetStore NewMemoryPulseSurveyTargetStore()
    {
        return new MemoryPulseSurveyTargetStore(new Dictionary<string, PulseSurveyTarget>(), new List<string>());
    }

    public static IPulseSurveyTargetStore NewCachedPulseSurveyTargetStore(MemoryPulseSurveyTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyTargetStore(inner, PulseSurveyTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyTarget> FindPulseSurveyTargets(IPulseSurveyTargetStore store, PulseSurveyTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
