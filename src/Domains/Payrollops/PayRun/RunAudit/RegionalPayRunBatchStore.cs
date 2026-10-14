namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pay run batches by code.</summary>
public interface IRegionalPayRunBatchStore
{
    RegionalPayRunBatch? Find(string code);
    void Save(RegionalPayRunBatch item);
    bool Remove(string code);
    List<RegionalPayRunBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayRunBatchStore(Dictionary<string, RegionalPayRunBatch> items, List<string> order) : IRegionalPayRunBatchStore
{
    public Dictionary<string, RegionalPayRunBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayRunBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayRunBatch item)
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

    public List<RegionalPayRunBatch> List()
    {
        var outValue = new List<RegionalPayRunBatch>();
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
public sealed class CachedRegionalPayRunBatchStore(MemoryRegionalPayRunBatchStore inner, RegionalPayRunBatchCache cache, int hits, int misses) : IRegionalPayRunBatchStore
{
    public MemoryRegionalPayRunBatchStore Inner { get; set; } = inner;
    public RegionalPayRunBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayRunBatch? Find(string code)
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

    public void Save(RegionalPayRunBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayRunBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pay run batches. Empty fields match all.</summary>
public sealed record RegionalPayRunBatchQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayRunBatch item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPayRunBatch> Apply(List<RegionalPayRunBatch> items)
    {
        var matched = Collections.Filter(items, (RegionalPayRunBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pay run batch store.</summary>
public static class RegionalPayRunBatchStore
{
    public static IRegionalPayRunBatchStore NewMemoryRegionalPayRunBatchStore()
    {
        return new MemoryRegionalPayRunBatchStore(new Dictionary<string, RegionalPayRunBatch>(), new List<string>());
    }

    public static IRegionalPayRunBatchStore NewCachedRegionalPayRunBatchStore(MemoryRegionalPayRunBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayRunBatchStore(inner, RegionalPayRunBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalPayRunBatch> FindRegionalPayRunBatches(IRegionalPayRunBatchStore store, RegionalPayRunBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
