namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal payslip snapshots by code.</summary>
public interface ISeasonalPayslipSnapshotStore
{
    SeasonalPayslipSnapshot? Find(string code);
    void Save(SeasonalPayslipSnapshot item);
    bool Remove(string code);
    List<SeasonalPayslipSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayslipSnapshotStore(Dictionary<string, SeasonalPayslipSnapshot> items, List<string> order) : ISeasonalPayslipSnapshotStore
{
    public Dictionary<string, SeasonalPayslipSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayslipSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayslipSnapshot item)
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

    public List<SeasonalPayslipSnapshot> List()
    {
        var outValue = new List<SeasonalPayslipSnapshot>();
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
public sealed class CachedSeasonalPayslipSnapshotStore(MemorySeasonalPayslipSnapshotStore inner, SeasonalPayslipSnapshotCache cache, int hits, int misses) : ISeasonalPayslipSnapshotStore
{
    public MemorySeasonalPayslipSnapshotStore Inner { get; set; } = inner;
    public SeasonalPayslipSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayslipSnapshot? Find(string code)
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

    public void Save(SeasonalPayslipSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayslipSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal payslip snapshots. Empty fields match all.</summary>
public sealed record SeasonalPayslipSnapshotQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayslipSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPayslipSnapshot> Apply(List<SeasonalPayslipSnapshot> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayslipSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal payslip snapshot store.</summary>
public static class SeasonalPayslipSnapshotStore
{
    public static ISeasonalPayslipSnapshotStore NewMemorySeasonalPayslipSnapshotStore()
    {
        return new MemorySeasonalPayslipSnapshotStore(
            new Dictionary<string, SeasonalPayslipSnapshot>(),
            new List<string>());
    }

    public static ISeasonalPayslipSnapshotStore NewCachedSeasonalPayslipSnapshotStore(MemorySeasonalPayslipSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayslipSnapshotStore(
            inner,
            SeasonalPayslipSnapshotCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPayslipSnapshot> FindSeasonalPayslipSnapshots(ISeasonalPayslipSnapshotStore store, SeasonalPayslipSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
