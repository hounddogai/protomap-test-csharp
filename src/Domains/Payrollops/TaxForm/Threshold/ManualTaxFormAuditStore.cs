namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual tax form audits by code.</summary>
public interface IManualTaxFormAuditStore
{
    ManualTaxFormAudit? Find(string code);
    void Save(ManualTaxFormAudit item);
    bool Remove(string code);
    List<ManualTaxFormAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualTaxFormAuditStore(Dictionary<string, ManualTaxFormAudit> items, List<string> order) : IManualTaxFormAuditStore
{
    public Dictionary<string, ManualTaxFormAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualTaxFormAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualTaxFormAudit item)
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

    public List<ManualTaxFormAudit> List()
    {
        var outValue = new List<ManualTaxFormAudit>();
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
public sealed class CachedManualTaxFormAuditStore(MemoryManualTaxFormAuditStore inner, ManualTaxFormAuditCache cache, int hits, int misses) : IManualTaxFormAuditStore
{
    public MemoryManualTaxFormAuditStore Inner { get; set; } = inner;
    public ManualTaxFormAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualTaxFormAudit? Find(string code)
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

    public void Save(ManualTaxFormAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualTaxFormAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual tax form audits. Empty fields match all.</summary>
public sealed record ManualTaxFormAuditQuery(string Channel, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ManualTaxFormAudit item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualTaxFormAudit> Apply(List<ManualTaxFormAudit> items)
    {
        var matched = Collections.Filter(items, (ManualTaxFormAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual tax form audit store.</summary>
public static class ManualTaxFormAuditStore
{
    public static IManualTaxFormAuditStore NewMemoryManualTaxFormAuditStore()
    {
        return new MemoryManualTaxFormAuditStore(new Dictionary<string, ManualTaxFormAudit>(), new List<string>());
    }

    public static IManualTaxFormAuditStore NewCachedManualTaxFormAuditStore(MemoryManualTaxFormAuditStore inner, int ttl, IClock clock)
    {
        return new CachedManualTaxFormAuditStore(inner, ManualTaxFormAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualTaxFormAudit> FindManualTaxFormAudits(IManualTaxFormAuditStore store, ManualTaxFormAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
