namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip templates by code.</summary>
public interface IPayslipTemplateStore
{
    PayslipTemplate? Find(string code);
    void Save(PayslipTemplate item);
    bool Remove(string code);
    List<PayslipTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipTemplateStore(Dictionary<string, PayslipTemplate> items, List<string> order) : IPayslipTemplateStore
{
    public Dictionary<string, PayslipTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipTemplate item)
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

    public List<PayslipTemplate> List()
    {
        var outValue = new List<PayslipTemplate>();
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
public sealed class CachedPayslipTemplateStore(MemoryPayslipTemplateStore inner, PayslipTemplateCache cache, int hits, int misses) : IPayslipTemplateStore
{
    public MemoryPayslipTemplateStore Inner { get; set; } = inner;
    public PayslipTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipTemplate? Find(string code)
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

    public void Save(PayslipTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip templates. Empty fields match all.</summary>
public sealed record PayslipTemplateQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipTemplate> Apply(List<PayslipTemplate> items)
    {
        var matched = Collections.Filter(items, (PayslipTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip template store.</summary>
public static class PayslipTemplateStore
{
    public static IPayslipTemplateStore NewMemoryPayslipTemplateStore()
    {
        return new MemoryPayslipTemplateStore(new Dictionary<string, PayslipTemplate>(), new List<string>());
    }

    public static IPayslipTemplateStore NewCachedPayslipTemplateStore(MemoryPayslipTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipTemplateStore(inner, PayslipTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipTemplate> FindPayslipTemplates(IPayslipTemplateStore store, PayslipTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
