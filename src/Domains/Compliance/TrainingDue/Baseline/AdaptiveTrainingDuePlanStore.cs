namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive training due plans by code.</summary>
public interface IAdaptiveTrainingDuePlanStore
{
    AdaptiveTrainingDuePlan? Find(string code);
    void Save(AdaptiveTrainingDuePlan item);
    bool Remove(string code);
    List<AdaptiveTrainingDuePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTrainingDuePlanStore(Dictionary<string, AdaptiveTrainingDuePlan> items, List<string> order) : IAdaptiveTrainingDuePlanStore
{
    public Dictionary<string, AdaptiveTrainingDuePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTrainingDuePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTrainingDuePlan item)
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

    public List<AdaptiveTrainingDuePlan> List()
    {
        var outValue = new List<AdaptiveTrainingDuePlan>();
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
public sealed class CachedAdaptiveTrainingDuePlanStore(MemoryAdaptiveTrainingDuePlanStore inner, AdaptiveTrainingDuePlanCache cache, int hits, int misses) : IAdaptiveTrainingDuePlanStore
{
    public MemoryAdaptiveTrainingDuePlanStore Inner { get; set; } = inner;
    public AdaptiveTrainingDuePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTrainingDuePlan? Find(string code)
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

    public void Save(AdaptiveTrainingDuePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTrainingDuePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive training due plans. Empty fields match all.</summary>
public sealed record AdaptiveTrainingDuePlanQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTrainingDuePlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AdaptiveTrainingDuePlan> Apply(List<AdaptiveTrainingDuePlan> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTrainingDuePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive training due plan store.</summary>
public static class AdaptiveTrainingDuePlanStore
{
    public static IAdaptiveTrainingDuePlanStore NewMemoryAdaptiveTrainingDuePlanStore()
    {
        return new MemoryAdaptiveTrainingDuePlanStore(
            new Dictionary<string, AdaptiveTrainingDuePlan>(),
            new List<string>());
    }

    public static IAdaptiveTrainingDuePlanStore NewCachedAdaptiveTrainingDuePlanStore(MemoryAdaptiveTrainingDuePlanStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTrainingDuePlanStore(
            inner,
            AdaptiveTrainingDuePlanCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTrainingDuePlan> FindAdaptiveTrainingDuePlans(IAdaptiveTrainingDuePlanStore store, AdaptiveTrainingDuePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
