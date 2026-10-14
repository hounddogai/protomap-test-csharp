namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey adjustments by code.</summary>
public interface IPulseSurveyAdjustmentStore
{
    PulseSurveyAdjustment? Find(string code);
    void Save(PulseSurveyAdjustment item);
    bool Remove(string code);
    List<PulseSurveyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyAdjustmentStore(Dictionary<string, PulseSurveyAdjustment> items, List<string> order) : IPulseSurveyAdjustmentStore
{
    public Dictionary<string, PulseSurveyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyAdjustment item)
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

    public List<PulseSurveyAdjustment> List()
    {
        var outValue = new List<PulseSurveyAdjustment>();
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
public sealed class CachedPulseSurveyAdjustmentStore(MemoryPulseSurveyAdjustmentStore inner, PulseSurveyAdjustmentCache cache, int hits, int misses) : IPulseSurveyAdjustmentStore
{
    public MemoryPulseSurveyAdjustmentStore Inner { get; set; } = inner;
    public PulseSurveyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyAdjustment? Find(string code)
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

    public void Save(PulseSurveyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey adjustments. Empty fields match all.</summary>
public sealed record PulseSurveyAdjustmentQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyAdjustment> Apply(List<PulseSurveyAdjustment> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey adjustment store.</summary>
public static class PulseSurveyAdjustmentStore
{
    public static IPulseSurveyAdjustmentStore NewMemoryPulseSurveyAdjustmentStore()
    {
        return new MemoryPulseSurveyAdjustmentStore(
            new Dictionary<string, PulseSurveyAdjustment>(),
            new List<string>());
    }

    public static IPulseSurveyAdjustmentStore NewCachedPulseSurveyAdjustmentStore(MemoryPulseSurveyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyAdjustmentStore(inner, PulseSurveyAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyAdjustment> FindPulseSurveyAdjustments(IPulseSurveyAdjustmentStore store, PulseSurveyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
