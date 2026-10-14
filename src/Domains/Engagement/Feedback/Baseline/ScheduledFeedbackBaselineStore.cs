namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled feedback baselines by code.</summary>
public interface IScheduledFeedbackBaselineStore
{
    ScheduledFeedbackBaseline? Find(string code);
    void Save(ScheduledFeedbackBaseline item);
    bool Remove(string code);
    List<ScheduledFeedbackBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFeedbackBaselineStore(Dictionary<string, ScheduledFeedbackBaseline> items, List<string> order) : IScheduledFeedbackBaselineStore
{
    public Dictionary<string, ScheduledFeedbackBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFeedbackBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFeedbackBaseline item)
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

    public List<ScheduledFeedbackBaseline> List()
    {
        var outValue = new List<ScheduledFeedbackBaseline>();
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
public sealed class CachedScheduledFeedbackBaselineStore(MemoryScheduledFeedbackBaselineStore inner, ScheduledFeedbackBaselineCache cache, int hits, int misses) : IScheduledFeedbackBaselineStore
{
    public MemoryScheduledFeedbackBaselineStore Inner { get; set; } = inner;
    public ScheduledFeedbackBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFeedbackBaseline? Find(string code)
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

    public void Save(ScheduledFeedbackBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFeedbackBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled feedback baselines. Empty fields match all.</summary>
public sealed record ScheduledFeedbackBaselineQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFeedbackBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFeedbackBaseline> Apply(List<ScheduledFeedbackBaseline> items)
    {
        var matched = Collections.Filter(items, (ScheduledFeedbackBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled feedback baseline store.</summary>
public static class ScheduledFeedbackBaselineStore
{
    public static IScheduledFeedbackBaselineStore NewMemoryScheduledFeedbackBaselineStore()
    {
        return new MemoryScheduledFeedbackBaselineStore(
            new Dictionary<string, ScheduledFeedbackBaseline>(),
            new List<string>());
    }

    public static IScheduledFeedbackBaselineStore NewCachedScheduledFeedbackBaselineStore(MemoryScheduledFeedbackBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFeedbackBaselineStore(
            inner,
            ScheduledFeedbackBaselineCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFeedbackBaseline> FindScheduledFeedbackBaselines(IScheduledFeedbackBaselineStore store, ScheduledFeedbackBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
