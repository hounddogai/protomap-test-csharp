namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit schedules by code.</summary>
public interface IAuditScheduleStore
{
    AuditSchedule? Find(string code);
    void Save(AuditSchedule item);
    bool Remove(string code);
    List<AuditSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditScheduleStore(Dictionary<string, AuditSchedule> items, List<string> order) : IAuditScheduleStore
{
    public Dictionary<string, AuditSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditSchedule item)
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

    public List<AuditSchedule> List()
    {
        var outValue = new List<AuditSchedule>();
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
public sealed class CachedAuditScheduleStore(MemoryAuditScheduleStore inner, AuditScheduleCache cache, int hits, int misses) : IAuditScheduleStore
{
    public MemoryAuditScheduleStore Inner { get; set; } = inner;
    public AuditScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditSchedule? Find(string code)
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

    public void Save(AuditSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit schedules. Empty fields match all.</summary>
public sealed record AuditScheduleQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AuditSchedule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditSchedule> Apply(List<AuditSchedule> items)
    {
        var matched = Collections.Filter(items, (AuditSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit schedule store.</summary>
public static class AuditScheduleStore
{
    public static IAuditScheduleStore NewMemoryAuditScheduleStore()
    {
        return new MemoryAuditScheduleStore(new Dictionary<string, AuditSchedule>(), new List<string>());
    }

    public static IAuditScheduleStore NewCachedAuditScheduleStore(MemoryAuditScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedAuditScheduleStore(inner, AuditScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AuditSchedule> FindAuditSchedules(IAuditScheduleStore store, AuditScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
