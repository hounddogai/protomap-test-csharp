namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted payslip drafts by code.</summary>
public interface IWeightedPayslipDraftStore
{
    WeightedPayslipDraft? Find(string code);
    void Save(WeightedPayslipDraft item);
    bool Remove(string code);
    List<WeightedPayslipDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPayslipDraftStore(Dictionary<string, WeightedPayslipDraft> items, List<string> order) : IWeightedPayslipDraftStore
{
    public Dictionary<string, WeightedPayslipDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPayslipDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPayslipDraft item)
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

    public List<WeightedPayslipDraft> List()
    {
        var outValue = new List<WeightedPayslipDraft>();
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
public sealed class CachedWeightedPayslipDraftStore(MemoryWeightedPayslipDraftStore inner, WeightedPayslipDraftCache cache, int hits, int misses) : IWeightedPayslipDraftStore
{
    public MemoryWeightedPayslipDraftStore Inner { get; set; } = inner;
    public WeightedPayslipDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPayslipDraft? Find(string code)
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

    public void Save(WeightedPayslipDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPayslipDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted payslip drafts. Empty fields match all.</summary>
public sealed record WeightedPayslipDraftQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPayslipDraft item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPayslipDraft> Apply(List<WeightedPayslipDraft> items)
    {
        var matched = Collections.Filter(items, (WeightedPayslipDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted payslip draft store.</summary>
public static class WeightedPayslipDraftStore
{
    public static IWeightedPayslipDraftStore NewMemoryWeightedPayslipDraftStore()
    {
        return new MemoryWeightedPayslipDraftStore(
            new Dictionary<string, WeightedPayslipDraft>(),
            new List<string>());
    }

    public static IWeightedPayslipDraftStore NewCachedWeightedPayslipDraftStore(MemoryWeightedPayslipDraftStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPayslipDraftStore(inner, WeightedPayslipDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedPayslipDraft> FindWeightedPayslipDrafts(IWeightedPayslipDraftStore store, WeightedPayslipDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
