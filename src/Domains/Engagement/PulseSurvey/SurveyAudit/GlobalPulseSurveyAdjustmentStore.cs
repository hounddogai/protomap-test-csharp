namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pulse survey adjustments by code.</summary>
public interface IGlobalPulseSurveyAdjustmentStore
{
    GlobalPulseSurveyAdjustment? Find(string code);
    void Save(GlobalPulseSurveyAdjustment item);
    bool Remove(string code);
    List<GlobalPulseSurveyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPulseSurveyAdjustmentStore(Dictionary<string, GlobalPulseSurveyAdjustment> items, List<string> order) : IGlobalPulseSurveyAdjustmentStore
{
    public Dictionary<string, GlobalPulseSurveyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPulseSurveyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPulseSurveyAdjustment item)
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

    public List<GlobalPulseSurveyAdjustment> List()
    {
        var outValue = new List<GlobalPulseSurveyAdjustment>();
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
public sealed class CachedGlobalPulseSurveyAdjustmentStore(MemoryGlobalPulseSurveyAdjustmentStore inner, GlobalPulseSurveyAdjustmentCache cache, int hits, int misses) : IGlobalPulseSurveyAdjustmentStore
{
    public MemoryGlobalPulseSurveyAdjustmentStore Inner { get; set; } = inner;
    public GlobalPulseSurveyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPulseSurveyAdjustment? Find(string code)
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

    public void Save(GlobalPulseSurveyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPulseSurveyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pulse survey adjustments. Empty fields match all.</summary>
public sealed record GlobalPulseSurveyAdjustmentQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPulseSurveyAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPulseSurveyAdjustment> Apply(List<GlobalPulseSurveyAdjustment> items)
    {
        var matched = Collections.Filter(items, (GlobalPulseSurveyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pulse survey adjustment store.</summary>
public static class GlobalPulseSurveyAdjustmentStore
{
    public static IGlobalPulseSurveyAdjustmentStore NewMemoryGlobalPulseSurveyAdjustmentStore()
    {
        return new MemoryGlobalPulseSurveyAdjustmentStore(
            new Dictionary<string, GlobalPulseSurveyAdjustment>(),
            new List<string>());
    }

    public static IGlobalPulseSurveyAdjustmentStore NewCachedGlobalPulseSurveyAdjustmentStore(MemoryGlobalPulseSurveyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPulseSurveyAdjustmentStore(
            inner,
            GlobalPulseSurveyAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPulseSurveyAdjustment> FindGlobalPulseSurveyAdjustments(IGlobalPulseSurveyAdjustmentStore store, GlobalPulseSurveyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
