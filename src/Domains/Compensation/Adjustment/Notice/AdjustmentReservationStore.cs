namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment reservations by code.</summary>
public interface IAdjustmentReservationStore
{
    AdjustmentReservation? Find(string code);
    void Save(AdjustmentReservation item);
    bool Remove(string code);
    List<AdjustmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentReservationStore(Dictionary<string, AdjustmentReservation> items, List<string> order) : IAdjustmentReservationStore
{
    public Dictionary<string, AdjustmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentReservation item)
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

    public List<AdjustmentReservation> List()
    {
        var outValue = new List<AdjustmentReservation>();
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
public sealed class CachedAdjustmentReservationStore(MemoryAdjustmentReservationStore inner, AdjustmentReservationCache cache, int hits, int misses) : IAdjustmentReservationStore
{
    public MemoryAdjustmentReservationStore Inner { get; set; } = inner;
    public AdjustmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentReservation? Find(string code)
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

    public void Save(AdjustmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment reservations. Empty fields match all.</summary>
public sealed record AdjustmentReservationQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentReservation> Apply(List<AdjustmentReservation> items)
    {
        var matched = Collections.Filter(items, (AdjustmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment reservation store.</summary>
public static class AdjustmentReservationStore
{
    public static IAdjustmentReservationStore NewMemoryAdjustmentReservationStore()
    {
        return new MemoryAdjustmentReservationStore(
            new Dictionary<string, AdjustmentReservation>(),
            new List<string>());
    }

    public static IAdjustmentReservationStore NewCachedAdjustmentReservationStore(MemoryAdjustmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentReservationStore(inner, AdjustmentReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdjustmentReservation> FindAdjustmentReservations(IAdjustmentReservationStore store, AdjustmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
