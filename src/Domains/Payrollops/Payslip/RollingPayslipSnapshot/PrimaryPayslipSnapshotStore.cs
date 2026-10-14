namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary payslip snapshots by code.</summary>
public interface IPrimaryPayslipSnapshotStore
{
    PrimaryPayslipSnapshot? Find(string code);
    void Save(PrimaryPayslipSnapshot item);
    bool Remove(string code);
    List<PrimaryPayslipSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPayslipSnapshotStore(Dictionary<string, PrimaryPayslipSnapshot> items, List<string> order) : IPrimaryPayslipSnapshotStore
{
    public Dictionary<string, PrimaryPayslipSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPayslipSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPayslipSnapshot item)
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

    public List<PrimaryPayslipSnapshot> List()
    {
        var outValue = new List<PrimaryPayslipSnapshot>();
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
public sealed class CachedPrimaryPayslipSnapshotStore(MemoryPrimaryPayslipSnapshotStore inner, PrimaryPayslipSnapshotCache cache, int hits, int misses) : IPrimaryPayslipSnapshotStore
{
    public MemoryPrimaryPayslipSnapshotStore Inner { get; set; } = inner;
    public PrimaryPayslipSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPayslipSnapshot? Find(string code)
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

    public void Save(PrimaryPayslipSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPayslipSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary payslip snapshots. Empty fields match all.</summary>
public sealed record PrimaryPayslipSnapshotQuery(string Team, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPayslipSnapshot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPayslipSnapshot> Apply(List<PrimaryPayslipSnapshot> items)
    {
        var matched = Collections.Filter(items, (PrimaryPayslipSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary payslip snapshot store.</summary>
public static class PrimaryPayslipSnapshotStore
{
    public static IPrimaryPayslipSnapshotStore NewMemoryPrimaryPayslipSnapshotStore()
    {
        return new MemoryPrimaryPayslipSnapshotStore(
            new Dictionary<string, PrimaryPayslipSnapshot>(),
            new List<string>());
    }

    public static IPrimaryPayslipSnapshotStore NewCachedPrimaryPayslipSnapshotStore(MemoryPrimaryPayslipSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPayslipSnapshotStore(
            inner,
            PrimaryPayslipSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPayslipSnapshot> FindPrimaryPayslipSnapshots(IPrimaryPayslipSnapshotStore store, PrimaryPayslipSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
