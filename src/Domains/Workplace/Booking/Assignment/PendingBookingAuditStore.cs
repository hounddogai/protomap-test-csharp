namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending booking audits by code.</summary>
public interface IPendingBookingAuditStore
{
    PendingBookingAudit? Find(string code);
    void Save(PendingBookingAudit item);
    bool Remove(string code);
    List<PendingBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBookingAuditStore(Dictionary<string, PendingBookingAudit> items, List<string> order) : IPendingBookingAuditStore
{
    public Dictionary<string, PendingBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBookingAudit item)
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

    public List<PendingBookingAudit> List()
    {
        var outValue = new List<PendingBookingAudit>();
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
public sealed class CachedPendingBookingAuditStore(MemoryPendingBookingAuditStore inner, PendingBookingAuditCache cache, int hits, int misses) : IPendingBookingAuditStore
{
    public MemoryPendingBookingAuditStore Inner { get; set; } = inner;
    public PendingBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBookingAudit? Find(string code)
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

    public void Save(PendingBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending booking audits. Empty fields match all.</summary>
public sealed record PendingBookingAuditQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBookingAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PendingBookingAudit> Apply(List<PendingBookingAudit> items)
    {
        var matched = Collections.Filter(items, (PendingBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending booking audit store.</summary>
public static class PendingBookingAuditStore
{
    public static IPendingBookingAuditStore NewMemoryPendingBookingAuditStore()
    {
        return new MemoryPendingBookingAuditStore(new Dictionary<string, PendingBookingAudit>(), new List<string>());
    }

    public static IPendingBookingAuditStore NewCachedPendingBookingAuditStore(MemoryPendingBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBookingAuditStore(inner, PendingBookingAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingBookingAudit> FindPendingBookingAudits(IPendingBookingAuditStore store, PendingBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
