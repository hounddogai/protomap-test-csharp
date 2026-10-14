namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual floor plan reservations by code.</summary>
public interface IManualFloorPlanReservationStore
{
    ManualFloorPlanReservation? Find(string code);
    void Save(ManualFloorPlanReservation item);
    bool Remove(string code);
    List<ManualFloorPlanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualFloorPlanReservationStore(Dictionary<string, ManualFloorPlanReservation> items, List<string> order) : IManualFloorPlanReservationStore
{
    public Dictionary<string, ManualFloorPlanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualFloorPlanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualFloorPlanReservation item)
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

    public List<ManualFloorPlanReservation> List()
    {
        var outValue = new List<ManualFloorPlanReservation>();
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
public sealed class CachedManualFloorPlanReservationStore(MemoryManualFloorPlanReservationStore inner, ManualFloorPlanReservationCache cache, int hits, int misses) : IManualFloorPlanReservationStore
{
    public MemoryManualFloorPlanReservationStore Inner { get; set; } = inner;
    public ManualFloorPlanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualFloorPlanReservation? Find(string code)
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

    public void Save(ManualFloorPlanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualFloorPlanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual floor plan reservations. Empty fields match all.</summary>
public sealed record ManualFloorPlanReservationQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ManualFloorPlanReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualFloorPlanReservation> Apply(List<ManualFloorPlanReservation> items)
    {
        var matched = Collections.Filter(items, (ManualFloorPlanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual floor plan reservation store.</summary>
public static class ManualFloorPlanReservationStore
{
    public static IManualFloorPlanReservationStore NewMemoryManualFloorPlanReservationStore()
    {
        return new MemoryManualFloorPlanReservationStore(
            new Dictionary<string, ManualFloorPlanReservation>(),
            new List<string>());
    }

    public static IManualFloorPlanReservationStore NewCachedManualFloorPlanReservationStore(MemoryManualFloorPlanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedManualFloorPlanReservationStore(
            inner,
            ManualFloorPlanReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualFloorPlanReservation> FindManualFloorPlanReservations(IManualFloorPlanReservationStore store, ManualFloorPlanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
