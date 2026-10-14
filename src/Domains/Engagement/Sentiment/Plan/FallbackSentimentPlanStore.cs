namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback sentiment plans by code.</summary>
public interface IFallbackSentimentPlanStore
{
    FallbackSentimentPlan? Find(string code);
    void Save(FallbackSentimentPlan item);
    bool Remove(string code);
    List<FallbackSentimentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSentimentPlanStore(Dictionary<string, FallbackSentimentPlan> items, List<string> order) : IFallbackSentimentPlanStore
{
    public Dictionary<string, FallbackSentimentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSentimentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSentimentPlan item)
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

    public List<FallbackSentimentPlan> List()
    {
        var outValue = new List<FallbackSentimentPlan>();
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
public sealed class CachedFallbackSentimentPlanStore(MemoryFallbackSentimentPlanStore inner, FallbackSentimentPlanCache cache, int hits, int misses) : IFallbackSentimentPlanStore
{
    public MemoryFallbackSentimentPlanStore Inner { get; set; } = inner;
    public FallbackSentimentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSentimentPlan? Find(string code)
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

    public void Save(FallbackSentimentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSentimentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback sentiment plans. Empty fields match all.</summary>
public sealed record FallbackSentimentPlanQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSentimentPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSentimentPlan> Apply(List<FallbackSentimentPlan> items)
    {
        var matched = Collections.Filter(items, (FallbackSentimentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback sentiment plan store.</summary>
public static class FallbackSentimentPlanStore
{
    public static IFallbackSentimentPlanStore NewMemoryFallbackSentimentPlanStore()
    {
        return new MemoryFallbackSentimentPlanStore(
            new Dictionary<string, FallbackSentimentPlan>(),
            new List<string>());
    }

    public static IFallbackSentimentPlanStore NewCachedFallbackSentimentPlanStore(MemoryFallbackSentimentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSentimentPlanStore(
            inner,
            FallbackSentimentPlanCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSentimentPlan> FindFallbackSentimentPlans(IFallbackSentimentPlanStore store, FallbackSentimentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
