namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled nomination adjustments by code.</summary>
public interface IScheduledNominationAdjustmentStore
{
    ScheduledNominationAdjustment? Find(string code);
    void Save(ScheduledNominationAdjustment item);
    bool Remove(string code);
    List<ScheduledNominationAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledNominationAdjustmentStore(Dictionary<string, ScheduledNominationAdjustment> items, List<string> order) : IScheduledNominationAdjustmentStore
{
    public Dictionary<string, ScheduledNominationAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledNominationAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledNominationAdjustment item)
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

    public List<ScheduledNominationAdjustment> List()
    {
        var outValue = new List<ScheduledNominationAdjustment>();
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
public sealed class CachedScheduledNominationAdjustmentStore(MemoryScheduledNominationAdjustmentStore inner, ScheduledNominationAdjustmentCache cache, int hits, int misses) : IScheduledNominationAdjustmentStore
{
    public MemoryScheduledNominationAdjustmentStore Inner { get; set; } = inner;
    public ScheduledNominationAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledNominationAdjustment? Find(string code)
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

    public void Save(ScheduledNominationAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledNominationAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled nomination adjustments. Empty fields match all.</summary>
public sealed record ScheduledNominationAdjustmentQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledNominationAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledNominationAdjustment> Apply(List<ScheduledNominationAdjustment> items)
    {
        var matched = Collections.Filter(items, (ScheduledNominationAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled nomination adjustment store.</summary>
public static class ScheduledNominationAdjustmentStore
{
    public static IScheduledNominationAdjustmentStore NewMemoryScheduledNominationAdjustmentStore()
    {
        return new MemoryScheduledNominationAdjustmentStore(
            new Dictionary<string, ScheduledNominationAdjustment>(),
            new List<string>());
    }

    public static IScheduledNominationAdjustmentStore NewCachedScheduledNominationAdjustmentStore(MemoryScheduledNominationAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledNominationAdjustmentStore(
            inner,
            ScheduledNominationAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledNominationAdjustment> FindScheduledNominationAdjustments(IScheduledNominationAdjustmentStore store, ScheduledNominationAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
