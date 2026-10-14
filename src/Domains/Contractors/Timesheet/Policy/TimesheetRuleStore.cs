namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet rules by code.</summary>
public interface ITimesheetRuleStore
{
    TimesheetRule? Find(string code);
    void Save(TimesheetRule item);
    bool Remove(string code);
    List<TimesheetRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetRuleStore(Dictionary<string, TimesheetRule> items, List<string> order) : ITimesheetRuleStore
{
    public Dictionary<string, TimesheetRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetRule item)
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

    public List<TimesheetRule> List()
    {
        var outValue = new List<TimesheetRule>();
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
public sealed class CachedTimesheetRuleStore(MemoryTimesheetRuleStore inner, TimesheetRuleCache cache, int hits, int misses) : ITimesheetRuleStore
{
    public MemoryTimesheetRuleStore Inner { get; set; } = inner;
    public TimesheetRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetRule? Find(string code)
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

    public void Save(TimesheetRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet rules. Empty fields match all.</summary>
public sealed record TimesheetRuleQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetRule> Apply(List<TimesheetRule> items)
    {
        var matched = Collections.Filter(items, (TimesheetRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet rule store.</summary>
public static class TimesheetRuleStore
{
    public static ITimesheetRuleStore NewMemoryTimesheetRuleStore()
    {
        return new MemoryTimesheetRuleStore(new Dictionary<string, TimesheetRule>(), new List<string>());
    }

    public static ITimesheetRuleStore NewCachedTimesheetRuleStore(MemoryTimesheetRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetRuleStore(inner, TimesheetRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetRule> FindTimesheetRules(ITimesheetRuleStore store, TimesheetRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
