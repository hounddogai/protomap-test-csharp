namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip snapshots by code.</summary>
public interface IPayslipSnapshotStore
{
    PayslipSnapshot? Find(string code);
    void Save(PayslipSnapshot item);
    bool Remove(string code);
    List<PayslipSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipSnapshotStore(Dictionary<string, PayslipSnapshot> items, List<string> order) : IPayslipSnapshotStore
{
    public Dictionary<string, PayslipSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipSnapshot item)
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

    public List<PayslipSnapshot> List()
    {
        var outValue = new List<PayslipSnapshot>();
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
public sealed class CachedPayslipSnapshotStore(MemoryPayslipSnapshotStore inner, PayslipSnapshotCache cache, int hits, int misses) : IPayslipSnapshotStore
{
    public MemoryPayslipSnapshotStore Inner { get; set; } = inner;
    public PayslipSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipSnapshot? Find(string code)
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

    public void Save(PayslipSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip snapshots. Empty fields match all.</summary>
public sealed record PayslipSnapshotQuery(string Department, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipSnapshot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<PayslipSnapshot> Apply(List<PayslipSnapshot> items)
    {
        var matched = Collections.Filter(items, (PayslipSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip snapshot store.</summary>
public static class PayslipSnapshotStore
{
    public static IPayslipSnapshotStore NewMemoryPayslipSnapshotStore()
    {
        return new MemoryPayslipSnapshotStore(new Dictionary<string, PayslipSnapshot>(), new List<string>());
    }

    public static IPayslipSnapshotStore NewCachedPayslipSnapshotStore(MemoryPayslipSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipSnapshotStore(inner, PayslipSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipSnapshot> FindPayslipSnapshots(IPayslipSnapshotStore store, PayslipSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
