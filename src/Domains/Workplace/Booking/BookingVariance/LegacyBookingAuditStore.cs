namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy booking audits by code.</summary>
public interface ILegacyBookingAuditStore
{
    LegacyBookingAudit? Find(string code);
    void Save(LegacyBookingAudit item);
    bool Remove(string code);
    List<LegacyBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyBookingAuditStore(Dictionary<string, LegacyBookingAudit> items, List<string> order) : ILegacyBookingAuditStore
{
    public Dictionary<string, LegacyBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyBookingAudit item)
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

    public List<LegacyBookingAudit> List()
    {
        var outValue = new List<LegacyBookingAudit>();
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
public sealed class CachedLegacyBookingAuditStore(MemoryLegacyBookingAuditStore inner, LegacyBookingAuditCache cache, int hits, int misses) : ILegacyBookingAuditStore
{
    public MemoryLegacyBookingAuditStore Inner { get; set; } = inner;
    public LegacyBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyBookingAudit? Find(string code)
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

    public void Save(LegacyBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy booking audits. Empty fields match all.</summary>
public sealed record LegacyBookingAuditQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyBookingAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyBookingAudit> Apply(List<LegacyBookingAudit> items)
    {
        var matched = Collections.Filter(items, (LegacyBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy booking audit store.</summary>
public static class LegacyBookingAuditStore
{
    public static ILegacyBookingAuditStore NewMemoryLegacyBookingAuditStore()
    {
        return new MemoryLegacyBookingAuditStore(new Dictionary<string, LegacyBookingAudit>(), new List<string>());
    }

    public static ILegacyBookingAuditStore NewCachedLegacyBookingAuditStore(MemoryLegacyBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyBookingAuditStore(inner, LegacyBookingAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyBookingAudit> FindLegacyBookingAudits(ILegacyBookingAuditStore store, LegacyBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
