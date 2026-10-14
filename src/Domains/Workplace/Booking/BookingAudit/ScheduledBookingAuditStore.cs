namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled booking audits by code.</summary>
public interface IScheduledBookingAuditStore
{
    ScheduledBookingAudit? Find(string code);
    void Save(ScheduledBookingAudit item);
    bool Remove(string code);
    List<ScheduledBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBookingAuditStore(Dictionary<string, ScheduledBookingAudit> items, List<string> order) : IScheduledBookingAuditStore
{
    public Dictionary<string, ScheduledBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBookingAudit item)
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

    public List<ScheduledBookingAudit> List()
    {
        var outValue = new List<ScheduledBookingAudit>();
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
public sealed class CachedScheduledBookingAuditStore(MemoryScheduledBookingAuditStore inner, ScheduledBookingAuditCache cache, int hits, int misses) : IScheduledBookingAuditStore
{
    public MemoryScheduledBookingAuditStore Inner { get; set; } = inner;
    public ScheduledBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBookingAudit? Find(string code)
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

    public void Save(ScheduledBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled booking audits. Empty fields match all.</summary>
public sealed record ScheduledBookingAuditQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBookingAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBookingAudit> Apply(List<ScheduledBookingAudit> items)
    {
        var matched = Collections.Filter(items, (ScheduledBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled booking audit store.</summary>
public static class ScheduledBookingAuditStore
{
    public static IScheduledBookingAuditStore NewMemoryScheduledBookingAuditStore()
    {
        return new MemoryScheduledBookingAuditStore(
            new Dictionary<string, ScheduledBookingAudit>(),
            new List<string>());
    }

    public static IScheduledBookingAuditStore NewCachedScheduledBookingAuditStore(MemoryScheduledBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBookingAuditStore(inner, ScheduledBookingAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScheduledBookingAudit> FindScheduledBookingAudits(IScheduledBookingAuditStore store, ScheduledBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
