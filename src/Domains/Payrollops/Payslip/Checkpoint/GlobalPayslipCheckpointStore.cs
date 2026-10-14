namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global payslip checkpoints by code.</summary>
public interface IGlobalPayslipCheckpointStore
{
    GlobalPayslipCheckpoint? Find(string code);
    void Save(GlobalPayslipCheckpoint item);
    bool Remove(string code);
    List<GlobalPayslipCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPayslipCheckpointStore(Dictionary<string, GlobalPayslipCheckpoint> items, List<string> order) : IGlobalPayslipCheckpointStore
{
    public Dictionary<string, GlobalPayslipCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPayslipCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPayslipCheckpoint item)
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

    public List<GlobalPayslipCheckpoint> List()
    {
        var outValue = new List<GlobalPayslipCheckpoint>();
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
public sealed class CachedGlobalPayslipCheckpointStore(MemoryGlobalPayslipCheckpointStore inner, GlobalPayslipCheckpointCache cache, int hits, int misses) : IGlobalPayslipCheckpointStore
{
    public MemoryGlobalPayslipCheckpointStore Inner { get; set; } = inner;
    public GlobalPayslipCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPayslipCheckpoint? Find(string code)
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

    public void Save(GlobalPayslipCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPayslipCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global payslip checkpoints. Empty fields match all.</summary>
public sealed record GlobalPayslipCheckpointQuery(string Country, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPayslipCheckpoint item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPayslipCheckpoint> Apply(List<GlobalPayslipCheckpoint> items)
    {
        var matched = Collections.Filter(items, (GlobalPayslipCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global payslip checkpoint store.</summary>
public static class GlobalPayslipCheckpointStore
{
    public static IGlobalPayslipCheckpointStore NewMemoryGlobalPayslipCheckpointStore()
    {
        return new MemoryGlobalPayslipCheckpointStore(
            new Dictionary<string, GlobalPayslipCheckpoint>(),
            new List<string>());
    }

    public static IGlobalPayslipCheckpointStore NewCachedGlobalPayslipCheckpointStore(MemoryGlobalPayslipCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPayslipCheckpointStore(
            inner,
            GlobalPayslipCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPayslipCheckpoint> FindGlobalPayslipCheckpoints(IGlobalPayslipCheckpointStore store, GlobalPayslipCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
