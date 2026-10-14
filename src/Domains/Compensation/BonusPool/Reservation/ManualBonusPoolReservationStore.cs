namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual bonus pool reservations by code.</summary>
public interface IManualBonusPoolReservationStore
{
    ManualBonusPoolReservation? Find(string code);
    void Save(ManualBonusPoolReservation item);
    bool Remove(string code);
    List<ManualBonusPoolReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBonusPoolReservationStore(Dictionary<string, ManualBonusPoolReservation> items, List<string> order) : IManualBonusPoolReservationStore
{
    public Dictionary<string, ManualBonusPoolReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBonusPoolReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBonusPoolReservation item)
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

    public List<ManualBonusPoolReservation> List()
    {
        var outValue = new List<ManualBonusPoolReservation>();
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
public sealed class CachedManualBonusPoolReservationStore(MemoryManualBonusPoolReservationStore inner, ManualBonusPoolReservationCache cache, int hits, int misses) : IManualBonusPoolReservationStore
{
    public MemoryManualBonusPoolReservationStore Inner { get; set; } = inner;
    public ManualBonusPoolReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBonusPoolReservation? Find(string code)
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

    public void Save(ManualBonusPoolReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBonusPoolReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual bonus pool reservations. Empty fields match all.</summary>
public sealed record ManualBonusPoolReservationQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBonusPoolReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBonusPoolReservation> Apply(List<ManualBonusPoolReservation> items)
    {
        var matched = Collections.Filter(items, (ManualBonusPoolReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual bonus pool reservation store.</summary>
public static class ManualBonusPoolReservationStore
{
    public static IManualBonusPoolReservationStore NewMemoryManualBonusPoolReservationStore()
    {
        return new MemoryManualBonusPoolReservationStore(
            new Dictionary<string, ManualBonusPoolReservation>(),
            new List<string>());
    }

    public static IManualBonusPoolReservationStore NewCachedManualBonusPoolReservationStore(MemoryManualBonusPoolReservationStore inner, int ttl, IClock clock)
    {
        return new CachedManualBonusPoolReservationStore(
            inner,
            ManualBonusPoolReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualBonusPoolReservation> FindManualBonusPoolReservations(IManualBonusPoolReservationStore store, ManualBonusPoolReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
