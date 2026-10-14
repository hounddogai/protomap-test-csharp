namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan reservations by code.</summary>
public interface IFloorPlanReservationStore
{
    FloorPlanReservation? Find(string code);
    void Save(FloorPlanReservation item);
    bool Remove(string code);
    List<FloorPlanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanReservationStore(Dictionary<string, FloorPlanReservation> items, List<string> order) : IFloorPlanReservationStore
{
    public Dictionary<string, FloorPlanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanReservation item)
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

    public List<FloorPlanReservation> List()
    {
        var outValue = new List<FloorPlanReservation>();
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
public sealed class CachedFloorPlanReservationStore(MemoryFloorPlanReservationStore inner, FloorPlanReservationCache cache, int hits, int misses) : IFloorPlanReservationStore
{
    public MemoryFloorPlanReservationStore Inner { get; set; } = inner;
    public FloorPlanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanReservation? Find(string code)
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

    public void Save(FloorPlanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan reservations. Empty fields match all.</summary>
public sealed record FloorPlanReservationQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanReservation> Apply(List<FloorPlanReservation> items)
    {
        var matched = Collections.Filter(items, (FloorPlanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan reservation store.</summary>
public static class FloorPlanReservationStore
{
    public static IFloorPlanReservationStore NewMemoryFloorPlanReservationStore()
    {
        return new MemoryFloorPlanReservationStore(
            new Dictionary<string, FloorPlanReservation>(),
            new List<string>());
    }

    public static IFloorPlanReservationStore NewCachedFloorPlanReservationStore(MemoryFloorPlanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanReservationStore(inner, FloorPlanReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanReservation> FindFloorPlanReservations(IFloorPlanReservationStore store, FloorPlanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
