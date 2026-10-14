namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict tax form audits by code.</summary>
public interface IStrictTaxFormAuditStore
{
    StrictTaxFormAudit? Find(string code);
    void Save(StrictTaxFormAudit item);
    bool Remove(string code);
    List<StrictTaxFormAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictTaxFormAuditStore(Dictionary<string, StrictTaxFormAudit> items, List<string> order) : IStrictTaxFormAuditStore
{
    public Dictionary<string, StrictTaxFormAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictTaxFormAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictTaxFormAudit item)
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

    public List<StrictTaxFormAudit> List()
    {
        var outValue = new List<StrictTaxFormAudit>();
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
public sealed class CachedStrictTaxFormAuditStore(MemoryStrictTaxFormAuditStore inner, StrictTaxFormAuditCache cache, int hits, int misses) : IStrictTaxFormAuditStore
{
    public MemoryStrictTaxFormAuditStore Inner { get; set; } = inner;
    public StrictTaxFormAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictTaxFormAudit? Find(string code)
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

    public void Save(StrictTaxFormAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictTaxFormAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict tax form audits. Empty fields match all.</summary>
public sealed record StrictTaxFormAuditQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(StrictTaxFormAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<StrictTaxFormAudit> Apply(List<StrictTaxFormAudit> items)
    {
        var matched = Collections.Filter(items, (StrictTaxFormAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict tax form audit store.</summary>
public static class StrictTaxFormAuditStore
{
    public static IStrictTaxFormAuditStore NewMemoryStrictTaxFormAuditStore()
    {
        return new MemoryStrictTaxFormAuditStore(new Dictionary<string, StrictTaxFormAudit>(), new List<string>());
    }

    public static IStrictTaxFormAuditStore NewCachedStrictTaxFormAuditStore(MemoryStrictTaxFormAuditStore inner, int ttl, IClock clock)
    {
        return new CachedStrictTaxFormAuditStore(inner, StrictTaxFormAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictTaxFormAudit> FindStrictTaxFormAudits(IStrictTaxFormAuditStore store, StrictTaxFormAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
