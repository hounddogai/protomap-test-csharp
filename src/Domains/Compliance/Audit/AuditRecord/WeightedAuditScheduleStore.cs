namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted audit schedules by code.</summary>
public interface IWeightedAuditScheduleStore
{
    WeightedAuditSchedule? Find(string code);
    void Save(WeightedAuditSchedule item);
    bool Remove(string code);
    List<WeightedAuditSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAuditScheduleStore(Dictionary<string, WeightedAuditSchedule> items, List<string> order) : IWeightedAuditScheduleStore
{
    public Dictionary<string, WeightedAuditSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAuditSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAuditSchedule item)
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

    public List<WeightedAuditSchedule> List()
    {
        var outValue = new List<WeightedAuditSchedule>();
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
public sealed class CachedWeightedAuditScheduleStore(MemoryWeightedAuditScheduleStore inner, WeightedAuditScheduleCache cache, int hits, int misses) : IWeightedAuditScheduleStore
{
    public MemoryWeightedAuditScheduleStore Inner { get; set; } = inner;
    public WeightedAuditScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAuditSchedule? Find(string code)
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

    public void Save(WeightedAuditSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAuditSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted audit schedules. Empty fields match all.</summary>
public sealed record WeightedAuditScheduleQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAuditSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<WeightedAuditSchedule> Apply(List<WeightedAuditSchedule> items)
    {
        var matched = Collections.Filter(items, (WeightedAuditSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted audit schedule store.</summary>
public static class WeightedAuditScheduleStore
{
    public static IWeightedAuditScheduleStore NewMemoryWeightedAuditScheduleStore()
    {
        return new MemoryWeightedAuditScheduleStore(
            new Dictionary<string, WeightedAuditSchedule>(),
            new List<string>());
    }

    public static IWeightedAuditScheduleStore NewCachedWeightedAuditScheduleStore(MemoryWeightedAuditScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAuditScheduleStore(inner, WeightedAuditScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WeightedAuditSchedule> FindWeightedAuditSchedules(IWeightedAuditScheduleStore store, WeightedAuditScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
