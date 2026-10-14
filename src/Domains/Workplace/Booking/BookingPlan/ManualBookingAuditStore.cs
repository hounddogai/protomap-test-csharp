namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual booking audits by code.</summary>
public interface IManualBookingAuditStore
{
    ManualBookingAudit? Find(string code);
    void Save(ManualBookingAudit item);
    bool Remove(string code);
    List<ManualBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBookingAuditStore(Dictionary<string, ManualBookingAudit> items, List<string> order) : IManualBookingAuditStore
{
    public Dictionary<string, ManualBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBookingAudit item)
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

    public List<ManualBookingAudit> List()
    {
        var outValue = new List<ManualBookingAudit>();
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
public sealed class CachedManualBookingAuditStore(MemoryManualBookingAuditStore inner, ManualBookingAuditCache cache, int hits, int misses) : IManualBookingAuditStore
{
    public MemoryManualBookingAuditStore Inner { get; set; } = inner;
    public ManualBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBookingAudit? Find(string code)
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

    public void Save(ManualBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual booking audits. Empty fields match all.</summary>
public sealed record ManualBookingAuditQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBookingAudit item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBookingAudit> Apply(List<ManualBookingAudit> items)
    {
        var matched = Collections.Filter(items, (ManualBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual booking audit store.</summary>
public static class ManualBookingAuditStore
{
    public static IManualBookingAuditStore NewMemoryManualBookingAuditStore()
    {
        return new MemoryManualBookingAuditStore(new Dictionary<string, ManualBookingAudit>(), new List<string>());
    }

    public static IManualBookingAuditStore NewCachedManualBookingAuditStore(MemoryManualBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedManualBookingAuditStore(inner, ManualBookingAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualBookingAudit> FindManualBookingAudits(IManualBookingAuditStore store, ManualBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
