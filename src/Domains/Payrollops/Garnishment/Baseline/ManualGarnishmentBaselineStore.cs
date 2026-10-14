namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual garnishment baselines by code.</summary>
public interface IManualGarnishmentBaselineStore
{
    ManualGarnishmentBaseline? Find(string code);
    void Save(ManualGarnishmentBaseline item);
    bool Remove(string code);
    List<ManualGarnishmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualGarnishmentBaselineStore(Dictionary<string, ManualGarnishmentBaseline> items, List<string> order) : IManualGarnishmentBaselineStore
{
    public Dictionary<string, ManualGarnishmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualGarnishmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualGarnishmentBaseline item)
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

    public List<ManualGarnishmentBaseline> List()
    {
        var outValue = new List<ManualGarnishmentBaseline>();
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
public sealed class CachedManualGarnishmentBaselineStore(MemoryManualGarnishmentBaselineStore inner, ManualGarnishmentBaselineCache cache, int hits, int misses) : IManualGarnishmentBaselineStore
{
    public MemoryManualGarnishmentBaselineStore Inner { get; set; } = inner;
    public ManualGarnishmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualGarnishmentBaseline? Find(string code)
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

    public void Save(ManualGarnishmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualGarnishmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual garnishment baselines. Empty fields match all.</summary>
public sealed record ManualGarnishmentBaselineQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ManualGarnishmentBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualGarnishmentBaseline> Apply(List<ManualGarnishmentBaseline> items)
    {
        var matched = Collections.Filter(items, (ManualGarnishmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual garnishment baseline store.</summary>
public static class ManualGarnishmentBaselineStore
{
    public static IManualGarnishmentBaselineStore NewMemoryManualGarnishmentBaselineStore()
    {
        return new MemoryManualGarnishmentBaselineStore(
            new Dictionary<string, ManualGarnishmentBaseline>(),
            new List<string>());
    }

    public static IManualGarnishmentBaselineStore NewCachedManualGarnishmentBaselineStore(MemoryManualGarnishmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedManualGarnishmentBaselineStore(
            inner,
            ManualGarnishmentBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualGarnishmentBaseline> FindManualGarnishmentBaselines(IManualGarnishmentBaselineStore store, ManualGarnishmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
