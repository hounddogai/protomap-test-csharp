namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback recognition plans by code.</summary>
public interface IFallbackRecognitionPlanStore
{
    FallbackRecognitionPlan? Find(string code);
    void Save(FallbackRecognitionPlan item);
    bool Remove(string code);
    List<FallbackRecognitionPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRecognitionPlanStore(Dictionary<string, FallbackRecognitionPlan> items, List<string> order) : IFallbackRecognitionPlanStore
{
    public Dictionary<string, FallbackRecognitionPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRecognitionPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRecognitionPlan item)
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

    public List<FallbackRecognitionPlan> List()
    {
        var outValue = new List<FallbackRecognitionPlan>();
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
public sealed class CachedFallbackRecognitionPlanStore(MemoryFallbackRecognitionPlanStore inner, FallbackRecognitionPlanCache cache, int hits, int misses) : IFallbackRecognitionPlanStore
{
    public MemoryFallbackRecognitionPlanStore Inner { get; set; } = inner;
    public FallbackRecognitionPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRecognitionPlan? Find(string code)
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

    public void Save(FallbackRecognitionPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRecognitionPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback recognition plans. Empty fields match all.</summary>
public sealed record FallbackRecognitionPlanQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRecognitionPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRecognitionPlan> Apply(List<FallbackRecognitionPlan> items)
    {
        var matched = Collections.Filter(items, (FallbackRecognitionPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback recognition plan store.</summary>
public static class FallbackRecognitionPlanStore
{
    public static IFallbackRecognitionPlanStore NewMemoryFallbackRecognitionPlanStore()
    {
        return new MemoryFallbackRecognitionPlanStore(
            new Dictionary<string, FallbackRecognitionPlan>(),
            new List<string>());
    }

    public static IFallbackRecognitionPlanStore NewCachedFallbackRecognitionPlanStore(MemoryFallbackRecognitionPlanStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRecognitionPlanStore(
            inner,
            FallbackRecognitionPlanCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRecognitionPlan> FindFallbackRecognitionPlans(IFallbackRecognitionPlanStore store, FallbackRecognitionPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
