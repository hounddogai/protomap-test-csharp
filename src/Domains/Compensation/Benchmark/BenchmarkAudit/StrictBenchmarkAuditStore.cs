namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict benchmark audits by code.</summary>
public interface IStrictBenchmarkAuditStore
{
    StrictBenchmarkAudit? Find(string code);
    void Save(StrictBenchmarkAudit item);
    bool Remove(string code);
    List<StrictBenchmarkAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictBenchmarkAuditStore(Dictionary<string, StrictBenchmarkAudit> items, List<string> order) : IStrictBenchmarkAuditStore
{
    public Dictionary<string, StrictBenchmarkAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictBenchmarkAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictBenchmarkAudit item)
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

    public List<StrictBenchmarkAudit> List()
    {
        var outValue = new List<StrictBenchmarkAudit>();
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
public sealed class CachedStrictBenchmarkAuditStore(MemoryStrictBenchmarkAuditStore inner, StrictBenchmarkAuditCache cache, int hits, int misses) : IStrictBenchmarkAuditStore
{
    public MemoryStrictBenchmarkAuditStore Inner { get; set; } = inner;
    public StrictBenchmarkAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictBenchmarkAudit? Find(string code)
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

    public void Save(StrictBenchmarkAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictBenchmarkAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict benchmark audits. Empty fields match all.</summary>
public sealed record StrictBenchmarkAuditQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(StrictBenchmarkAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictBenchmarkAudit> Apply(List<StrictBenchmarkAudit> items)
    {
        var matched = Collections.Filter(items, (StrictBenchmarkAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict benchmark audit store.</summary>
public static class StrictBenchmarkAuditStore
{
    public static IStrictBenchmarkAuditStore NewMemoryStrictBenchmarkAuditStore()
    {
        return new MemoryStrictBenchmarkAuditStore(
            new Dictionary<string, StrictBenchmarkAudit>(),
            new List<string>());
    }

    public static IStrictBenchmarkAuditStore NewCachedStrictBenchmarkAuditStore(MemoryStrictBenchmarkAuditStore inner, int ttl, IClock clock)
    {
        return new CachedStrictBenchmarkAuditStore(inner, StrictBenchmarkAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictBenchmarkAudit> FindStrictBenchmarkAudits(IStrictBenchmarkAuditStore store, StrictBenchmarkAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
