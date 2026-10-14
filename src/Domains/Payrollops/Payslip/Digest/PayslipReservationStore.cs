namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip reservations by code.</summary>
public interface IPayslipReservationStore
{
    PayslipReservation? Find(string code);
    void Save(PayslipReservation item);
    bool Remove(string code);
    List<PayslipReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipReservationStore(Dictionary<string, PayslipReservation> items, List<string> order) : IPayslipReservationStore
{
    public Dictionary<string, PayslipReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipReservation item)
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

    public List<PayslipReservation> List()
    {
        var outValue = new List<PayslipReservation>();
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
public sealed class CachedPayslipReservationStore(MemoryPayslipReservationStore inner, PayslipReservationCache cache, int hits, int misses) : IPayslipReservationStore
{
    public MemoryPayslipReservationStore Inner { get; set; } = inner;
    public PayslipReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipReservation? Find(string code)
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

    public void Save(PayslipReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip reservations. Empty fields match all.</summary>
public sealed record PayslipReservationQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipReservation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipReservation> Apply(List<PayslipReservation> items)
    {
        var matched = Collections.Filter(items, (PayslipReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip reservation store.</summary>
public static class PayslipReservationStore
{
    public static IPayslipReservationStore NewMemoryPayslipReservationStore()
    {
        return new MemoryPayslipReservationStore(new Dictionary<string, PayslipReservation>(), new List<string>());
    }

    public static IPayslipReservationStore NewCachedPayslipReservationStore(MemoryPayslipReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipReservationStore(inner, PayslipReservationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipReservation> FindPayslipReservations(IPayslipReservationStore store, PayslipReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
