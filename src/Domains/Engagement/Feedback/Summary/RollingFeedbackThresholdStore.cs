namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling feedback thresholds by code.</summary>
public interface IRollingFeedbackThresholdStore
{
    RollingFeedbackThreshold? Find(string code);
    void Save(RollingFeedbackThreshold item);
    bool Remove(string code);
    List<RollingFeedbackThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingFeedbackThresholdStore(Dictionary<string, RollingFeedbackThreshold> items, List<string> order) : IRollingFeedbackThresholdStore
{
    public Dictionary<string, RollingFeedbackThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingFeedbackThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingFeedbackThreshold item)
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

    public List<RollingFeedbackThreshold> List()
    {
        var outValue = new List<RollingFeedbackThreshold>();
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
public sealed class CachedRollingFeedbackThresholdStore(MemoryRollingFeedbackThresholdStore inner, RollingFeedbackThresholdCache cache, int hits, int misses) : IRollingFeedbackThresholdStore
{
    public MemoryRollingFeedbackThresholdStore Inner { get; set; } = inner;
    public RollingFeedbackThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingFeedbackThreshold? Find(string code)
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

    public void Save(RollingFeedbackThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingFeedbackThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling feedback thresholds. Empty fields match all.</summary>
public sealed record RollingFeedbackThresholdQuery(string Unit, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RollingFeedbackThreshold item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingFeedbackThreshold> Apply(List<RollingFeedbackThreshold> items)
    {
        var matched = Collections.Filter(items, (RollingFeedbackThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling feedback threshold store.</summary>
public static class RollingFeedbackThresholdStore
{
    public static IRollingFeedbackThresholdStore NewMemoryRollingFeedbackThresholdStore()
    {
        return new MemoryRollingFeedbackThresholdStore(
            new Dictionary<string, RollingFeedbackThreshold>(),
            new List<string>());
    }

    public static IRollingFeedbackThresholdStore NewCachedRollingFeedbackThresholdStore(MemoryRollingFeedbackThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedRollingFeedbackThresholdStore(
            inner,
            RollingFeedbackThresholdCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingFeedbackThreshold> FindRollingFeedbackThresholds(IRollingFeedbackThresholdStore store, RollingFeedbackThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
