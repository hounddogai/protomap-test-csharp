namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global deduction baselines by code.</summary>
public interface IGlobalDeductionBaselineStore
{
    GlobalDeductionBaseline? Find(string code);
    void Save(GlobalDeductionBaseline item);
    bool Remove(string code);
    List<GlobalDeductionBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalDeductionBaselineStore(Dictionary<string, GlobalDeductionBaseline> items, List<string> order) : IGlobalDeductionBaselineStore
{
    public Dictionary<string, GlobalDeductionBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalDeductionBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalDeductionBaseline item)
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

    public List<GlobalDeductionBaseline> List()
    {
        var outValue = new List<GlobalDeductionBaseline>();
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
public sealed class CachedGlobalDeductionBaselineStore(MemoryGlobalDeductionBaselineStore inner, GlobalDeductionBaselineCache cache, int hits, int misses) : IGlobalDeductionBaselineStore
{
    public MemoryGlobalDeductionBaselineStore Inner { get; set; } = inner;
    public GlobalDeductionBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalDeductionBaseline? Find(string code)
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

    public void Save(GlobalDeductionBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalDeductionBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global deduction baselines. Empty fields match all.</summary>
public sealed record GlobalDeductionBaselineQuery(string Region, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalDeductionBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalDeductionBaseline> Apply(List<GlobalDeductionBaseline> items)
    {
        var matched = Collections.Filter(items, (GlobalDeductionBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global deduction baseline store.</summary>
public static class GlobalDeductionBaselineStore
{
    public static IGlobalDeductionBaselineStore NewMemoryGlobalDeductionBaselineStore()
    {
        return new MemoryGlobalDeductionBaselineStore(
            new Dictionary<string, GlobalDeductionBaseline>(),
            new List<string>());
    }

    public static IGlobalDeductionBaselineStore NewCachedGlobalDeductionBaselineStore(MemoryGlobalDeductionBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalDeductionBaselineStore(
            inner,
            GlobalDeductionBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalDeductionBaseline> FindGlobalDeductionBaselines(IGlobalDeductionBaselineStore store, GlobalDeductionBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
