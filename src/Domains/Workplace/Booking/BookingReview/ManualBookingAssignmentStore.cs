namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual booking assignments by code.</summary>
public interface IManualBookingAssignmentStore
{
    ManualBookingAssignment? Find(string code);
    void Save(ManualBookingAssignment item);
    bool Remove(string code);
    List<ManualBookingAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBookingAssignmentStore(Dictionary<string, ManualBookingAssignment> items, List<string> order) : IManualBookingAssignmentStore
{
    public Dictionary<string, ManualBookingAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBookingAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBookingAssignment item)
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

    public List<ManualBookingAssignment> List()
    {
        var outValue = new List<ManualBookingAssignment>();
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
public sealed class CachedManualBookingAssignmentStore(MemoryManualBookingAssignmentStore inner, ManualBookingAssignmentCache cache, int hits, int misses) : IManualBookingAssignmentStore
{
    public MemoryManualBookingAssignmentStore Inner { get; set; } = inner;
    public ManualBookingAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBookingAssignment? Find(string code)
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

    public void Save(ManualBookingAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBookingAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual booking assignments. Empty fields match all.</summary>
public sealed record ManualBookingAssignmentQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBookingAssignment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBookingAssignment> Apply(List<ManualBookingAssignment> items)
    {
        var matched = Collections.Filter(items, (ManualBookingAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual booking assignment store.</summary>
public static class ManualBookingAssignmentStore
{
    public static IManualBookingAssignmentStore NewMemoryManualBookingAssignmentStore()
    {
        return new MemoryManualBookingAssignmentStore(
            new Dictionary<string, ManualBookingAssignment>(),
            new List<string>());
    }

    public static IManualBookingAssignmentStore NewCachedManualBookingAssignmentStore(MemoryManualBookingAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedManualBookingAssignmentStore(
            inner,
            ManualBookingAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualBookingAssignment> FindManualBookingAssignments(IManualBookingAssignmentStore store, ManualBookingAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
