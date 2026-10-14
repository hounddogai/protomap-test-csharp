namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet agreements by code.</summary>
public interface ITimesheetAgreementStore
{
    TimesheetAgreement? Find(string code);
    void Save(TimesheetAgreement item);
    bool Remove(string code);
    List<TimesheetAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetAgreementStore(Dictionary<string, TimesheetAgreement> items, List<string> order) : ITimesheetAgreementStore
{
    public Dictionary<string, TimesheetAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetAgreement item)
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

    public List<TimesheetAgreement> List()
    {
        var outValue = new List<TimesheetAgreement>();
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
public sealed class CachedTimesheetAgreementStore(MemoryTimesheetAgreementStore inner, TimesheetAgreementCache cache, int hits, int misses) : ITimesheetAgreementStore
{
    public MemoryTimesheetAgreementStore Inner { get; set; } = inner;
    public TimesheetAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetAgreement? Find(string code)
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

    public void Save(TimesheetAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet agreements. Empty fields match all.</summary>
public sealed record TimesheetAgreementQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetAgreement> Apply(List<TimesheetAgreement> items)
    {
        var matched = Collections.Filter(items, (TimesheetAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet agreement store.</summary>
public static class TimesheetAgreementStore
{
    public static ITimesheetAgreementStore NewMemoryTimesheetAgreementStore()
    {
        return new MemoryTimesheetAgreementStore(new Dictionary<string, TimesheetAgreement>(), new List<string>());
    }

    public static ITimesheetAgreementStore NewCachedTimesheetAgreementStore(MemoryTimesheetAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetAgreementStore(inner, TimesheetAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TimesheetAgreement> FindTimesheetAgreements(ITimesheetAgreementStore store, TimesheetAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
