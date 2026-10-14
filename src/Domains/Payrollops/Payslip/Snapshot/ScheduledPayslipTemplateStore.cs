namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled payslip templates by code.</summary>
public interface IScheduledPayslipTemplateStore
{
    ScheduledPayslipTemplate? Find(string code);
    void Save(ScheduledPayslipTemplate item);
    bool Remove(string code);
    List<ScheduledPayslipTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayslipTemplateStore(Dictionary<string, ScheduledPayslipTemplate> items, List<string> order) : IScheduledPayslipTemplateStore
{
    public Dictionary<string, ScheduledPayslipTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayslipTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayslipTemplate item)
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

    public List<ScheduledPayslipTemplate> List()
    {
        var outValue = new List<ScheduledPayslipTemplate>();
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
public sealed class CachedScheduledPayslipTemplateStore(MemoryScheduledPayslipTemplateStore inner, ScheduledPayslipTemplateCache cache, int hits, int misses) : IScheduledPayslipTemplateStore
{
    public MemoryScheduledPayslipTemplateStore Inner { get; set; } = inner;
    public ScheduledPayslipTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayslipTemplate? Find(string code)
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

    public void Save(ScheduledPayslipTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayslipTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled payslip templates. Empty fields match all.</summary>
public sealed record ScheduledPayslipTemplateQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayslipTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayslipTemplate> Apply(List<ScheduledPayslipTemplate> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayslipTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled payslip template store.</summary>
public static class ScheduledPayslipTemplateStore
{
    public static IScheduledPayslipTemplateStore NewMemoryScheduledPayslipTemplateStore()
    {
        return new MemoryScheduledPayslipTemplateStore(
            new Dictionary<string, ScheduledPayslipTemplate>(),
            new List<string>());
    }

    public static IScheduledPayslipTemplateStore NewCachedScheduledPayslipTemplateStore(MemoryScheduledPayslipTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayslipTemplateStore(
            inner,
            ScheduledPayslipTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPayslipTemplate> FindScheduledPayslipTemplates(IScheduledPayslipTemplateStore store, ScheduledPayslipTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
