namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional payslip reservations by code.</summary>
public interface IRegionalPayslipReservationStore
{
    RegionalPayslipReservation? Find(string code);
    void Save(RegionalPayslipReservation item);
    bool Remove(string code);
    List<RegionalPayslipReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayslipReservationStore(Dictionary<string, RegionalPayslipReservation> items, List<string> order) : IRegionalPayslipReservationStore
{
    public Dictionary<string, RegionalPayslipReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayslipReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayslipReservation item)
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

    public List<RegionalPayslipReservation> List()
    {
        var outValue = new List<RegionalPayslipReservation>();
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
public sealed class CachedRegionalPayslipReservationStore(MemoryRegionalPayslipReservationStore inner, RegionalPayslipReservationCache cache, int hits, int misses) : IRegionalPayslipReservationStore
{
    public MemoryRegionalPayslipReservationStore Inner { get; set; } = inner;
    public RegionalPayslipReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayslipReservation? Find(string code)
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

    public void Save(RegionalPayslipReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayslipReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional payslip reservations. Empty fields match all.</summary>
public sealed record RegionalPayslipReservationQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayslipReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPayslipReservation> Apply(List<RegionalPayslipReservation> items)
    {
        var matched = Collections.Filter(items, (RegionalPayslipReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional payslip reservation store.</summary>
public static class RegionalPayslipReservationStore
{
    public static IRegionalPayslipReservationStore NewMemoryRegionalPayslipReservationStore()
    {
        return new MemoryRegionalPayslipReservationStore(
            new Dictionary<string, RegionalPayslipReservation>(),
            new List<string>());
    }

    public static IRegionalPayslipReservationStore NewCachedRegionalPayslipReservationStore(MemoryRegionalPayslipReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayslipReservationStore(
            inner,
            RegionalPayslipReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPayslipReservation> FindRegionalPayslipReservations(IRegionalPayslipReservationStore store, RegionalPayslipReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
