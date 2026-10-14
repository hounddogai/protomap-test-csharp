namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered booking assignments by code.</summary>
public interface ITieredBookingAssignmentStore
{
    TieredBookingAssignment? Find(string code);
    void Save(TieredBookingAssignment item);
    bool Remove(string code);
    List<TieredBookingAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBookingAssignmentStore(Dictionary<string, TieredBookingAssignment> items, List<string> order) : ITieredBookingAssignmentStore
{
    public Dictionary<string, TieredBookingAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBookingAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBookingAssignment item)
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

    public List<TieredBookingAssignment> List()
    {
        var outValue = new List<TieredBookingAssignment>();
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
public sealed class CachedTieredBookingAssignmentStore(MemoryTieredBookingAssignmentStore inner, TieredBookingAssignmentCache cache, int hits, int misses) : ITieredBookingAssignmentStore
{
    public MemoryTieredBookingAssignmentStore Inner { get; set; } = inner;
    public TieredBookingAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBookingAssignment? Find(string code)
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

    public void Save(TieredBookingAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBookingAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered booking assignments. Empty fields match all.</summary>
public sealed record TieredBookingAssignmentQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBookingAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBookingAssignment> Apply(List<TieredBookingAssignment> items)
    {
        var matched = Collections.Filter(items, (TieredBookingAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered booking assignment store.</summary>
public static class TieredBookingAssignmentStore
{
    public static ITieredBookingAssignmentStore NewMemoryTieredBookingAssignmentStore()
    {
        return new MemoryTieredBookingAssignmentStore(
            new Dictionary<string, TieredBookingAssignment>(),
            new List<string>());
    }

    public static ITieredBookingAssignmentStore NewCachedTieredBookingAssignmentStore(MemoryTieredBookingAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBookingAssignmentStore(
            inner,
            TieredBookingAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredBookingAssignment> FindTieredBookingAssignments(ITieredBookingAssignmentStore store, TieredBookingAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
