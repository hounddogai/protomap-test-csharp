namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment schedules by code.</summary>
public interface IAdjustmentScheduleStore
{
    AdjustmentSchedule? Find(string code);
    void Save(AdjustmentSchedule item);
    bool Remove(string code);
    List<AdjustmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentScheduleStore(Dictionary<string, AdjustmentSchedule> items, List<string> order) : IAdjustmentScheduleStore
{
    public Dictionary<string, AdjustmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentSchedule item)
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

    public List<AdjustmentSchedule> List()
    {
        var outValue = new List<AdjustmentSchedule>();
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
public sealed class CachedAdjustmentScheduleStore(MemoryAdjustmentScheduleStore inner, AdjustmentScheduleCache cache, int hits, int misses) : IAdjustmentScheduleStore
{
    public MemoryAdjustmentScheduleStore Inner { get; set; } = inner;
    public AdjustmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentSchedule? Find(string code)
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

    public void Save(AdjustmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment schedules. Empty fields match all.</summary>
public sealed record AdjustmentScheduleQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentSchedule item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AdjustmentSchedule> Apply(List<AdjustmentSchedule> items)
    {
        var matched = Collections.Filter(items, (AdjustmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment schedule store.</summary>
public static class AdjustmentScheduleStore
{
    public static IAdjustmentScheduleStore NewMemoryAdjustmentScheduleStore()
    {
        return new MemoryAdjustmentScheduleStore(new Dictionary<string, AdjustmentSchedule>(), new List<string>());
    }

    public static IAdjustmentScheduleStore NewCachedAdjustmentScheduleStore(MemoryAdjustmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentScheduleStore(inner, AdjustmentScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentSchedule> FindAdjustmentSchedules(IAdjustmentScheduleStore store, AdjustmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
