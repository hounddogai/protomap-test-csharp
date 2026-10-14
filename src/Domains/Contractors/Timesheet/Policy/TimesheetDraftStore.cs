namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet drafts by code.</summary>
public interface ITimesheetDraftStore
{
    TimesheetDraft? Find(string code);
    void Save(TimesheetDraft item);
    bool Remove(string code);
    List<TimesheetDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetDraftStore(Dictionary<string, TimesheetDraft> items, List<string> order) : ITimesheetDraftStore
{
    public Dictionary<string, TimesheetDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetDraft item)
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

    public List<TimesheetDraft> List()
    {
        var outValue = new List<TimesheetDraft>();
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
public sealed class CachedTimesheetDraftStore(MemoryTimesheetDraftStore inner, TimesheetDraftCache cache, int hits, int misses) : ITimesheetDraftStore
{
    public MemoryTimesheetDraftStore Inner { get; set; } = inner;
    public TimesheetDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetDraft? Find(string code)
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

    public void Save(TimesheetDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet drafts. Empty fields match all.</summary>
public sealed record TimesheetDraftQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetDraft item)
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

    public List<TimesheetDraft> Apply(List<TimesheetDraft> items)
    {
        var matched = Collections.Filter(items, (TimesheetDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet draft store.</summary>
public static class TimesheetDraftStore
{
    public static ITimesheetDraftStore NewMemoryTimesheetDraftStore()
    {
        return new MemoryTimesheetDraftStore(new Dictionary<string, TimesheetDraft>(), new List<string>());
    }

    public static ITimesheetDraftStore NewCachedTimesheetDraftStore(MemoryTimesheetDraftStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetDraftStore(inner, TimesheetDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetDraft> FindTimesheetDrafts(ITimesheetDraftStore store, TimesheetDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
