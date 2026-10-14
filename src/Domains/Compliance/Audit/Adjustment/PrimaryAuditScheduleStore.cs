namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary audit schedules by code.</summary>
public interface IPrimaryAuditScheduleStore
{
    PrimaryAuditSchedule? Find(string code);
    void Save(PrimaryAuditSchedule item);
    bool Remove(string code);
    List<PrimaryAuditSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAuditScheduleStore(Dictionary<string, PrimaryAuditSchedule> items, List<string> order) : IPrimaryAuditScheduleStore
{
    public Dictionary<string, PrimaryAuditSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAuditSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAuditSchedule item)
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

    public List<PrimaryAuditSchedule> List()
    {
        var outValue = new List<PrimaryAuditSchedule>();
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
public sealed class CachedPrimaryAuditScheduleStore(MemoryPrimaryAuditScheduleStore inner, PrimaryAuditScheduleCache cache, int hits, int misses) : IPrimaryAuditScheduleStore
{
    public MemoryPrimaryAuditScheduleStore Inner { get; set; } = inner;
    public PrimaryAuditScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAuditSchedule? Find(string code)
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

    public void Save(PrimaryAuditSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAuditSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary audit schedules. Empty fields match all.</summary>
public sealed record PrimaryAuditScheduleQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAuditSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAuditSchedule> Apply(List<PrimaryAuditSchedule> items)
    {
        var matched = Collections.Filter(items, (PrimaryAuditSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary audit schedule store.</summary>
public static class PrimaryAuditScheduleStore
{
    public static IPrimaryAuditScheduleStore NewMemoryPrimaryAuditScheduleStore()
    {
        return new MemoryPrimaryAuditScheduleStore(
            new Dictionary<string, PrimaryAuditSchedule>(),
            new List<string>());
    }

    public static IPrimaryAuditScheduleStore NewCachedPrimaryAuditScheduleStore(MemoryPrimaryAuditScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAuditScheduleStore(inner, PrimaryAuditScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryAuditSchedule> FindPrimaryAuditSchedules(IPrimaryAuditScheduleStore store, PrimaryAuditScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
