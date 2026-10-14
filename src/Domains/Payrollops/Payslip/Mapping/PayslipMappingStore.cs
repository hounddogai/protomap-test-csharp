namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip mappings by code.</summary>
public interface IPayslipMappingStore
{
    PayslipMapping? Find(string code);
    void Save(PayslipMapping item);
    bool Remove(string code);
    List<PayslipMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipMappingStore(Dictionary<string, PayslipMapping> items, List<string> order) : IPayslipMappingStore
{
    public Dictionary<string, PayslipMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipMapping item)
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

    public List<PayslipMapping> List()
    {
        var outValue = new List<PayslipMapping>();
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
public sealed class CachedPayslipMappingStore(MemoryPayslipMappingStore inner, PayslipMappingCache cache, int hits, int misses) : IPayslipMappingStore
{
    public MemoryPayslipMappingStore Inner { get; set; } = inner;
    public PayslipMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipMapping? Find(string code)
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

    public void Save(PayslipMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip mappings. Empty fields match all.</summary>
public sealed record PayslipMappingQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PayslipMapping> Apply(List<PayslipMapping> items)
    {
        var matched = Collections.Filter(items, (PayslipMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip mapping store.</summary>
public static class PayslipMappingStore
{
    public static IPayslipMappingStore NewMemoryPayslipMappingStore()
    {
        return new MemoryPayslipMappingStore(new Dictionary<string, PayslipMapping>(), new List<string>());
    }

    public static IPayslipMappingStore NewCachedPayslipMappingStore(MemoryPayslipMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipMappingStore(inner, PayslipMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipMapping> FindPayslipMappings(IPayslipMappingStore store, PayslipMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
