namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient benchmark audits by code.</summary>
public interface ILenientBenchmarkAuditStore
{
    LenientBenchmarkAudit? Find(string code);
    void Save(LenientBenchmarkAudit item);
    bool Remove(string code);
    List<LenientBenchmarkAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBenchmarkAuditStore(Dictionary<string, LenientBenchmarkAudit> items, List<string> order) : ILenientBenchmarkAuditStore
{
    public Dictionary<string, LenientBenchmarkAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBenchmarkAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBenchmarkAudit item)
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

    public List<LenientBenchmarkAudit> List()
    {
        var outValue = new List<LenientBenchmarkAudit>();
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
public sealed class CachedLenientBenchmarkAuditStore(MemoryLenientBenchmarkAuditStore inner, LenientBenchmarkAuditCache cache, int hits, int misses) : ILenientBenchmarkAuditStore
{
    public MemoryLenientBenchmarkAuditStore Inner { get; set; } = inner;
    public LenientBenchmarkAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBenchmarkAudit? Find(string code)
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

    public void Save(LenientBenchmarkAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBenchmarkAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient benchmark audits. Empty fields match all.</summary>
public sealed record LenientBenchmarkAuditQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBenchmarkAudit item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBenchmarkAudit> Apply(List<LenientBenchmarkAudit> items)
    {
        var matched = Collections.Filter(items, (LenientBenchmarkAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient benchmark audit store.</summary>
public static class LenientBenchmarkAuditStore
{
    public static ILenientBenchmarkAuditStore NewMemoryLenientBenchmarkAuditStore()
    {
        return new MemoryLenientBenchmarkAuditStore(
            new Dictionary<string, LenientBenchmarkAudit>(),
            new List<string>());
    }

    public static ILenientBenchmarkAuditStore NewCachedLenientBenchmarkAuditStore(MemoryLenientBenchmarkAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBenchmarkAuditStore(inner, LenientBenchmarkAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientBenchmarkAudit> FindLenientBenchmarkAudits(ILenientBenchmarkAuditStore store, LenientBenchmarkAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
