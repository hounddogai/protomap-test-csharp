namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional deduction batches by code.</summary>
public interface IRegionalDeductionBatchStore
{
    RegionalDeductionBatch? Find(string code);
    void Save(RegionalDeductionBatch item);
    bool Remove(string code);
    List<RegionalDeductionBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalDeductionBatchStore(Dictionary<string, RegionalDeductionBatch> items, List<string> order) : IRegionalDeductionBatchStore
{
    public Dictionary<string, RegionalDeductionBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalDeductionBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalDeductionBatch item)
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

    public List<RegionalDeductionBatch> List()
    {
        var outValue = new List<RegionalDeductionBatch>();
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
public sealed class CachedRegionalDeductionBatchStore(MemoryRegionalDeductionBatchStore inner, RegionalDeductionBatchCache cache, int hits, int misses) : IRegionalDeductionBatchStore
{
    public MemoryRegionalDeductionBatchStore Inner { get; set; } = inner;
    public RegionalDeductionBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalDeductionBatch? Find(string code)
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

    public void Save(RegionalDeductionBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalDeductionBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional deduction batches. Empty fields match all.</summary>
public sealed record RegionalDeductionBatchQuery(string Source, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalDeductionBatch item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalDeductionBatch> Apply(List<RegionalDeductionBatch> items)
    {
        var matched = Collections.Filter(items, (RegionalDeductionBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional deduction batch store.</summary>
public static class RegionalDeductionBatchStore
{
    public static IRegionalDeductionBatchStore NewMemoryRegionalDeductionBatchStore()
    {
        return new MemoryRegionalDeductionBatchStore(
            new Dictionary<string, RegionalDeductionBatch>(),
            new List<string>());
    }

    public static IRegionalDeductionBatchStore NewCachedRegionalDeductionBatchStore(MemoryRegionalDeductionBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalDeductionBatchStore(
            inner,
            RegionalDeductionBatchCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalDeductionBatch> FindRegionalDeductionBatches(IRegionalDeductionBatchStore store, RegionalDeductionBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
