namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional timesheet audits by code.</summary>
public interface IRegionalTimesheetAuditStore
{
    RegionalTimesheetAudit? Find(string code);
    void Save(RegionalTimesheetAudit item);
    bool Remove(string code);
    List<RegionalTimesheetAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTimesheetAuditStore(Dictionary<string, RegionalTimesheetAudit> items, List<string> order) : IRegionalTimesheetAuditStore
{
    public Dictionary<string, RegionalTimesheetAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTimesheetAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTimesheetAudit item)
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

    public List<RegionalTimesheetAudit> List()
    {
        var outValue = new List<RegionalTimesheetAudit>();
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
public sealed class CachedRegionalTimesheetAuditStore(MemoryRegionalTimesheetAuditStore inner, RegionalTimesheetAuditCache cache, int hits, int misses) : IRegionalTimesheetAuditStore
{
    public MemoryRegionalTimesheetAuditStore Inner { get; set; } = inner;
    public RegionalTimesheetAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTimesheetAudit? Find(string code)
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

    public void Save(RegionalTimesheetAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTimesheetAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional timesheet audits. Empty fields match all.</summary>
public sealed record RegionalTimesheetAuditQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTimesheetAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTimesheetAudit> Apply(List<RegionalTimesheetAudit> items)
    {
        var matched = Collections.Filter(items, (RegionalTimesheetAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional timesheet audit store.</summary>
public static class RegionalTimesheetAuditStore
{
    public static IRegionalTimesheetAuditStore NewMemoryRegionalTimesheetAuditStore()
    {
        return new MemoryRegionalTimesheetAuditStore(
            new Dictionary<string, RegionalTimesheetAudit>(),
            new List<string>());
    }

    public static IRegionalTimesheetAuditStore NewCachedRegionalTimesheetAuditStore(MemoryRegionalTimesheetAuditStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTimesheetAuditStore(
            inner,
            RegionalTimesheetAuditCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTimesheetAudit> FindRegionalTimesheetAudits(IRegionalTimesheetAuditStore store, RegionalTimesheetAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
