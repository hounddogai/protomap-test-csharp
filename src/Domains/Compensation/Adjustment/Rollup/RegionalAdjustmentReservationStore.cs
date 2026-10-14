namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional adjustment reservations by code.</summary>
public interface IRegionalAdjustmentReservationStore
{
    RegionalAdjustmentReservation? Find(string code);
    void Save(RegionalAdjustmentReservation item);
    bool Remove(string code);
    List<RegionalAdjustmentReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAdjustmentReservationStore(Dictionary<string, RegionalAdjustmentReservation> items, List<string> order) : IRegionalAdjustmentReservationStore
{
    public Dictionary<string, RegionalAdjustmentReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAdjustmentReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAdjustmentReservation item)
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

    public List<RegionalAdjustmentReservation> List()
    {
        var outValue = new List<RegionalAdjustmentReservation>();
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
public sealed class CachedRegionalAdjustmentReservationStore(MemoryRegionalAdjustmentReservationStore inner, RegionalAdjustmentReservationCache cache, int hits, int misses) : IRegionalAdjustmentReservationStore
{
    public MemoryRegionalAdjustmentReservationStore Inner { get; set; } = inner;
    public RegionalAdjustmentReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAdjustmentReservation? Find(string code)
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

    public void Save(RegionalAdjustmentReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAdjustmentReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional adjustment reservations. Empty fields match all.</summary>
public sealed record RegionalAdjustmentReservationQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAdjustmentReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<RegionalAdjustmentReservation> Apply(List<RegionalAdjustmentReservation> items)
    {
        var matched = Collections.Filter(items, (RegionalAdjustmentReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional adjustment reservation store.</summary>
public static class RegionalAdjustmentReservationStore
{
    public static IRegionalAdjustmentReservationStore NewMemoryRegionalAdjustmentReservationStore()
    {
        return new MemoryRegionalAdjustmentReservationStore(
            new Dictionary<string, RegionalAdjustmentReservation>(),
            new List<string>());
    }

    public static IRegionalAdjustmentReservationStore NewCachedRegionalAdjustmentReservationStore(MemoryRegionalAdjustmentReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAdjustmentReservationStore(
            inner,
            RegionalAdjustmentReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAdjustmentReservation> FindRegionalAdjustmentReservations(IRegionalAdjustmentReservationStore store, RegionalAdjustmentReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
