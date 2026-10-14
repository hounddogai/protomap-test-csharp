namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal payslip baselines by code.</summary>
public interface ISeasonalPayslipBaselineStore
{
    SeasonalPayslipBaseline? Find(string code);
    void Save(SeasonalPayslipBaseline item);
    bool Remove(string code);
    List<SeasonalPayslipBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayslipBaselineStore(Dictionary<string, SeasonalPayslipBaseline> items, List<string> order) : ISeasonalPayslipBaselineStore
{
    public Dictionary<string, SeasonalPayslipBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayslipBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayslipBaseline item)
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

    public List<SeasonalPayslipBaseline> List()
    {
        var outValue = new List<SeasonalPayslipBaseline>();
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
public sealed class CachedSeasonalPayslipBaselineStore(MemorySeasonalPayslipBaselineStore inner, SeasonalPayslipBaselineCache cache, int hits, int misses) : ISeasonalPayslipBaselineStore
{
    public MemorySeasonalPayslipBaselineStore Inner { get; set; } = inner;
    public SeasonalPayslipBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayslipBaseline? Find(string code)
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

    public void Save(SeasonalPayslipBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayslipBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal payslip baselines. Empty fields match all.</summary>
public sealed record SeasonalPayslipBaselineQuery(string Department, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayslipBaseline item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPayslipBaseline> Apply(List<SeasonalPayslipBaseline> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayslipBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal payslip baseline store.</summary>
public static class SeasonalPayslipBaselineStore
{
    public static ISeasonalPayslipBaselineStore NewMemorySeasonalPayslipBaselineStore()
    {
        return new MemorySeasonalPayslipBaselineStore(
            new Dictionary<string, SeasonalPayslipBaseline>(),
            new List<string>());
    }

    public static ISeasonalPayslipBaselineStore NewCachedSeasonalPayslipBaselineStore(MemorySeasonalPayslipBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayslipBaselineStore(
            inner,
            SeasonalPayslipBaselineCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPayslipBaseline> FindSeasonalPayslipBaselines(ISeasonalPayslipBaselineStore store, SeasonalPayslipBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
