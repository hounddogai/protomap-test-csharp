namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global assignment reservations by code.</summary>
public interface IGlobalAssignmentReservationStore
{
    GlobalAssignmentReservation? Find(string code);
    void Save(GlobalAssignmentReservation item);
    bool Remove(string code);
    List<GlobalAssignmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAssignmentReservationStore(Dictionary<string, GlobalAssignmentReservation> items, List<string> order) : IGlobalAssignmentReservationStore
{
    public Dictionary<string, GlobalAssignmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAssignmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAssignmentReservation item)
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

    public List<GlobalAssignmentReservation> List()
    {
        var outValue = new List<GlobalAssignmentReservation>();
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
public sealed class CachedGlobalAssignmentReservationStore(MemoryGlobalAssignmentReservationStore inner, GlobalAssignmentReservationCache cache, int hits, int misses) : IGlobalAssignmentReservationStore
{
    public MemoryGlobalAssignmentReservationStore Inner { get; set; } = inner;
    public GlobalAssignmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAssignmentReservation? Find(string code)
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

    public void Save(GlobalAssignmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAssignmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global assignment reservations. Empty fields match all.</summary>
public sealed record GlobalAssignmentReservationQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAssignmentReservation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAssignmentReservation> Apply(List<GlobalAssignmentReservation> items)
    {
        var matched = Collections.Filter(items, (GlobalAssignmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global assignment reservation store.</summary>
public static class GlobalAssignmentReservationStore
{
    public static IGlobalAssignmentReservationStore NewMemoryGlobalAssignmentReservationStore()
    {
        return new MemoryGlobalAssignmentReservationStore(
            new Dictionary<string, GlobalAssignmentReservation>(),
            new List<string>());
    }

    public static IGlobalAssignmentReservationStore NewCachedGlobalAssignmentReservationStore(MemoryGlobalAssignmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAssignmentReservationStore(
            inner,
            GlobalAssignmentReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAssignmentReservation> FindGlobalAssignmentReservations(IGlobalAssignmentReservationStore store, GlobalAssignmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
