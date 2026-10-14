namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit reservations by code.</summary>
public interface IAuditReservationStore
{
    AuditReservation? Find(string code);
    void Save(AuditReservation item);
    bool Remove(string code);
    List<AuditReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditReservationStore(Dictionary<string, AuditReservation> items, List<string> order) : IAuditReservationStore
{
    public Dictionary<string, AuditReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditReservation item)
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

    public List<AuditReservation> List()
    {
        var outValue = new List<AuditReservation>();
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
public sealed class CachedAuditReservationStore(MemoryAuditReservationStore inner, AuditReservationCache cache, int hits, int misses) : IAuditReservationStore
{
    public MemoryAuditReservationStore Inner { get; set; } = inner;
    public AuditReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditReservation? Find(string code)
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

    public void Save(AuditReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit reservations. Empty fields match all.</summary>
public sealed record AuditReservationQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AuditReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditReservation> Apply(List<AuditReservation> items)
    {
        var matched = Collections.Filter(items, (AuditReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit reservation store.</summary>
public static class AuditReservationStore
{
    public static IAuditReservationStore NewMemoryAuditReservationStore()
    {
        return new MemoryAuditReservationStore(new Dictionary<string, AuditReservation>(), new List<string>());
    }

    public static IAuditReservationStore NewCachedAuditReservationStore(MemoryAuditReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAuditReservationStore(inner, AuditReservationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditReservation> FindAuditReservations(IAuditReservationStore store, AuditReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
