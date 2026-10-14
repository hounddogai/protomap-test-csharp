namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip checkpoints by code.</summary>
public interface IPayslipCheckpointStore
{
    PayslipCheckpoint? Find(string code);
    void Save(PayslipCheckpoint item);
    bool Remove(string code);
    List<PayslipCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipCheckpointStore(Dictionary<string, PayslipCheckpoint> items, List<string> order) : IPayslipCheckpointStore
{
    public Dictionary<string, PayslipCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipCheckpoint item)
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

    public List<PayslipCheckpoint> List()
    {
        var outValue = new List<PayslipCheckpoint>();
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
public sealed class CachedPayslipCheckpointStore(MemoryPayslipCheckpointStore inner, PayslipCheckpointCache cache, int hits, int misses) : IPayslipCheckpointStore
{
    public MemoryPayslipCheckpointStore Inner { get; set; } = inner;
    public PayslipCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipCheckpoint? Find(string code)
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

    public void Save(PayslipCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip checkpoints. Empty fields match all.</summary>
public sealed record PayslipCheckpointQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipCheckpoint> Apply(List<PayslipCheckpoint> items)
    {
        var matched = Collections.Filter(items, (PayslipCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip checkpoint store.</summary>
public static class PayslipCheckpointStore
{
    public static IPayslipCheckpointStore NewMemoryPayslipCheckpointStore()
    {
        return new MemoryPayslipCheckpointStore(new Dictionary<string, PayslipCheckpoint>(), new List<string>());
    }

    public static IPayslipCheckpointStore NewCachedPayslipCheckpointStore(MemoryPayslipCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipCheckpointStore(inner, PayslipCheckpointCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayslipCheckpoint> FindPayslipCheckpoints(IPayslipCheckpointStore store, PayslipCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
