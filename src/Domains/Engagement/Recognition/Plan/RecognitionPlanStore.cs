namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition plans by code.</summary>
public interface IRecognitionPlanStore
{
    RecognitionPlan? Find(string code);
    void Save(RecognitionPlan item);
    bool Remove(string code);
    List<RecognitionPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionPlanStore(Dictionary<string, RecognitionPlan> items, List<string> order) : IRecognitionPlanStore
{
    public Dictionary<string, RecognitionPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionPlan item)
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

    public List<RecognitionPlan> List()
    {
        var outValue = new List<RecognitionPlan>();
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
public sealed class CachedRecognitionPlanStore(MemoryRecognitionPlanStore inner, RecognitionPlanCache cache, int hits, int misses) : IRecognitionPlanStore
{
    public MemoryRecognitionPlanStore Inner { get; set; } = inner;
    public RecognitionPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionPlan? Find(string code)
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

    public void Save(RecognitionPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition plans. Empty fields match all.</summary>
public sealed record RecognitionPlanQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionPlan item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionPlan> Apply(List<RecognitionPlan> items)
    {
        var matched = Collections.Filter(items, (RecognitionPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition plan store.</summary>
public static class RecognitionPlanStore
{
    public static IRecognitionPlanStore NewMemoryRecognitionPlanStore()
    {
        return new MemoryRecognitionPlanStore(new Dictionary<string, RecognitionPlan>(), new List<string>());
    }

    public static IRecognitionPlanStore NewCachedRecognitionPlanStore(MemoryRecognitionPlanStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionPlanStore(inner, RecognitionPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RecognitionPlan> FindRecognitionPlans(IRecognitionPlanStore store, RecognitionPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
