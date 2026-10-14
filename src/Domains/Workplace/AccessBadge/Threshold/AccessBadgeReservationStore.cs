namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge reservations by code.</summary>
public interface IAccessBadgeReservationStore
{
    AccessBadgeReservation? Find(string code);
    void Save(AccessBadgeReservation item);
    bool Remove(string code);
    List<AccessBadgeReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeReservationStore(Dictionary<string, AccessBadgeReservation> items, List<string> order) : IAccessBadgeReservationStore
{
    public Dictionary<string, AccessBadgeReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeReservation item)
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

    public List<AccessBadgeReservation> List()
    {
        var outValue = new List<AccessBadgeReservation>();
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
public sealed class CachedAccessBadgeReservationStore(MemoryAccessBadgeReservationStore inner, AccessBadgeReservationCache cache, int hits, int misses) : IAccessBadgeReservationStore
{
    public MemoryAccessBadgeReservationStore Inner { get; set; } = inner;
    public AccessBadgeReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeReservation? Find(string code)
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

    public void Save(AccessBadgeReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge reservations. Empty fields match all.</summary>
public sealed record AccessBadgeReservationQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeReservation item)
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

    public List<AccessBadgeReservation> Apply(List<AccessBadgeReservation> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge reservation store.</summary>
public static class AccessBadgeReservationStore
{
    public static IAccessBadgeReservationStore NewMemoryAccessBadgeReservationStore()
    {
        return new MemoryAccessBadgeReservationStore(
            new Dictionary<string, AccessBadgeReservation>(),
            new List<string>());
    }

    public static IAccessBadgeReservationStore NewCachedAccessBadgeReservationStore(MemoryAccessBadgeReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeReservationStore(
            inner,
            AccessBadgeReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AccessBadgeReservation> FindAccessBadgeReservations(IAccessBadgeReservationStore store, AccessBadgeReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
