namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global feedback variances by code.</summary>
public interface IGlobalFeedbackVarianceStore
{
    GlobalFeedbackVariance? Find(string code);
    void Save(GlobalFeedbackVariance item);
    bool Remove(string code);
    List<GlobalFeedbackVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalFeedbackVarianceStore(Dictionary<string, GlobalFeedbackVariance> items, List<string> order) : IGlobalFeedbackVarianceStore
{
    public Dictionary<string, GlobalFeedbackVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalFeedbackVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalFeedbackVariance item)
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

    public List<GlobalFeedbackVariance> List()
    {
        var outValue = new List<GlobalFeedbackVariance>();
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
public sealed class CachedGlobalFeedbackVarianceStore(MemoryGlobalFeedbackVarianceStore inner, GlobalFeedbackVarianceCache cache, int hits, int misses) : IGlobalFeedbackVarianceStore
{
    public MemoryGlobalFeedbackVarianceStore Inner { get; set; } = inner;
    public GlobalFeedbackVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalFeedbackVariance? Find(string code)
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

    public void Save(GlobalFeedbackVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalFeedbackVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global feedback variances. Empty fields match all.</summary>
public sealed record GlobalFeedbackVarianceQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalFeedbackVariance item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalFeedbackVariance> Apply(List<GlobalFeedbackVariance> items)
    {
        var matched = Collections.Filter(items, (GlobalFeedbackVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global feedback variance store.</summary>
public static class GlobalFeedbackVarianceStore
{
    public static IGlobalFeedbackVarianceStore NewMemoryGlobalFeedbackVarianceStore()
    {
        return new MemoryGlobalFeedbackVarianceStore(
            new Dictionary<string, GlobalFeedbackVariance>(),
            new List<string>());
    }

    public static IGlobalFeedbackVarianceStore NewCachedGlobalFeedbackVarianceStore(MemoryGlobalFeedbackVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalFeedbackVarianceStore(
            inner,
            GlobalFeedbackVarianceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalFeedbackVariance> FindGlobalFeedbackVariances(IGlobalFeedbackVarianceStore store, GlobalFeedbackVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
