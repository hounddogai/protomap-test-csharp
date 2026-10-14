namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default payslip mappings by code.</summary>
public interface IDefaultPayslipMappingStore
{
    DefaultPayslipMapping? Find(string code);
    void Save(DefaultPayslipMapping item);
    bool Remove(string code);
    List<DefaultPayslipMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayslipMappingStore(Dictionary<string, DefaultPayslipMapping> items, List<string> order) : IDefaultPayslipMappingStore
{
    public Dictionary<string, DefaultPayslipMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayslipMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayslipMapping item)
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

    public List<DefaultPayslipMapping> List()
    {
        var outValue = new List<DefaultPayslipMapping>();
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
public sealed class CachedDefaultPayslipMappingStore(MemoryDefaultPayslipMappingStore inner, DefaultPayslipMappingCache cache, int hits, int misses) : IDefaultPayslipMappingStore
{
    public MemoryDefaultPayslipMappingStore Inner { get; set; } = inner;
    public DefaultPayslipMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayslipMapping? Find(string code)
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

    public void Save(DefaultPayslipMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayslipMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default payslip mappings. Empty fields match all.</summary>
public sealed record DefaultPayslipMappingQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayslipMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPayslipMapping> Apply(List<DefaultPayslipMapping> items)
    {
        var matched = Collections.Filter(items, (DefaultPayslipMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default payslip mapping store.</summary>
public static class DefaultPayslipMappingStore
{
    public static IDefaultPayslipMappingStore NewMemoryDefaultPayslipMappingStore()
    {
        return new MemoryDefaultPayslipMappingStore(
            new Dictionary<string, DefaultPayslipMapping>(),
            new List<string>());
    }

    public static IDefaultPayslipMappingStore NewCachedDefaultPayslipMappingStore(MemoryDefaultPayslipMappingStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayslipMappingStore(inner, DefaultPayslipMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultPayslipMapping> FindDefaultPayslipMappings(IDefaultPayslipMappingStore store, DefaultPayslipMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
