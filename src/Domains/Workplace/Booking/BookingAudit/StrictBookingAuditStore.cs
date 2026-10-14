namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict booking audits by code.</summary>
public interface IStrictBookingAuditStore
{
    StrictBookingAudit? Find(string code);
    void Save(StrictBookingAudit item);
    bool Remove(string code);
    List<StrictBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictBookingAuditStore(Dictionary<string, StrictBookingAudit> items, List<string> order) : IStrictBookingAuditStore
{
    public Dictionary<string, StrictBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictBookingAudit item)
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

    public List<StrictBookingAudit> List()
    {
        var outValue = new List<StrictBookingAudit>();
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
public sealed class CachedStrictBookingAuditStore(MemoryStrictBookingAuditStore inner, StrictBookingAuditCache cache, int hits, int misses) : IStrictBookingAuditStore
{
    public MemoryStrictBookingAuditStore Inner { get; set; } = inner;
    public StrictBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictBookingAudit? Find(string code)
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

    public void Save(StrictBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict booking audits. Empty fields match all.</summary>
public sealed record StrictBookingAuditQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(StrictBookingAudit item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictBookingAudit> Apply(List<StrictBookingAudit> items)
    {
        var matched = Collections.Filter(items, (StrictBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict booking audit store.</summary>
public static class StrictBookingAuditStore
{
    public static IStrictBookingAuditStore NewMemoryStrictBookingAuditStore()
    {
        return new MemoryStrictBookingAuditStore(new Dictionary<string, StrictBookingAudit>(), new List<string>());
    }

    public static IStrictBookingAuditStore NewCachedStrictBookingAuditStore(MemoryStrictBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedStrictBookingAuditStore(inner, StrictBookingAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictBookingAudit> FindStrictBookingAudits(IStrictBookingAuditStore store, StrictBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
