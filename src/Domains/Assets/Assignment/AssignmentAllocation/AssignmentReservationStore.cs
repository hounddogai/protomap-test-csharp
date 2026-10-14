namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment reservations by code.</summary>
public interface IAssignmentReservationStore
{
    AssignmentReservation? Find(string code);
    void Save(AssignmentReservation item);
    bool Remove(string code);
    List<AssignmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentReservationStore(Dictionary<string, AssignmentReservation> items, List<string> order) : IAssignmentReservationStore
{
    public Dictionary<string, AssignmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentReservation item)
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

    public List<AssignmentReservation> List()
    {
        var outValue = new List<AssignmentReservation>();
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
public sealed class CachedAssignmentReservationStore(MemoryAssignmentReservationStore inner, AssignmentReservationCache cache, int hits, int misses) : IAssignmentReservationStore
{
    public MemoryAssignmentReservationStore Inner { get; set; } = inner;
    public AssignmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentReservation? Find(string code)
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

    public void Save(AssignmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment reservations. Empty fields match all.</summary>
public sealed record AssignmentReservationQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentReservation> Apply(List<AssignmentReservation> items)
    {
        var matched = Collections.Filter(items, (AssignmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment reservation store.</summary>
public static class AssignmentReservationStore
{
    public static IAssignmentReservationStore NewMemoryAssignmentReservationStore()
    {
        return new MemoryAssignmentReservationStore(
            new Dictionary<string, AssignmentReservation>(),
            new List<string>());
    }

    public static IAssignmentReservationStore NewCachedAssignmentReservationStore(MemoryAssignmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentReservationStore(inner, AssignmentReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssignmentReservation> FindAssignmentReservations(IAssignmentReservationStore store, AssignmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
