namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual mobility reservations by code.</summary>
public interface IManualMobilityReservationStore
{
    ManualMobilityReservation? Find(string code);
    void Save(ManualMobilityReservation item);
    bool Remove(string code);
    List<ManualMobilityReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualMobilityReservationStore(Dictionary<string, ManualMobilityReservation> items, List<string> order) : IManualMobilityReservationStore
{
    public Dictionary<string, ManualMobilityReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualMobilityReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualMobilityReservation item)
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

    public List<ManualMobilityReservation> List()
    {
        var outValue = new List<ManualMobilityReservation>();
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
public sealed class CachedManualMobilityReservationStore(MemoryManualMobilityReservationStore inner, ManualMobilityReservationCache cache, int hits, int misses) : IManualMobilityReservationStore
{
    public MemoryManualMobilityReservationStore Inner { get; set; } = inner;
    public ManualMobilityReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualMobilityReservation? Find(string code)
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

    public void Save(ManualMobilityReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualMobilityReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual mobility reservations. Empty fields match all.</summary>
public sealed record ManualMobilityReservationQuery(string Segment, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(ManualMobilityReservation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<ManualMobilityReservation> Apply(List<ManualMobilityReservation> items)
    {
        var matched = Collections.Filter(items, (ManualMobilityReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual mobility reservation store.</summary>
public static class ManualMobilityReservationStore
{
    public static IManualMobilityReservationStore NewMemoryManualMobilityReservationStore()
    {
        return new MemoryManualMobilityReservationStore(
            new Dictionary<string, ManualMobilityReservation>(),
            new List<string>());
    }

    public static IManualMobilityReservationStore NewCachedManualMobilityReservationStore(MemoryManualMobilityReservationStore inner, int ttl, IClock clock)
    {
        return new CachedManualMobilityReservationStore(
            inner,
            ManualMobilityReservationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualMobilityReservation> FindManualMobilityReservations(IManualMobilityReservationStore store, ManualMobilityReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
